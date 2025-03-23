using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    /// <summary>
    ///     Collect each w × h in a dictionary, then keep the highest refresh rate.
    /// </summary>
    private Dictionary<(int, int), Resolution> bestResolutionMap 
        = new Dictionary<(int, int), Resolution>();

    /// <summary>
    ///     Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        // Get all system resolutions (each resolution has a width, height, and refresh rate.
        // So there can be multiple resolutions with the same width and height, but different refresh rates).
        Resolution[] allResolutions = Screen.resolutions;

        // Filter to 16:9 and keep highest refresh rate per w × h
        // I might make this 60hz later... but I'm not sure. Does this affect the cursor as well? If so, then
        // The highest refresh rate should be the best. I probably won't test this cuz I'm lazy as fuck.
        foreach (Resolution res in allResolutions)
        {
            float aspect = (float) res.width / (float) res.height;

            if (Mathf.Abs(aspect - (16f / 9f)) < 0.01f) {
                var key = (res.width, res.height);

                // If we haven't seen this resolution yet, store it;
                // if we have, keep whichever has the higher refresh rate
                if (!bestResolutionMap.ContainsKey(key)) {
                    bestResolutionMap[key] = res;
                } else {
                    // Compare refresh rates; keep the higher
                    Resolution existing = bestResolutionMap[key];
                    if (res.refreshRateRatio.value > existing.refreshRateRatio.value) {
                        bestResolutionMap[key] = res;
                    }
                }
            }
        }

        // Convert map values to a list
        var validResolutions = new List<Resolution>(bestResolutionMap.Values);

        // Populate the dropdown
        PopulateDropdown(validResolutions);
    }

    /// <summary>
    ///     Populate the dropdown with the given resolutions.
    /// </summary>
    private void PopulateDropdown(List<Resolution> resolutions)
    {
        // Sort resolutions by width and height (highest first)
        resolutions.Sort((a, b) =>
            (b.width * b.height).CompareTo(a.width * a.height));

        // Limit to only the top 5 resolutions (or fewer if less are available)
        int maxResolutions = 5;
        if (resolutions.Count > maxResolutions)
        {
            resolutions = resolutions.GetRange(0, maxResolutions);
        }

        // Apply the highest resolution immediately at startup
        Resolution highestResolution = resolutions[0];
        Screen.SetResolution(highestResolution.width, highestResolution.height, Screen.fullScreenMode, highestResolution.refreshRateRatio);

        resolutionDropdown.ClearOptions();

        List<string> optionStrings = new List<string>();
        int currentIndex = 0;  // Default to first (highest) resolution

        for (int i = 0; i < resolutions.Count; i++)
        {
            Resolution res = resolutions[i];
            // Example label: "1920 x 1080 @ 144Hz (16:9)"
            string optionString = $"{res.width} x {res.height} (16:9)";
            optionStrings.Add(optionString);
        }

        resolutionDropdown.AddOptions(optionStrings);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        // Listen for resolution changes
        resolutionDropdown.onValueChanged.AddListener(index =>
        {
            Resolution chosen = resolutions[index];
            Screen.SetResolution(chosen.width, chosen.height, Screen.fullScreenMode, chosen.refreshRateRatio);
        });
    }
}