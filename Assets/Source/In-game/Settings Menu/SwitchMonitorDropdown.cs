using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

public class SwitchMonitorDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown monitorDropdown;
    [SerializeField] private ResolutionDropdown resolutionDropdownComponent;
    private List<DisplayInfo> displayLayout;
    
    private void Start()
    {
        try
        {
            // Get available displays using the new API
            displayLayout = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displayLayout);
            
            if (displayLayout.Count == 0) {
                this.LogWarning("No displays found using GetDisplayLayout API. This might be unsupported on your platform.");
                gameObject.SetActive(false); // Hide this component if no displays are found
                return;
            }
            
            // Populate the dropdown with display info
            PopulateDropdown();
            
            this.Log($"Found {displayLayout.Count} displays");
            foreach (var display in displayLayout) {
                this.Log($"Display: {display.name}, {display.width}x{display.height}");
            }
        }
        catch (Exception e)
        {
            // Catch any exceptions during startup
            this.LogError($"Error initializing monitor dropdown: {e.Message}\n{e.StackTrace}");
            gameObject.SetActive(false); // Hide this component if it fails
        }
    }
    
    /// <summary>
    ///     Populate the dropdown with available display information.
    /// </summary>
    private void PopulateDropdown()
    {
        monitorDropdown.ClearOptions();
        List<string> optionStrings = new List<string>();

        // Create an option for each available monitor with its name
        for (int i = 0; i < displayLayout.Count; i++)
        {
            DisplayInfo display = displayLayout[i];
            string optionString = string.IsNullOrEmpty(display.name) 
                ? $"{i+1} (UNKNOWN)" 
                : $"{i+1}: {display.name}";
            
            optionStrings.Add(optionString);
        }

        monitorDropdown.AddOptions(optionStrings);
        monitorDropdown.value = 0; // Always starts at display 0 in Unity
        monitorDropdown.RefreshShownValue();

        // Simple direct handler
        monitorDropdown.onValueChanged.AddListener(SwitchToMonitor);
    }
    
    /// <summary>
    /// Switch to the selected monitor
    /// </summary>
    private void SwitchToMonitor(int index)
    {
        if (index >= displayLayout.Count)
        {
            this.LogError($"Monitor index {index+1} is out of range");
            return;
        }

        DisplayInfo display = displayLayout[index];
        this.Log($"Attempting to switch to monitor {index+1}: {display.name}");

        try 
        {
            // Step 1: Always force windowed mode first
            FullScreenMode currentMode = Screen.fullScreenMode;
            
            // Record current resolution settings to restore later
            int currentWidth  = Screen.width;
            int currentHeight = Screen.height;
            RefreshRate currentRefreshRate = Screen.currentResolution.refreshRateRatio;
            
            // Force to windowed at a slightly smaller size to ensure it's movable
            int tempWidth = Mathf.Min(currentWidth - 100, 1280);
            int tempHeight = Mathf.Min(currentHeight - 100, 720);
            
            this.Log($"Switching to windowed mode temporarily at {tempWidth}x{tempHeight}");
            Screen.SetResolution(tempWidth, tempHeight, FullScreenMode.Windowed);
            
            // Step 2: Wait a frame to let the windowed mode take effect
            StartCoroutine(CompleteMonitorSwitchSequence(index, display, currentMode, currentWidth, currentHeight, currentRefreshRate));
        }
        catch (Exception e)
        {
            this.LogError($"Error initiating monitor switch: {e.Message}");
        }
    }

    private System.Collections.IEnumerator CompleteMonitorSwitchSequence(
        int monitorIndex, 
        DisplayInfo targetDisplay, 
        FullScreenMode originalMode,
        int originalWidth,
        int originalHeight,
        RefreshRate originalRefreshRate)
    {
        // Wait for the window mode change to apply
        yield return new WaitForEndOfFrame();

        this.Log($"Moving window to monitor {monitorIndex+1}");
        
        // Move window to center of target monitor
        Vector2Int position = new Vector2Int(
            targetDisplay.width / 2 - Screen.width / 2,
            targetDisplay.height / 2 - Screen.height / 2
        );
        
        Screen.MoveMainWindowTo(targetDisplay, position);
        
        // Wait for move to complete
        yield return new WaitForEndOfFrame();
        
        // For fullscreen modes when moving to a higher resolution monitor,
        // automatically adjust to the new monitor's native resolution
        if (originalMode != FullScreenMode.Windowed && 
            (targetDisplay.width > originalWidth || targetDisplay.height > originalHeight))
        {
            this.Log($"Switching to higher resolution display in fullscreen mode - adjusting to native resolution: {targetDisplay.width}x{targetDisplay.height}");
            
            // Use the target display's native resolution
            Screen.SetResolution(
                targetDisplay.width, 
                targetDisplay.height, 
                originalMode, 
                originalRefreshRate
            );
        }
        else
        {
            // Otherwise, restore original mode and resolution
            this.Log($"Restoring mode: {originalMode} and resolution: {originalWidth}x{originalHeight}");
            Screen.SetResolution(originalWidth, originalHeight, originalMode, originalRefreshRate);
        }
        
        // Let the system catch up
        yield return new WaitForSeconds(0.2f);
        
        // Update resolution dropdown to match the new monitor options
        if (resolutionDropdownComponent != null)
        {
            resolutionDropdownComponent.UpdateToMatchCurrentResolution();
        }
        
        this.Log($"Monitor switch to {monitorIndex+1} complete");
    }

    // New method to find the best resolution for a display
    private Resolution FindBestResolutionForDisplay(DisplayInfo display)
    {
        // Start with the display's native resolution
        int targetWidth = display.width;
        int targetHeight = display.height;
        
        // Get all available resolutions
        Resolution[] allResolutions = Screen.resolutions;
        
        // First try to find exact match with best refresh rate
        Resolution bestMatch = new Resolution 
        { 
            width = targetWidth, 
            height = targetHeight,
            refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 }
        };
        
        float bestRefreshRate = 0f;
        
        foreach (Resolution res in allResolutions)
        {
            // If this resolution matches the target dimensions
            if (res.width == targetWidth && res.height == targetHeight)
            {
                // Check if it has a better refresh rate
                float refreshRate = (float) res.refreshRateRatio.value;
                if (refreshRate > bestRefreshRate)
                {
                    bestMatch = res;
                    bestRefreshRate = refreshRate;
                }
            }
        }
        
        // If no exact match found, find the closest resolution with 16:9 aspect ratio
        if (bestRefreshRate == 0f)
        {
            int bestMatchScore = int.MaxValue;
            float targetAspectRatio = 16f / 9f;
            
            foreach (Resolution res in allResolutions)
            {
                float aspectRatio = (float)res.width / res.height;
                
                // Check if this is a 16:9 aspect ratio
                if (Mathf.Abs(aspectRatio - targetAspectRatio) < 0.01f)
                {
                    // Calculate how close this is to target resolution
                    int score = Mathf.Abs(res.width - targetWidth) + Mathf.Abs(res.height - targetHeight);
                    float refreshRate = (float) res.refreshRateRatio.value;  // Explicit cast
                    
                    if (score < bestMatchScore)
                    {
                        bestMatch = res;
                        bestMatchScore = score;
                        bestRefreshRate = refreshRate;
                    }
                    else if (score == bestMatchScore && refreshRate > bestRefreshRate)
                    {
                        // Same score but better refresh rate
                        bestMatch = res;
                        bestRefreshRate = refreshRate;
                    }
                }
            }
        }
        
        return bestMatch;
    }
}