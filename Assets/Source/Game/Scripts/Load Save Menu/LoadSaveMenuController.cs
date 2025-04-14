using UnityEngine;
using Game.Audio;
public class LoadSaveMenuController : MonoBehaviour
{
    [SerializeField] private GameObject loadSavePanel;
    [SerializeField] private GameObject saveExistsIndicator;

    /// <summary>
    ///     Show the load save panel.
    /// </summary>
    public void ShowLoadSave()
    {
        // TODO: Implement logic to see if there are any save files.
        // If there are, show the saveExistsIndicator.

        saveExistsIndicator.SetActive(true); // just for now, testing.

        loadSavePanel.SetActive(true);

        // Play menu open sound
        AudioManager.SFX.Play("menu_open_1", volume: 3.0f);
    }

    /// <summary>
    ///     Hide the load save panel.
    /// </summary>
    public void HideLoadSave()
    {
        loadSavePanel.SetActive(false);

        // Play menu close sound
        AudioManager.SFX.Play("menu_close_1", volume: 3.0f);
    }
}