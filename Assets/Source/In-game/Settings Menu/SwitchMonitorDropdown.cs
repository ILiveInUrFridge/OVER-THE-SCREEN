using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class SwitchMonitorDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown monitorDropdown;
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
    /// Simply switch to the selected monitor directly
    /// </summary>
    private void SwitchToMonitor(int index)
    {
        if (index >= displayLayout.Count)
        {
            this.LogError($"Monitor index {index+1} is out of range");
            return;
        }

        DisplayInfo display = displayLayout[index]; 
        Vector2Int position = new Vector2Int(0, 0);
        
        if (Screen.fullScreenMode != FullScreenMode.Windowed)
        {
            position.x += display.width / 2;
            position.y += display.height / 2;
        }
        
        this.Log($"Moving to monitor {index+1}: {display.name} at position {position}");
        
        try 
        {
            // Just call the API directly - no need to wait
            Screen.MoveMainWindowTo(display, position);
            
            // Apply the current resolution
            // Screen.SetResolution(Screen.currentResolution.width, 
            //                    Screen.currentResolution.height, 
            //                    Screen.fullScreenMode, 
            //                    Screen.currentResolution.refreshRateRatio);
            
            this.Log($"Requested move to monitor {index+1}");
        } 
        catch (Exception e) 
        {
            this.LogError($"Error moving to monitor {index+1}: {e.Message}");
        }
    }
}