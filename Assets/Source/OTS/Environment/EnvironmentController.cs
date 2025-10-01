using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using OTS.Common;

namespace OTS.Scripts.Environment
{
    /// <summary>
    ///     Environment controller that calls layer transitions.
    /// 
    ///     Depends on the TimeOfDayManager to be present in the scene.
    /// </summary>
    public class EnvironmentController : MonoBehaviour, ILoggable
    {
        [Header("Environment Configuration")]
        [SerializeField] private bool autoFindLayers = true;

        [Header("Layer Management")]
        [SerializeField] private GameObject environmentBase;
        [SerializeField] private List<EnvironmentLayer> environmentLayers = new List<EnvironmentLayer>();
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool useDebugControls = true;
        
        // Current state
        private TimeOfDay currentTime = TimeOfDay.Morning;
        private Dictionary<EnvironmentLayerType, EnvironmentLayer> layerDictionary;
        
        void Awake()
        {
            if (autoFindLayers)
            {
                FindEnvironmentLayers();
            }
            
            InitializeLayerDictionary();
        }
        
        void Start()
        {
            if (environmentBase != null)
            {
                environmentBase.SetActive(true);
            }

            // Listen to TimeOfDayManager if it exists
            if (TimeOfDayManager.Instance != null)
            {
                TimeOfDayManager.OnTimeOfDayChanged += HandleTimeChanged;
                SetTimeInstant(TimeOfDayManager.Instance.CurrentTimeOfDay);
            }
            else
            {
                // Set initial time
                SetTimeInstant(TimeOfDay.Morning);
            }
        }
        
        void OnDestroy()
        {
            if (TimeOfDayManager.Instance != null)
            {
                TimeOfDayManager.OnTimeOfDayChanged -= HandleTimeChanged;
            }
        }
        
        /// <summary>
        ///     Handle time changes from TimeOfDayManager
        /// </summary>
        void HandleTimeChanged(TimeOfDay oldTime, TimeOfDay newTime)
        {
            TransitionToTime(newTime);
        }
        
        void Update()
        {
            if (!useDebugControls) return;
            
            // Debug controls
            if (Input.GetKeyDown(KeyCode.Alpha1))
                TransitionToTime(TimeOfDay.Morning);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                TransitionToTime(TimeOfDay.Afternoon);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                TransitionToTime(TimeOfDay.Night);
        }
        
        /// <summary>
        ///     Transition all layers to a new time
        /// </summary>
        public void TransitionToTime(TimeOfDay newTime)
        {
            if (currentTime == newTime) return;
            
            if (enableDebugLogs)
                this.Log($"Transitioning all layers from {currentTime} to {newTime}");
            
            currentTime = newTime;
            
            // Tell all layers to transition
            foreach (var layer in environmentLayers)
            {
                if (layer != null)
                    layer.TransitionToTime(newTime);
            }
        }
        
        /// <summary>
        ///     Set all layers to a time instantly
        /// </summary>
        public void SetTimeInstant(TimeOfDay time)
        {
            if (enableDebugLogs)
                this.Log($"Setting all layers to {time} instantly");
            
            currentTime = time;
            
            foreach (var layer in environmentLayers)
            {
                if (layer != null)
                    layer.SetTimeInstant(time);
            }
        }
        
        /// <summary>
        ///     Get a specific environment layer by type
        /// </summary>
        public EnvironmentLayer GetLayer(EnvironmentLayerType layerType)
        {
            if (layerDictionary != null && layerDictionary.TryGetValue(layerType, out EnvironmentLayer layer))
            {
                return layer;
            }
            return null;
        }
        
        /// <summary>
        ///     Check if a specific layer type exists
        /// </summary>
        public bool HasLayer(EnvironmentLayerType layerType)
        {
            return layerDictionary?.ContainsKey(layerType) ?? false;
        }
        
        /// <summary>
        ///     Enable or disable a specific layer
        /// </summary>
        public void SetLayerActive(EnvironmentLayerType layerType, bool active)
        {
            var layer = GetLayer(layerType);
            if (layer != null)
            {
                layer.gameObject.SetActive(active);
            }
        }
        
        // Debug methods
        [ContextMenu("Debug: Morning")]
        public void DebugMorning() => TransitionToTime(TimeOfDay.Morning);
        
        [ContextMenu("Debug: Afternoon")]  
        public void DebugAfternoon() => TransitionToTime(TimeOfDay.Afternoon);
        
        [ContextMenu("Debug: Night")]
        public void DebugNight() => TransitionToTime(TimeOfDay.Night);
        
        [ContextMenu("Show Status")]
        public void ShowStatus()
        {
            this.Log($"=== Environment Controller Status ===");
            this.Log($"Current Time: {currentTime}");
            this.Log($"Active Layers: {environmentLayers.Count}");
            
            foreach (var layer in environmentLayers)
            {
                if (layer != null)
                    this.Log($"  - {layer.layerType}: {layer.gameObject.name}");
            }
        }
        
        /// <summary>
        ///     Automatically find environment layers in children
        /// </summary>
        private void FindEnvironmentLayers()
        {
            environmentLayers.Clear();
            var foundLayers = GetComponentsInChildren<EnvironmentLayer>(true);
            environmentLayers.AddRange(foundLayers);
            
            if (enableDebugLogs)
                this.Log($"Auto-found {foundLayers.Length} environment layers");
        }
        
        /// <summary>
        ///     Initialize the layer dictionary for fast lookups
        /// </summary>
        private void InitializeLayerDictionary()
        {
            layerDictionary = new Dictionary<EnvironmentLayerType, EnvironmentLayer>();
            
            foreach (var layer in environmentLayers)
            {
                if (layer != null)
                {
                    if (layerDictionary.ContainsKey(layer.layerType))
                    {
                        this.LogWarning($"Duplicate layer type found: {layer.layerType.GetName()}");
                    }
                    else
                    {
                        layerDictionary[layer.layerType] = layer;
                    }
                }
            }
        }
    }
}
