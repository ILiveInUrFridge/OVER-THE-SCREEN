using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
}

public class SFXPlayer : AudioPlayer
{
    [Header("Sound Effects Library")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();
    
    // Dictionary for quick lookup of sound effects by name
    private Dictionary<string, AudioClip> soundEffectLookup = new Dictionary<string, AudioClip>();
    
    // Singleton pattern
    private static SFXPlayer _instance;
    public static SFXPlayer Instance => _instance;
    
    /// <summary>
    ///     Awake is called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            base.Awake();
            
            // Build sound effect lookup dictionary
            BuildSoundEffectLookup();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    ///     Build the sound effect lookup dictionary
    /// </summary>
    private void BuildSoundEffectLookup()
    {
        soundEffectLookup.Clear();
        
        // Add all sound effects from the list
        foreach (var sfx in soundEffects)
        {
            if (sfx.clip != null && !string.IsNullOrEmpty(sfx.name))
            {
                soundEffectLookup[sfx.name] = sfx.clip;
            }
        }
    }
    
    /// <summary>
    ///     Play a sound effect with optional volume and position
    /// </summary>
    public override int Play(AudioClip clip, float volume = 1.0f, bool loop = false)
    {
        if (clip == null) return -1;
        
        int soundID = GenerateSoundID();
        AudioSource source = GetAvailableAudioSource();
        
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.Play();
            
            // Track this sound if we need to stop it later
            activeSounds[soundID] = source;
            
            // If not looping, remove from tracking when done
            if (!loop)
            {
                StartCoroutine(RemoveWhenDone(soundID, clip.length));
            }
        }
        
        return soundID;
    }
    
    /// <summary>
    ///     Play a sound effect by name
    /// </summary>
    public int Play(string soundName, float volume = 1.0f, bool loop = false)
    {
        if (soundEffectLookup.TryGetValue(soundName, out AudioClip clip))
        {
            return Play(clip, volume, loop);
        }
        
        Debug.LogWarning($"SFXPlayer: Sound effect '{soundName}' not found.");
        return -1;
    }
    
    /// <summary>
    ///     Helper coroutine to remove a sound from tracking once it's done playing
    /// </summary>
    private IEnumerator RemoveWhenDone(int soundID, float duration)
    {
        yield return new WaitForSeconds(duration);
        activeSounds.Remove(soundID);
    }
    
    /// <summary>
    ///     Add a new sound effect programmatically
    /// </summary>
    public void AddSoundEffect(string name, AudioClip clip)
    {
        if (clip != null && !string.IsNullOrEmpty(name))
        {
            // Add to dictionary lookup
            soundEffectLookup[name] = clip;
            
            // Add to list for inspector view
            var sfx = new SoundEffect { name = name, clip = clip };
            soundEffects.Add(sfx);
        }
    }
} 