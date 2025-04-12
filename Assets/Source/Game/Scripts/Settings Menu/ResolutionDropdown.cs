using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
///     This is a dropdown that allows the player to switch between resolutions.
///     
///     Only first five resolutions are shown, and resolutions dynamically change
///     based on the current display monitor.
/// </summary>
public class ResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private List<Resolution> availableResolutions = new List<Resolution>();
    private int currentMonitorIndex = -1;
    private bool resolutionsInitialized = false;
    
    // PlayerPrefs keys
    private const string PREFS_RESOLUTION_WIDTH = "ResolutionWidth";
    private const string PREFS_RESOLUTION_HEIGHT = "ResolutionHeight";
    
    void Start()
    {
        RefreshResolutions();
    }
    
    public void RefreshResolutions()
    {
        // Check if we're on the same monitor as before - if so, no need to rebuild the full list
        int newMonitorIndex = DisplayUtils.GetCurrentMonitorIndex();
        
        // Only rebuild resolution list if we haven't initialized yet or the monitor changed
        if (!resolutionsInitialized || newMonitorIndex != currentMonitorIndex)
        {
            // Record current monitor index
            currentMonitorIndex = newMonitorIndex;
            resolutionsInitialized = true;
            
            // Get all available resolutions
            Resolution[] allResolutions = Screen.resolutions;
            
            // Create a clean list of 16:9 resolutions with best refresh rates
            Dictionary<(int, int), Resolution> bestResolutions = new Dictionary<(int, int), Resolution>();
            
            foreach (Resolution res in allResolutions)
            {
                float aspect = (float) res.width / (float) res.height;
                
                // Filter to approximately 16:9 aspect ratio
                if (Mathf.Abs(aspect - (16f / 9f)) < 0.01f)
                {
                    var key = (res.width, res.height);
                    
                    if (!bestResolutions.ContainsKey(key) || 
                        res.refreshRateRatio.value > bestResolutions[key].refreshRateRatio.value)
                    {
                        bestResolutions[key] = res;
                    }
                }
            }
            
            // Convert to list and sort by resolution size (highest first)
            availableResolutions = new List<Resolution>(bestResolutions.Values);
            availableResolutions.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));
            
            // Limit to top 5
            if (availableResolutions.Count > 5)
            {
                availableResolutions = availableResolutions.GetRange(0, 5);
            }
        }
        
        // Always update the dropdown UI
        PopulateDropdown();
    }
    
    private void PopulateDropdown()
    {
        // Clear existing options
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentIndex = 0;
        
        // Try to load saved resolution preference
        int savedWidth = PlayerPrefs.GetInt(PREFS_RESOLUTION_WIDTH, -1);
        int savedHeight = PlayerPrefs.GetInt(PREFS_RESOLUTION_HEIGHT, -1);
        
        // Get current resolution if no saved preference
        int currentWidth = (savedWidth > 0) ? savedWidth : Screen.width;
        int currentHeight = (savedHeight > 0) ? savedHeight : Screen.height;
        bool foundMatch = false;
        
        // Create options for each resolution
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Resolution res = availableResolutions[i];
            string option = $"{res.width} x {res.height} (16:9)";
            options.Add(option);
            
            // Check if this matches current/saved resolution
            if (res.width == currentWidth && res.height == currentHeight)
            {
                currentIndex = i;
                foundMatch = true;
            }
        }
        
        // If no match was found, default to first (highest) resolution
        if (!foundMatch && availableResolutions.Count > 0)
        {
            currentIndex = 0;
        }
        
        // Add options to dropdown
        resolutionDropdown.AddOptions(options);
        
        // Remove existing listeners to avoid duplicates
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        
        // Set current value without triggering events
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        
        // Add listener for resolution changes
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }
    
    private void OnResolutionChanged(int index)
    {
        if (index >= 0 && index < availableResolutions.Count)
        {
            Resolution selected = availableResolutions[index];
            
            // Save the new resolution preference
            PlayerPrefs.SetInt(PREFS_RESOLUTION_WIDTH, selected.width);
            PlayerPrefs.SetInt(PREFS_RESOLUTION_HEIGHT, selected.height);
            PlayerPrefs.Save();
            
            // Get current display info to properly center the window if in windowed mode
            DisplayInfo currentDisplay = DisplayUtils.GetCurrentDisplay();
            FullScreenMode currentMode = Screen.fullScreenMode;
            
            // Set the new resolution while maintaining current display mode
            Screen.SetResolution(
                selected.width,
                selected.height,
                currentMode,
                selected.refreshRateRatio
            );
            
            // If in windowed mode, re-center the window on the current monitor
            if (currentMode == FullScreenMode.Windowed && currentDisplay.width > 0)
            {
                // Center window on the current monitor
                Vector2Int centerPosition = new Vector2Int(
                    currentDisplay.width / 2 - selected.width / 2,
                    currentDisplay.height / 2 - selected.height / 2
                );
                
                // Wait briefly for resolution change to apply
                StartCoroutine(CenterWindowAfterResolutionChange(currentDisplay, centerPosition));
            }
        }
    }
    
    /// <summary>
    ///     Centers the window after resolution change
    /// </summary>
    private IEnumerator CenterWindowAfterResolutionChange(DisplayInfo display, Vector2Int position)
    {
        // Wait for resolution change to take effect
        yield return new WaitForSeconds(0.1f);
        
        // Move window to center of display
        Screen.MoveMainWindowTo(display, position);
    }
    
    // Public method to be called after monitor switches or display mode changes
    public void UpdateToMatchCurrentResolution()
    {
        // Refresh the resolution list first
        RefreshResolutions();
        
        // Get current resolution
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        
        // Find the matching resolution in our list
        int bestMatchIndex = 0;
        int closestDiff = int.MaxValue;
        
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Resolution res = availableResolutions[i];
            int diff = Mathf.Abs(res.width - currentWidth) + Mathf.Abs(res.height - currentHeight);
            
            if (diff < closestDiff)
            {
                closestDiff = diff;
                bestMatchIndex = i;
            }
        }
        
        // Update dropdown without triggering event
        if (availableResolutions.Count > 0)
        {
            resolutionDropdown.SetValueWithoutNotify(bestMatchIndex);
            
            // Also update saved preferences to match current resolution
            PlayerPrefs.SetInt(PREFS_RESOLUTION_WIDTH, Screen.width);
            PlayerPrefs.SetInt(PREFS_RESOLUTION_HEIGHT, Screen.height);
            PlayerPrefs.Save();
        }
    }
}