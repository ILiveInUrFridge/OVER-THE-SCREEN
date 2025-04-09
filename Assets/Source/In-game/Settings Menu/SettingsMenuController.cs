using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    /// <summary>
    ///     Show the settings panel.
    /// </summary>
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);

        // Play menu open sound
        if (AudioManager.SFX != null)
        {
            AudioManager.SFX.Play("settings_menu_open");
        }
    }

    /// <summary>
    ///     Hide the settings panel.
    /// </summary>
    public void HideSettings()
    {
        settingsPanel.SetActive(false);

        // Play menu close sound
        if (AudioManager.SFX != null)
        {
            AudioManager.SFX.Play("settings_menu_close");
        }
    }
}