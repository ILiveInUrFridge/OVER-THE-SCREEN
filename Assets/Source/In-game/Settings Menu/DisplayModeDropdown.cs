using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class DisplayModeDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    
    // List of display modes to offer in the dropdown
    private readonly FullScreenMode[] displayModes = new FullScreenMode[]
    {
        FullScreenMode.ExclusiveFullScreen,    // Standard fullscreen
        FullScreenMode.FullScreenWindow,       // Borderless fullscreen
        FullScreenMode.Windowed                // Windowed mode
    };
    
    // Human-readable names for each display mode
    private readonly string[] displayModeNames = new string[]
    {
        "Fullscreen",
        "Fullscreen Windowed",
        "Windowed"
    };
    
    private void Start()
    {
        try
        {
            // Populate dropdown with display mode options
            PopulateDropdown();
            
            // Set initial selection based on current display mode
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
    ///     Sets the initial dropdown selection based on the current display mode
    /// </summary>
    private void SetInitialSelection()
    {
        FullScreenMode currentMode = Screen.fullScreenMode;
        
        // Find the index of the current mode in our array
        int currentIndex = Array.IndexOf(displayModes, currentMode);
        
        // Default to first option if current mode isn't in our list
        if (currentIndex < 0)
        {
            currentIndex = 0;
            this.LogWarning($"Current display mode ({currentMode}) not found in options, defaulting to {displayModeNames[0]}");
        }
        
        // Set dropdown value without triggering the listener
        displayModeDropdown.SetValueWithoutNotify(currentIndex);
    }
    
    /// <summary>
    ///     Changes the display mode when a new option is selected
    /// </summary>
    private void ChangeDisplayMode(int index)
    {
        if (index >= 0 && index < displayModes.Length)
        {
            FullScreenMode newMode = displayModes[index];
            this.Log($"Changing display mode to: {displayModeNames[index]} ({newMode})");
            
            try
            {
                // Keep the current resolution when changing display mode
                Screen.SetResolution(
                    Screen.currentResolution.width,
                    Screen.currentResolution.height,
                    newMode,
                    Screen.currentResolution.refreshRateRatio
                );
                
                this.Log($"Display mode changed to {displayModeNames[index]}");
            }
            catch (Exception e)
            {
                this.LogError($"Error changing display mode: {e.Message}");
            }
        }
        else
        {
            this.LogError($"Invalid display mode index: {index}");
        }
    }
}
