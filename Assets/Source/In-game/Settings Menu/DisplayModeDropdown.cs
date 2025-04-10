using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
///     This is a dropdown that allows the player to switch between display modes.
///     
///     I'm not sure whether to enable exclusive fullscreen or not because the game itself
///     is generally 2D without any heavy graphics work going on, so I doubt fullscreen will ever be necessary.
///     However... if some people do want it - say, experiencing performance issues - I might enable it in the future.
/// </summary>
public class DisplayModeDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private ResolutionDropdown resolutionDropdownComponent;
    
    // PlayerPrefs key
    private const string PREFS_DISPLAY_MODE = "DisplayMode";
    
    // List of display modes to offer in the dropdown
    private readonly FullScreenMode[] displayModes = new FullScreenMode[]
    {
        // FullScreenMode.ExclusiveFullScreen,    // Standard fullscreen
        FullScreenMode.FullScreenWindow,          // Borderless fullscreen
        FullScreenMode.Windowed                   // Windowed mode
    };
    
    // Human-readable names for each display mode
    private readonly string[] displayModeNames = new string[]
    {
        // "Fullscreen", full screen mode is basically unncessary. Borderless is better. Who actually prefers fullscreen?
        "Fullscreen", // this is actually borderless fullscreen
        "Windowed"
    };
    
    private void Start()
    {
        try
        {
            // Populate dropdown with display mode options
            PopulateDropdown();
            
            // Set initial selection based on saved preference or current display mode
            SetInitialSelection();
        }
        catch (Exception e)
        {
            this.LogError($"Error initializing display mode dropdown: {e.Message}");
            gameObject.SetActive(false); // Hide this component if it fails
        }
    }
    
    /// <summary>
    ///     Populates the dropdown with available display modes
    /// </summary>
    private void PopulateDropdown()
    {
        displayModeDropdown.ClearOptions();
        
        List<string> options = new List<string>(displayModeNames);
        displayModeDropdown.AddOptions(options);
        
        // Add listener for when display mode changes
        displayModeDropdown.onValueChanged.AddListener(ChangeDisplayMode);
    }
    
    /// <summary>
    ///     Sets the initial dropdown selection based on saved preference or current display mode
    /// </summary>
    private void SetInitialSelection()
    {
        // Try to load saved preference first
        int savedIndex = PlayerPrefs.GetInt(PREFS_DISPLAY_MODE, -1);
        if (savedIndex >= 0 && savedIndex < displayModes.Length)
        {
            displayModeDropdown.SetValueWithoutNotify(savedIndex);
            return;
        }
        
        // Otherwise use current system setting
        FullScreenMode currentMode = Screen.fullScreenMode;
        
        // Find the index of the current mode in our array
        int currentIndex = Array.IndexOf(displayModes, currentMode);
        
        // Default to first option if current mode isn't in our list
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }
        
        // Set dropdown value without triggering the listener
        displayModeDropdown.SetValueWithoutNotify(currentIndex);
    }
    
    /// <summary>
    ///     Changes the display mode when a new option is selected
    /// </summary>
    private void ChangeDisplayMode(int index)
    {
        if (index < 0 || index >= displayModes.Length)
            return;
            
        // Save the preference
        PlayerPrefs.SetInt(PREFS_DISPLAY_MODE, index);
        PlayerPrefs.Save();
        
        FullScreenMode newMode = displayModes[index];

        // Get the currently selected monitor
        int targetMonitorIndex = DisplayUtils.GetCurrentMonitorIndex();
        
        // Get display layout to ensure we're going fullscreen on the right monitor
        List<DisplayInfo> displays = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displays);
        
        if (displays.Count > targetMonitorIndex)
        {
            DisplayInfo targetDisplay = displays[targetMonitorIndex];
            ApplyDisplayModeChange(newMode, targetDisplay, index, targetMonitorIndex);
        }
        else
        {
            // Fallback if target monitor not found
            FallbackDisplayModeChange(newMode);
        }
    }
    
    /// <summary>
    ///     Apply display mode change through standard process
    /// </summary>
    private void ApplyDisplayModeChange(FullScreenMode newMode, DisplayInfo targetDisplay, int displayModeIndex, int monitorIndex)
    {
        // Always move the window to the target monitor first, regardless of the new mode
        Vector2Int centerPosition = new Vector2Int(
            targetDisplay.width / 2 - Screen.width / 2,
            targetDisplay.height / 2 - Screen.height / 2
        );
        
        // Move window to the target monitor
        Screen.MoveMainWindowTo(targetDisplay, centerPosition);
        
        // Wait briefly to ensure the move completes
        StartCoroutine(CompleteDisplayModeChange(newMode, targetDisplay, displayModeIndex, monitorIndex));
    }
    
    /// <summary>
    ///     Fallback display mode change when target monitor not found
    /// </summary>
    private void FallbackDisplayModeChange(FullScreenMode newMode)
    {
        // Fallback if target monitor not found
        Screen.SetResolution(
            Screen.currentResolution.width,
            Screen.currentResolution.height,
            newMode,
            Screen.currentResolution.refreshRateRatio
        );
        
        // Wait for the display mode change to take effect
        StartCoroutine(UpdateResolutionDropdown());
    }

    /// <summary>
    ///     Completes the display mode change after ensuring the window is positioned correctly
    /// </summary>
    private IEnumerator CompleteDisplayModeChange(
        FullScreenMode newMode, 
        DisplayInfo targetDisplay, 
        int displayModeIndex,
        int monitorIndex)
    {
        // Wait briefly to ensure window move is complete
        yield return new WaitForSeconds(0.1f);

        if (newMode != FullScreenMode.Windowed)
        {
            // More aggressive approach for fullscreen
            // First force windowed mode to reset any fullscreen state
            Screen.SetResolution(
                800, 600, // Small temporary resolution
                FullScreenMode.Windowed,
                Screen.currentResolution.refreshRateRatio
            );
        }
        else
        {
            // For windowed mode, scale based on monitor resolution
            var (windowedWidth, windowedHeight) = DisplayUtils.CalculateWindowedResolution(targetDisplay);
            
            Screen.SetResolution(
                windowedWidth,
                windowedHeight,
                FullScreenMode.Windowed,
                Screen.currentResolution.refreshRateRatio
            );
        }
        
        // Wait after first resolution change
        yield return new WaitForSeconds(0.1f);

        Vector2Int centerPosition;
        
        if (newMode != FullScreenMode.Windowed)
        {
            // Move the window again to ensure it's on the right monitor
            centerPosition = new Vector2Int(
                targetDisplay.width / 2 - 400, // Half of 800
                targetDisplay.height / 2 - 300 // Half of 600
            );
        }
        else
        {
            // Determine window size again for positioning
            var (windowedWidth, windowedHeight) = DisplayUtils.CalculateWindowedResolution(targetDisplay);
            
            // Ensure the window stays on the target monitor even in windowed mode
            centerPosition = new Vector2Int(
                targetDisplay.width / 2 - windowedWidth / 2,
                targetDisplay.height / 2 - windowedHeight / 2
            );
        }
        
        // Move window to the calculated position
        Screen.MoveMainWindowTo(targetDisplay, centerPosition);
        
        // Wait after moving window
        yield return new WaitForSeconds(0.1f);
        
        if (newMode != FullScreenMode.Windowed)
        {
            // Now go fullscreen on that monitor
            Screen.SetResolution(
                targetDisplay.width,
                targetDisplay.height,
                newMode,
                Screen.currentResolution.refreshRateRatio
            );
        }
        
        // Wait for the display mode change to take effect and update resolution dropdown
        yield return StartCoroutine(UpdateResolutionDropdown());
    }
    
    /// <summary>
    ///     Updates the resolution dropdown after display mode changes
    /// </summary>
    private IEnumerator UpdateResolutionDropdown()
    {
        // Wait for the display mode change to take effect
        yield return new WaitForSeconds(0.2f);
        
        // Update the resolution dropdown
        if (resolutionDropdownComponent != null)
        {
            resolutionDropdownComponent.UpdateToMatchCurrentResolution();
        }
    }
}