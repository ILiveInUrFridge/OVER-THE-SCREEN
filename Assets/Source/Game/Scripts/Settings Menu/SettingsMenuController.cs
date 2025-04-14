using UnityEngine;
using Game.Audio;
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
        AudioManager.SFX.Play("menu_open_1", volume: 3.0f);
    }

    /// <summary>
    ///     Hide the settings panel.
    /// </summary>
    public void HideSettings()
    {
        settingsPanel.SetActive(false);

        // Play menu close sound
        AudioManager.SFX.Play("menu_close_1", volume: 3.0f);
    }
}