using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

/// <summary>
///     Rotates an arrow image when a dropdown is opened or closed.
///     
///     Attach this to the GameObject containing the TMP_Dropdown component.
///     This script detects the dropdown state by checking for the "Dropdown List" GameObject
///     that Unity creates when the dropdown is open.
///     
///     I'm not sure if this is the best way to do this, but shit isn't in them documentation
///     nor is it fucking obvious to implement in the editor.
///     
///     So fuck you.
/// </summary>
public class DropdownArrowListener : MonoBehaviour, ILoggable
{
    [Header("Arrow Settings")]
    [Tooltip("The RectTransform of the arrow image that will rotate")]
    [SerializeField] private RectTransform arrowTransform;
    
    [Tooltip("Rotation angle (in degrees) when dropdown is open")]
    [SerializeField] private float openRotation = 90f;
    
    [Tooltip("Rotation angle (in degrees) when dropdown is closed")]
    [SerializeField] private float closedRotation = 0f;
    
    [Tooltip("Duration of the rotation animation in seconds")]
    [SerializeField] private float rotationDuration = 0.2f;
    
    [Header("Dropdown Reference")]
    [Tooltip("Reference to the dropdown component (auto-detected if null)")]
    [SerializeField] private TMP_Dropdown dropdown;
    
    private bool isOpen = false;
    private Coroutine rotationCoroutine;

    /// <summary>
    ///     Initialize the component and set up event listeners.
    /// </summary>
    private void Start()
    {
        // Auto-find dropdown on this object if not assigned
        if (dropdown == null) {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        
        if (dropdown == null) {
            this.LogWarning("DropdownArrowListener: No TMP_Dropdown component found!");
            enabled = false;
            return;
        }
        
        // Initialize the arrow rotation to closed state
        if (arrowTransform != null) {
            arrowTransform.localEulerAngles = new Vector3(
                arrowTransform.localEulerAngles.x,
                arrowTransform.localEulerAngles.y,
                closedRotation
            );
        } else {
            this.LogWarning("DropdownArrowListener: Arrow Transform is not assigned!");
            enabled = false;
        }
    }
    
    /// <summary>
    ///     Check for the dropdown state by looking for the "Dropdown List" GameObject.
    /// </summary>
    private void Update()
    {
        // Check for the dynamically created "Dropdown List" GameObject
        Transform dropdownList = FindDropdownList();
        
        if (dropdownList != null)
        {
            // If a dropdown list exists in the scene, the dropdown is open
            if (!isOpen) {
                isOpen = true;
                RotateArrow(openRotation);
            }
        } else {
            // If no dropdown list exists, the dropdown is closed
            if (isOpen) {
                isOpen = false;
                RotateArrow(closedRotation);
            }
        }
    }
    
    /// <summary>
    ///     Find the dynamically created dropdown list in the scene.
    /// </summary>
    /// 
    /// <returns>
    ///     The transform of the dropdown list, or null if not found.
    /// </returns>
    private Transform FindDropdownList()
    {
        // The dropdown creates a child in the Canvas with this specific name.
        // Not sure if this is the best way to do this, but it works.
        Transform dropdownList = transform.Find("Dropdown List");
        return dropdownList;
    }
    
    /// <summary>
    ///     Rotate the arrow to the specified angle with animation.
    /// </summary>
    /// 
    /// <param name="targetRotation">
    ///     The target rotation angle in degrees.
    /// </param>
    private void RotateArrow(float targetRotation)
    {
        if (arrowTransform == null)
            return;
            
        // Stop existing animation if any
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
            
        // Start new animation
        rotationCoroutine = StartCoroutine(AnimateRotation(targetRotation));
    }
    
    /// <summary>
    ///     Animate the arrow rotation smoothly over time.
    /// </summary>
    /// 
    /// <param name="targetRotation">
    ///     The target rotation angle in degrees.
    /// </param>
    private IEnumerator AnimateRotation(float targetRotation)
    {
        if (arrowTransform == null)
            yield break;
            
        Vector3 startEuler = arrowTransform.localEulerAngles;
        float startZ = startEuler.z;
        
        // Calculate shortest rotation path
        float angleDifference = Mathf.DeltaAngle(startZ, targetRotation);
        
        float elapsedTime = 0;
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);
            
            // Use SmoothStep for easing
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Apply rotation
            float newZ = startZ + angleDifference * smoothT;
            arrowTransform.localEulerAngles = new Vector3(startEuler.x, startEuler.y, newZ);
            
            yield return null;
        }
        
        // Ensure we land exactly on target
        arrowTransform.localEulerAngles = new Vector3(
            startEuler.x, 
            startEuler.y,
            targetRotation
        );
    }
}