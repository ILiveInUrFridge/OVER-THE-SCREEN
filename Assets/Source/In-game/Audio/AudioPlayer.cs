using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public abstract class AudioPlayer : MonoBehaviour
{
    [Header("Audio Source Settings")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected bool playOnAwake = false;
    [SerializeField] protected bool loop = false;
    [Tooltip("How many simultaneous sounds this player can play")]
    [SerializeField] protected int poolSize = 5;
    
    // Volume parameter name from VolumeManager
    protected string volumeParameter;
    
    // AudioMixer reference from AudioManager
    protected AudioMixer audioMixer;
    
    // Dictionary to track playing sounds by their ID
    protected Dictionary<int, AudioSource> activeSounds = new Dictionary<int, AudioSource>();
    protected List<AudioSource> audioSourcePool = new List<AudioSource>();
    protected int lastSoundID = 0;
    
    protected virtual void Awake()
    {
        // Initialize the main audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        audioSource.playOnAwake = playOnAwake;
        audioSource.loop = loop;
        
        // Create pool of audio sources for multiple simultaneous sounds
        audioSourcePool.Add(audioSource);
        
        for (int i = 1; i < poolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            newSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            audioSourcePool.Add(newSource);
        }
    }
    
    /// <summary>
    ///     Initialize this audio player with mixer and parameter
    /// </summary>
    public virtual void Initialize(AudioMixer mixer, string volumeParam)
    {
        audioMixer = mixer;
        volumeParameter = volumeParam;
    }
    
    /// <summary>
    ///     Play an audio clip. Returns a sound ID that can be used to stop it later.
    /// </summary>
    public abstract int Play(AudioClip clip, float volume = 1.0f, bool loop = false);
    
    /// <summary>
    ///     Stop a specific sound by its ID
    /// </summary>
    public virtual bool Stop(int soundID)
    {
        if (activeSounds.TryGetValue(soundID, out AudioSource source))
        {
            source.Stop();
            activeSounds.Remove(soundID);
            return true;
        }
        return false;
    }
    
    /// <summary>
    ///     Stop all sounds played by this audio player
    /// </summary>
    public virtual void StopAll()
    {
        foreach (var source in audioSourcePool)
        {
            source.Stop();
        }
        activeSounds.Clear();
    }
    
    /// <summary>
    ///     Set volume level for this audio type
    /// </summary>
    public virtual void SetVolume(float normalizedVolume)
    {
        if (audioMixer != null && !string.IsNullOrEmpty(volumeParameter))
        {
            // Convert to decibels (logarithmic scale)
            // Modified formula to make normalized value 0.5 = 0dB 
            float adjustedVolume = normalizedVolume * 2f; // Scale 0-0.5-1 to 0-1-2
            float dB = normalizedVolume <= 0.0f ? -80f : Mathf.Log10(adjustedVolume) * 20f;
            audioMixer.SetFloat(volumeParameter, dB);
        }
    }
    
    /// <summary>
    ///     Find an available audio source from the pool
    /// </summary>
    protected AudioSource GetAvailableAudioSource()
    {
        // First try to find a source that's not playing
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // If all sources are in use, use the oldest one
        return audioSourcePool[0];
    }
    
    /// <summary>
    ///     Generate a unique ID for the sound
    /// </summary>
    protected int GenerateSoundID()
    {
        return ++lastSoundID;
    }
} 