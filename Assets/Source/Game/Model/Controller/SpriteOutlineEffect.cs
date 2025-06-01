using UnityEngine;
using System.Collections.Generic;

namespace Game.Model.Controller
{
    /// <summary>
    ///     Applies outline effect to all sprite renderers in the game object hierarchy.
    ///     Perfect for character sprites with clothing and facial features.
    /// </summary>
    public class SpriteOutlineEffect : MonoBehaviour, ILoggable
    {
        [Header("Outline Settings")]
        [SerializeField] private float outlineSize = 2f;
        [SerializeField] private float blurStrength = 1f;
        [SerializeField] private float outlineAlpha = 0.8f;
        
        [Header("Material Management")]
        [SerializeField] private Shader outlineShader;
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool includeChildren = true;
        
        private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        private Dictionary<SpriteRenderer, Material> originalMaterials = new Dictionary<SpriteRenderer, Material>();
        private Dictionary<SpriteRenderer, Material> outlineMaterials = new Dictionary<SpriteRenderer, Material>();
        
        private void Start()
        {
            if (outlineShader == null)
            {
                outlineShader = Shader.Find("Custom/SpriteOutline");
                if (outlineShader == null)
                {
                    this.LogError("Could not find Custom/SpriteOutline shader! Make sure the shader is compiled.");
                    return;
                }
            }
            
            if (applyOnStart)
            {
                ApplyOutlineEffect();
            }
        }
        
        /// <summary>
        ///     Apply outline effect to all sprite renderers
        /// </summary>
        [ContextMenu("Apply Outline Effect")]
        public void ApplyOutlineEffect()
        {
            CollectSpriteRenderers();
            CreateOutlineMaterials();
            ApplyMaterials();
            
            this.Log($"Applied outline effect to {spriteRenderers.Count} sprite renderers");
        }
        
        /// <summary>
        ///     Remove outline effect and restore original materials
        /// </summary>
        [ContextMenu("Remove Outline Effect")]
        public void RemoveOutlineEffect()
        {
            RestoreOriginalMaterials();
            CleanupMaterials();
            
            this.Log($"Removed outline effect from {spriteRenderers.Count} sprite renderers");
        }
        
        /// <summary>
        ///     Update outline properties on existing materials
        /// </summary>
        [ContextMenu("Update Outline Properties")]
        public void UpdateOutlineProperties()
        {
            foreach (var material in outlineMaterials.Values)
            {
                if (material != null)
                {
                    material.SetFloat("_OutlineSize", outlineSize);
                    material.SetFloat("_BlurStrength", blurStrength);
                    material.SetFloat("_OutlineAlpha", outlineAlpha);
                }
            }
            
            this.Log("Updated outline properties on all materials");
        }
        
        /// <summary>
        ///     Collect all sprite renderers in hierarchy
        /// </summary>
        private void CollectSpriteRenderers()
        {
            spriteRenderers.Clear();
            
            if (includeChildren)
            {
                // Get all sprite renderers in children
                SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
                spriteRenderers.AddRange(renderers);
            }
            else
            {
                // Get only sprite renderer on this game object
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    spriteRenderers.Add(renderer);
                }
            }
        }
        
        /// <summary>
        ///     Create outline materials for each sprite renderer
        /// </summary>
        private void CreateOutlineMaterials()
        {
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                if (renderer == null) continue;
                
                // Store original material
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material;
                }
                
                // Create outline material
                Material outlineMaterial = new Material(outlineShader);
                outlineMaterial.name = $"OutlineMaterial_{renderer.gameObject.name}";
                
                // Set outline properties
                outlineMaterial.SetFloat("_OutlineSize", outlineSize);
                outlineMaterial.SetFloat("_BlurStrength", blurStrength);
                outlineMaterial.SetFloat("_OutlineAlpha", outlineAlpha);
                
