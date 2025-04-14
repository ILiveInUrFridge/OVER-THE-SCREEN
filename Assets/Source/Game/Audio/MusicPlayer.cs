using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Utilities;

namespace Game.Audio
{
    [System.Serializable]
    public class MusicTrack
    {
        public string name;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 1.0f;
        public bool loop = true;
    }

    /// <summary>
    ///     Player responsible for background music
    /// </summary>
    public class MusicPlayer : AudioPlayer
    {
        [Header("Music Settings")]
        [SerializeField] private float crossFadeDuration = 2.0f;
        
        [Header("Music Library")]
        [SerializeField] private List<MusicTrack> musicTracks = new List<MusicTrack>();
        [SerializeField] private string defaultTrackName;
        
        // Dictionary for quick lookup of music tracks by name
        private Dictionary<string, MusicTrack> trackLookup = new Dictionary<string, MusicTrack>();
        
        // Singleton pattern
        private static MusicPlayer _instance;
        public static MusicPlayer Instance => _instance;
        
        private int currentMusicID = -1;
        private AudioClip currentMusic;

        /// <summary>
        ///     Awake is called when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                base.Awake();

                // Build music track lookup dictionary
                BuildMusicTrackLookup();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        ///     Initialize after awake
        /// </summary>
        private void Start()
        {
            // Start default music if assigned and play on awake is true
            if (playOnAwake && !string.IsNullOrEmpty(defaultTrackName))
            {
                Play(defaultTrackName);
            }
        }
        
        /// <summary>
        ///     Build music track lookup dictionary
        /// </summary>
        private void BuildMusicTrackLookup()
        {
            trackLookup.Clear();
            
            foreach (var track in musicTracks)
            {
                if (track.clip != null && !string.IsNullOrEmpty(track.name))
                {
                    trackLookup[track.name] = track;
                }
            }
        }
        
        /// <summary>
        ///     Play a music track, with crossfade if another track is already playing
        /// </summary>
        public override int Play(AudioClip clip, float volume = 1.0f, bool loop = true)
        {
            return Play(clip, volume, loop, 0f);
        }
        
        /// <summary>
        ///     Play a music track, with crossfade if another track is already playing
        ///     and optional fade in if no track is currently playing
        /// </summary>
        public int Play(AudioClip clip, float volume = 1.0f, bool loop = true, float fadeIn = 0f)
        {
            if (clip == null) return -1;
            
            // If same music is already playing, do nothing
            if (currentMusic == clip)
            {
                return currentMusicID;
            }
            
            // Stop previous music with crossfade if needed
            if (currentMusicID >= 0)
            {
                StartCoroutine(CrossFadeMusic(clip, volume, loop));
                return currentMusicID; // Return the same ID since we're just changing the clip
            }
            else
            {
                // No music playing, start immediately
                int soundID = GenerateSoundID();
                currentMusicID = soundID;
                currentMusic = clip;
                
                AudioSource source = audioSource; // Always use the main audio source for music
                source.clip = clip;
                source.loop = loop;
                
                if (fadeIn > 0f)
                {
                    source.volume = 0f;
                    source.Play();
                    StartCoroutine(FadeIn(source, volume, fadeIn));
                }
                else
                {
                    source.volume = volume;
                    source.Play();
                }
                
                activeSounds[soundID] = source;
                return soundID;
            }
        }
        
        /// <summary>
        ///     Play a music track by name
        /// </summary>
        public int Play(string trackName)
        {
            return Play(trackName, 0f);
        }
        
        /// <summary>
        ///     Play a music track by name with optional fade in
        /// </summary>
        public int Play(string trackName, float fadeIn = 0f)
        {
            if (trackLookup.TryGetValue(trackName, out MusicTrack track))
            {
                return Play(track.clip, track.volume, track.loop, fadeIn);
            }
            
            Debug.LogWarning($"MusicPlayer: Track '{trackName}' not found.");
            return -1;
        }
        
        /// <summary>
        ///     Add a new music track programmatically.
        ///     
        ///     Doubt I'll ever use this, but it's here if needed.
        /// </summary>
        public void AddMusicTrack(string name, AudioClip clip, float volume = 1.0f, bool loop = true)
        {
            if (clip != null && !string.IsNullOrEmpty(name))
            {
                // Create new track
                var track = new MusicTrack 
                { 
                    name = name, 
                    clip = clip,
                    volume = volume,
                    loop = loop
                };
                
                // Add to dictionary and list
                trackLookup[name] = track;
                musicTracks.Add(track);
            }
        }
        
        /// <summary>
        ///     Smoothly transition between music tracks
        /// </summary>
        private IEnumerator CrossFadeMusic(AudioClip newClip, float targetVolume, bool loop)
        {
            // Create a new audio source for the new music
            AudioSource newSource = GetAvailableAudioSource();
            if (newSource == audioSource) // If we got the main source, get another one
            {
                newSource = gameObject.AddComponent<AudioSource>();
                newSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            }
            
            // Setup the new source
            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.loop = loop;
            newSource.Play();
            
            AudioSource oldSource = activeSounds[currentMusicID];
            float startVolume = oldSource.volume;
            float timer = 0;
            
            // Crossfade
            while (timer < crossFadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / crossFadeDuration;
                newSource.volume = Mathf.Lerp(0f, targetVolume, t);
                oldSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            
            // Cleanup
            oldSource.Stop();
            oldSource.clip = null;
            
            // Update tracking
            activeSounds.Remove(currentMusicID);
            int newID = GenerateSoundID();
            currentMusicID = newID;
            currentMusic = newClip;
            activeSounds[newID] = newSource;
            
            // If we added a new source, we should swap it with the main one
            if (newSource != audioSource)
            {
                audioSource.clip = newSource.clip;
                audioSource.volume = newSource.volume;
                audioSource.loop = newSource.loop;
                audioSource.timeSamples = newSource.timeSamples;
                audioSource.Play();
                
                newSource.Stop();
                Destroy(newSource);
                
                activeSounds[newID] = audioSource;
            }
        }
        
        /// <summary>
        ///     Fade in helper coroutine
        /// </summary>
        private IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
        {
            float timer = 0;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        /// <summary>
        ///     Pause the current music
        /// </summary>
        public void PauseMusic()
        {
            if (currentMusicID >= 0 && activeSounds.TryGetValue(currentMusicID, out AudioSource source))
            {
                source.Pause();
            }
        }
        
        /// <summary>
        ///     Resume the current music
        /// </summary>
        public void ResumeMusic()
        {
            if (currentMusicID >= 0 && activeSounds.TryGetValue(currentMusicID, out AudioSource source))
            {
                source.UnPause();
            }
        }
        
        /// <summary>
        ///     Fade out the current music
        /// </summary>
        public void FadeOutMusic(float duration = 2.0f)
        {
            if (currentMusicID >= 0)
            {
                StartCoroutine(FadeOut(currentMusicID, duration));
            }
        }
        
        /// <summary>
        ///     Fade out helper coroutine
        /// </summary>
        private IEnumerator FadeOut(int soundID, float duration)
        {
            if (activeSounds.TryGetValue(soundID, out AudioSource source))
            {
                float startVolume = source.volume;
                float timer = 0;
                
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                    yield return null;
                }
                
                source.Stop();
                source.clip = null;
                activeSounds.Remove(soundID);
                
                if (soundID == currentMusicID)
                {
                    currentMusicID = -1;
                    currentMusic = null;
                }
            }
        }
    }
} 