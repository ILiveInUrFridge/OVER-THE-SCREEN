using UnityEngine;

public class ButtonOpenLink : MonoBehaviour, ILoggable
{
    [SerializeField] private string url;

    /// <summary>
    ///     Opens the URL set in the inspector on this component
    /// </summary>
    public void OpenLink()
    {
        if (string.IsNullOrEmpty(url))
        {
            this.LogWarning("URL is empty!");
            return;
        }

        Application.OpenURL(url);
    }

    /// <summary>
    ///     Opens the provided URL (useful if passing a string parameter from the Button OnClick)
    /// </summary>
    public void OpenLink(string link)
    {
        if (string.IsNullOrEmpty(link))
        {
            this.LogWarning("Provided URL is empty!");
            return;
        }

        Application.OpenURL(link);
    }
}