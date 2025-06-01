using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    ///     Base class for all specialized audio players
    /// </summary>
    public abstract class AudioPlayer : MonoBehaviour, ILoggable
    {
        [Header("Audio Source Settings")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected bool playOnAwake = false;
        [SerializeField] protected bool loop = false;
        [Tooltip("Initial pool size - more sources will be created if needed")]
        [SerializeField] protected int initialPoolSize = 5;
        [Tooltip("Maximum audio sources to create (0 = unlimited)")]
        [SerializeField] protected int maxAudioSources = 0;
        
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
            
            // Create initial pool of audio sources for multiple simultaneous sounds
            audioSourcePool.Add(audioSource);
            
            for (int i = 1; i < initialPoolSize; i++)
            {
                CreateNewAudioSource();
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
        ///     Play an audio by name. Returns a sound ID that can be used to stop it later.
        /// </summary>
        public abstract int Play(string soundName, float volume = 1.0f, bool loop = false);
        
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
        ///     Creates a new audio source and adds it to the pool
        /// </summary>
        protected AudioSource CreateNewAudioSource()
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            newSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            audioSourcePool.Add(newSource);
            return newSource;
        }
        
        /// <summary>
        ///     Find an available audio source from the pool or create a new one if needed
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
            
            // If we reach the max sources limit, reuse an existing one
            if (maxAudioSources > 0 && audioSourcePool.Count >= maxAudioSources)
            {
                // Find the one closest to completion
                AudioSource oldestSource = audioSourcePool[0];
                float highestPercentComplete = 0f;
                
                foreach (var source in audioSourcePool)
                {
                    if (source.clip != null && source.time > 0)
                    {
                        float percentComplete = source.time / source.clip.length;
                        if (percentComplete > highestPercentComplete)
                        {
                            highestPercentComplete = percentComplete;
                            oldestSource = source;
                        }
                    }
                }
                
                // Clean up the existing reference to this source in activeSounds
                int keyToRemove = -1;
                foreach (var pair in activeSounds)
                {
                    if (pair.Value == oldestSource)
                    {
                        keyToRemove = pair.Key;
                        break;
                    }
                }
                
                if (keyToRemove != -1)
                {
                    activeSounds.Remove(keyToRemove);
                }
                
                return oldestSource;
            }
            
            // If we still have room to grow, create a new audio source
            return CreateNewAudioSource();
        }
        
        /// <summary>
        ///     Generate a unique ID for the sound
        /// </summary>
        protected int GenerateSoundID()
        {
            return ++lastSoundID;
        }
    }
} 