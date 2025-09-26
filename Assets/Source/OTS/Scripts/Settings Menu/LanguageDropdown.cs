using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
///     This is a dropdown that allows the player to switch between languages.
/// </summary>
public class LanguageDropdown : MonoBehaviour
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
        { "zh-Hant", "繁體中文" },
        { "ko", "한국어" },
        { "es", "Español" }
    };

    /// <summary>
    ///     Method to load and apply saved language settings. Can be called from anywhere.
    /// </summary>
    public static void LoadSavedLanguage()
    {
        // Check for saved language preference
        string savedLanguageCode = PlayerPrefs.GetString(PREFS_LANGUAGE_CODE, string.Empty);
        if (string.IsNullOrEmpty(savedLanguageCode))
            return;
            
        // Only proceed if localization system is initialized
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            LocalizationSettings.InitializationOperation.Completed += _ => ApplyLanguageCode(savedLanguageCode);
        }
        else
        {
            ApplyLanguageCode(savedLanguageCode);
        }
    }
    
    /// <summary>
    ///     Apply specific language code to the localization system
    /// </summary>
    private static void ApplyLanguageCode(string languageCode)
    {
        var availableLocales = LocalizationSettings.AvailableLocales.Locales;
        
        // Find and apply the saved locale
        for (int i = 0; i < availableLocales.Count; i++)
        {
            if (availableLocales[i].Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = availableLocales[i];
                break;
            }
        }
    }

    private void Start()
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

            // Edge cases
            switch (localeCode)
            {
                case "zh-Hans":
                    baseCode = "zh-Hans";
                    break;
                case "zh-Hant":
                    baseCode = "zh-Hant";
                    break;
            }
            
            // Get the native name, fallback to the locale name if not in our dictionary
            string displayName = nativeLanguageNames.TryGetValue(baseCode, out string nativeName) ? 
                nativeName : locale.LocaleName;
            
            options.Add(displayName);
            
            // Check if this is the currently selected locale
            if (!hasSavedPreference && locale == currentLocale)
            {
                currentIndex = i;
            }
        }
        
        // If no supported locales were found, disable the dropdown
        if (options.Count == 0)
        {
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
            
            // Save the preference
            PlayerPrefs.SetString(PREFS_LANGUAGE_CODE, selectedLocale.Identifier.Code);
            PlayerPrefs.Save();
            
            // Change the selected locale
            LocalizationSettings.SelectedLocale = selectedLocale;
        }
    }
}
