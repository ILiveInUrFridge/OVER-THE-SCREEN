using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    /// <summary>
    ///     The settings panel is hidden by default.
    /// </summary>
    void Start()
    {
        settingsPanel.SetActive(false);
    }

    /// <summary>
    ///     Show the settings panel.
    /// </summary>
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    /// <summary>
    ///     Hide the settings panel.
    /// </summary>
    public void HideSettings()
    {
        settingsPanel.SetActive(false);
    }
}