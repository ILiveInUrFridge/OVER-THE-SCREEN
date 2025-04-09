using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
///     This is a dropdown that allows the player to switch between languages.
/// </summary>
public class LanguageDropdown : MonoBehaviour, ILoggable
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    private List<Locale> availableLocales = new List<Locale>();
    
    // PlayerPrefs key
    private const string PREFS_LANGUAGE_CODE = "LanguageCode";

    /// <summary>
    ///     Dictionary mapping locale codes to their native display names.
    /// </summary>
    private readonly Dictionary<string, string> nativeLanguageNames = new Dictionary<string, string>
    {
        { "en", "English" },
        { "ja", "日本語" },
        { "zh-Hans", "简体中文" },
        { "ko", "한국어" },
        { "es", "Español" }
    };

    private void Start()
    {
        try
        {
            // Wait for the localization system to initialize
            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                PopulateDropdown();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += _ => PopulateDropdown();
            }
        }
        catch (System.Exception e)
        {
            // this.LogError($"Error initializing language dropdown: {e.Message}\n{e.StackTrace}");
            gameObject.SetActive(false); // Hide this component if it fails
        }
    }

    /// <summary>
    ///     Populate the dropdown with supported languages
    /// </summary>
    private void PopulateDropdown()
    {
        languageDropdown.ClearOptions();
        
        // Get the locales you've configured in your Unity project
        availableLocales = LocalizationSettings.AvailableLocales.Locales;
        
        List<string> options = new List<string>();
        int currentIndex = 0;
        
        // Check for saved language preference
        string savedLanguageCode = PlayerPrefs.GetString(PREFS_LANGUAGE_CODE, string.Empty);
        bool hasSavedPreference  = !string.IsNullOrEmpty(savedLanguageCode);
        
        // If we have a saved preference, try to use it
        if (hasSavedPreference)
        {
            // Find the locale matching the saved code
            for (int i = 0; i < availableLocales.Count; i++)
            {
                if (availableLocales[i].Identifier.Code == savedLanguageCode)
                {
                    LocalizationSettings.SelectedLocale = availableLocales[i];
                    currentIndex = i;
                    // this.Log($"Loaded saved language: {savedLanguageCode}");
                    break;
                }
            }
        }
        
        // Get current locale (either from saved preference or system default)
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        
        // Add each configured locale to the dropdown with its native name
        for (int i = 0; i < availableLocales.Count; i++)
        {
            Locale locale = availableLocales[i];
            string localeCode = locale.Identifier.Code;
            
            // Get the base language code
            string baseCode = localeCode.Contains("-") ? localeCode.Split('-')[0] : localeCode;

            // Special case for Chinese Simplified
            // bruh
            if (localeCode.StartsWith("zh-Hans"))
            {
                baseCode = "zh-Hans";
            }
            
            // Get the native name, fallback to the locale name if not in our dictionary
            string displayName = nativeLanguageNames.TryGetValue(baseCode, out string nativeName) ? 
                nativeName : locale.LocaleName;
            
            options.Add(displayName);
            
            // Check if this is the currently selected locale
            if (!hasSavedPreference && locale == currentLocale)
            {
                currentIndex = i;
                // this.Log($"Current locale: {locale.Identifier.Code} ({displayName})");
            }
        }
        
        // If no supported locales were found, show a warning and disable the dropdown
        if (options.Count == 0)
        {
            // this.LogWarning("None of the supported languages are available in the Localization Settings.");
            gameObject.SetActive(false);
            return;
        }
        
        // Add options to dropdown
        languageDropdown.AddOptions(options);
        
        // Set current value without triggering events
        languageDropdown.SetValueWithoutNotify(currentIndex);
        
        // Add listener for language changes
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    /// <summary>
    ///     Handle language change when dropdown selection changes
    /// </summary>
    private void OnLanguageChanged(int index)
    {
        if (index >= 0 && index < availableLocales.Count)
        {
            Locale selectedLocale = availableLocales[index];
            // this.Log($"Changing language to: {selectedLocale.Identifier.Code}");
            
            // Save the preference
            PlayerPrefs.SetString(PREFS_LANGUAGE_CODE, selectedLocale.Identifier.Code);
            PlayerPrefs.Save();
            
            // Change the selected locale
            LocalizationSettings.SelectedLocale = selectedLocale;
        }
    }
}
