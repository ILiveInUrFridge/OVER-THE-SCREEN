using UnityEngine;

public class ExitButton : MonoBehaviour
{
    /// <summary>
    ///     Exits the game
    ///     
    ///     Kinda no shit
    /// </summary>
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
