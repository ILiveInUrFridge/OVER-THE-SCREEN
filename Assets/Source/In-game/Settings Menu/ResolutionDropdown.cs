using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
///     This is a dropdown that allows the player to switch between resolutions.
///     
///     Only first five resolutions are shown, and resolutions dynamically change
///     based on the current display monitor.
/// </summary>
public class ResolutionDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private List<Resolution> availableResolutions = new List<Resolution>();
    
    void Start()
    {
        RefreshResolutions();
    }
    
    public void RefreshResolutions()
    {
        this.Log("Refreshing resolutions list");
        
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
        
        PopulateDropdown();
    }
    
    private void PopulateDropdown()
    {
        // Clear existing options
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentIndex = 0;
        
        // Get current resolution
        int currentWidth  = Screen.width;
        int currentHeight = Screen.height;
        bool foundMatch   = false;
        
        // Create options for each resolution
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Resolution res = availableResolutions[i];
            string option  = $"{res.width} x {res.height} (16:9)";
            options.Add(option);
            
            // Check if this matches current resolution
            if (res.width == currentWidth && res.height == currentHeight)
            {
                currentIndex = i;
                foundMatch = true;
                this.Log($"Found matching resolution at index {i}: {res.width}x{res.height}");
            }
        }
        
        // If no match was found, default to first (highest) resolution
        if (!foundMatch && availableResolutions.Count > 0)
        {
            this.Log($"No matching resolution found for {currentWidth}x{currentHeight}, defaulting to highest");
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
            this.Log($"Changing resolution to: {selected.width}x{selected.height}");
            
            // Set the new resolution while maintaining current display mode
            Screen.SetResolution(
                selected.width,
                selected.height,
                Screen.fullScreenMode,
                selected.refreshRateRatio
            );
        }
    }
    
    // Public method to be called after monitor switches or display mode changes
    public void UpdateToMatchCurrentResolution()
    {
        // Refresh the resolution list first
        RefreshResolutions();
    }
}