                // Copy main texture and color from original material
                if (originalMaterials[renderer] != null)
                {
                    if (originalMaterials[renderer].HasProperty("_MainTex"))
                    {
                        outlineMaterial.SetTexture("_MainTex", originalMaterials[renderer].GetTexture("_MainTex"));
                    }
                    if (originalMaterials[renderer].HasProperty("_Color"))
                    {
                        outlineMaterial.SetColor("_Color", originalMaterials[renderer].GetColor("_Color"));
                    }
                }
                
                outlineMaterials[renderer] = outlineMaterial;
            }
        }
        
        /// <summary>
        ///     Apply outline materials to sprite renderers
        /// </summary>
        private void ApplyMaterials()
        {
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                if (renderer != null && outlineMaterials.ContainsKey(renderer))
                {
                    renderer.material = outlineMaterials[renderer];
                }
            }
        }
        
        /// <summary>
        ///     Restore original materials
        /// </summary>
        private void RestoreOriginalMaterials()
        {
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                if (renderer != null && originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = originalMaterials[renderer];
                }
            }
        }
        
        /// <summary>
        ///     Clean up created materials
        /// </summary>
        private void CleanupMaterials()
        {
            foreach (var material in outlineMaterials.Values)
            {
                if (material != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(material);
                    }
                    else
                    {
                        DestroyImmediate(material);
                    }
                }
            }
            
            outlineMaterials.Clear();
        }
        
        /// <summary>
        ///     Add a new sprite renderer to the effect (useful for dynamically added clothing)
        /// </summary>
        public void AddSpriteRenderer(SpriteRenderer renderer)
        {
            if (renderer == null || spriteRenderers.Contains(renderer)) return;
            
            spriteRenderers.Add(renderer);
            
            // Store original material
            originalMaterials[renderer] = renderer.material;
            
            // Create and apply outline material
            Material outlineMaterial = new Material(outlineShader);
            outlineMaterial.name = $"OutlineMaterial_{renderer.gameObject.name}";
            outlineMaterial.SetFloat("_OutlineSize", outlineSize);
            outlineMaterial.SetFloat("_BlurStrength", blurStrength);
            outlineMaterial.SetFloat("_OutlineAlpha", outlineAlpha);
            
            // Copy properties from original material
            if (originalMaterials[renderer] != null)
            {
                if (originalMaterials[renderer].HasProperty("_MainTex"))
                {
                    outlineMaterial.SetTexture("_MainTex", originalMaterials[renderer].GetTexture("_MainTex"));
                }
                if (originalMaterials[renderer].HasProperty("_Color"))
                {
                    outlineMaterial.SetColor("_Color", originalMaterials[renderer].GetColor("_Color"));
                }
            }
            
            outlineMaterials[renderer] = outlineMaterial;
            renderer.material = outlineMaterial;
            
            this.Log($"Added outline effect to {renderer.gameObject.name}");
        }
        
        /// <summary>
        ///     Remove a sprite renderer from the effect
        /// </summary>
        public void RemoveSpriteRenderer(SpriteRenderer renderer)
        {
            if (renderer == null || !spriteRenderers.Contains(renderer)) return;
            
            // Restore original material
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.material = originalMaterials[renderer];
                originalMaterials.Remove(renderer);
            }
            
            // Clean up outline material
            if (outlineMaterials.ContainsKey(renderer))
            {
                if (outlineMaterials[renderer] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(outlineMaterials[renderer]);
                    }
                    else
                    {
                        DestroyImmediate(outlineMaterials[renderer]);
                    }
                }
                outlineMaterials.Remove(renderer);
            }
            
            spriteRenderers.Remove(renderer);
            this.Log($"Removed outline effect from {renderer.gameObject.name}");
        }
        
        private void OnDestroy()
        {
            CleanupMaterials();
        }
        
        private void OnValidate()
        {
            // Update properties when values change in inspector
            if (Application.isPlaying && outlineMaterials.Count > 0)
            {
                UpdateOutlineProperties();
            }
        }
    }
} 