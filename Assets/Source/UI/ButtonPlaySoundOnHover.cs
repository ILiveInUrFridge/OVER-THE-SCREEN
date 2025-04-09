using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Added for UI components

[System.Serializable]
public class SoundData
{
    public string name;
    public float volume = 1.0f;
}

// Explicitly implement the interface
public class ButtonPlaySoundOnHover : MonoBehaviour, IPointerEnterHandler, ILoggable
{
    [SerializeField] private List<SoundData> soundData;
    
    private void Awake()
    {
        // Ensure this gameObject has required components
        if (GetComponent<Selectable>() == null && GetComponent<Image>() == null)
        {
            this.LogWarning("ButtonPlaySoundOnHover should be attached to a UI element with Image or Selectable component");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var sound in soundData)
        {
            // this.Log($"Playing sound: {sound.name} with volume: {sound.volume}");
            AudioManager.SFX.Play(sound.name, sound.volume);
        }
    }
}