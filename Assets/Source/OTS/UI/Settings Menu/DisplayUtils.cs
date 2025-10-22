using UnityEngine;

/// <summary>
///     Static utility class for display-related functionality shared between display components
/// </summary>
public static class DisplayUtils
{
    /// <summary>
    ///     Calculate the appropriate windowed resolution based on monitor size
    /// </summary>
    /// 
    /// <param name="display">
    ///     Display information for the target monitor
    /// </param>
    /// 
    /// <returns>
    ///     The calculated width and height tuple
    /// </returns>
    public static (int width, int height) CalculateWindowedResolution(DisplayInfo display)
    {
        int width, height;
        
        // Scale window size based on monitor resolution
        if (display.width >= 3840) // 4K monitor
        {
            width = 2560;  // 2K resolution
            height = 1440;
        }
        else if (display.width >= 2560) // 2K monitor
        {
            width = 1920;  // 1080p resolution
            height = 1080;
        }
        else // 1080p monitor or smaller
        {
            width = 1600;  // 1600x900 resolution
            height = 900;
        }
        
        // Ensure the window fits within the monitor bounds
        width = Mathf.Min(width, display.width - 100);
        height = Mathf.Min(height, display.height - 100);
        
        return (width, height);
    }
    
    /// <summary>
    ///     Gets the currently selected monitor index from SwitchMonitorDropdown, or defaults to 0
    /// </summary>
    public static int GetCurrentMonitorIndex()
    {
        int targetMonitorIndex = 0; // Default to primary display
        SwitchMonitorDropdown monitorDropdown = Object.FindAnyObjectByType<SwitchMonitorDropdown>(); // shit code, but just leaving this as is for now
        if (monitorDropdown != null)
        {
            targetMonitorIndex = monitorDropdown.GetCurrentMonitorIndex();
        }
        return targetMonitorIndex;
    }
    
    /// <summary>
    ///     Gets the current display the window is on
    /// </summary>
    public static DisplayInfo GetCurrentDisplay()
    {
        // Get current monitor index
        int currentMonitorIndex = GetCurrentMonitorIndex();
        
        // Get display info
        var displays = new System.Collections.Generic.List<DisplayInfo>();
        Screen.GetDisplayLayout(displays);
        
        if (displays.Count > currentMonitorIndex)
        {
            return displays[currentMonitorIndex];
        }
        
        // Return a default display if not found
        return new DisplayInfo();
    }
} 