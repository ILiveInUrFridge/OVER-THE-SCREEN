using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace Game.Audio
{
    [System.Serializable]
    public class VoiceSound
    {
        public string id;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 1.0f;
        [Range(0f, 2.0f)]
        public float pitchVariation = 0f;
    }

    /// <summary>
    ///     Player responsible for character voice sounds (Undertale-style).
    /// </summary>
    public class VoicePlayer : AudioPlayer, ILoggable
    {
        [Header("Voice Settings")]
        [SerializeField] private bool interruptCurrentVoice = false;
        [SerializeField] private float characterTalkSpeed = 0.05f;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
        
        [Header("Voice Library")]
        [SerializeField] private List<VoiceSound> voiceSounds = new List<VoiceSound>();
        
        // Dictionary for quick lookup of voice sounds
        private Dictionary<string, VoiceSound> voiceLookup = new Dictionary<string, VoiceSound>();
        
        // Singleton pattern
        private static VoicePlayer _instance;
        public static VoicePlayer Instance => _instance;
        
        private int currentVoiceID = -1;
        private Coroutine currentTalkRoutine = null;
        
        // Event that fires when character starts/stops talking
        public System.Action<string> OnTalkStart;
        public System.Action<string> OnTalkEnd;
        
        /// <summary>
        ///     Awake is called when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                base.Awake();
                
                // Build voice sound lookup
                BuildVoiceSoundLookup();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        ///     Build voice sound lookup dictionary
        /// </summary>
        private void BuildVoiceSoundLookup()
        {
            voiceLookup.Clear();
            
            foreach (var sound in voiceSounds)
            {
                if (sound.clip != null && !string.IsNullOrEmpty(sound.id))
                {
                    voiceLookup[sound.id] = sound;
                }
            }
        }
        
        /// <summary>
        ///     Play a voice clip with given volume and pitch
        /// </summary>
        public override int Play(AudioClip clip, float volume = 1.0f, bool loop = false)
        {
            if (clip == null) return -1;
            
            // If set to interrupt and we're already playing a voice
            if (interruptCurrentVoice && currentVoiceID >= 0)
            {
                Stop(currentVoiceID);
            }
            
            int soundID = GenerateSoundID();
            AudioSource source = GetAvailableAudioSource();
            
            if (source != null)
            {
                source.clip = clip;
                source.volume = volume;
                source.loop = loop;
                source.pitch = Random.Range(pitchRange.x, pitchRange.y);
                source.Play();
                
                // Track this sound
                activeSounds[soundID] = source;
                
                // Only set as current voice if we're not already playing one
                // or if we're set to interrupt
                if (currentVoiceID < 0 || interruptCurrentVoice)
                {
                    currentVoiceID = soundID;
                }
                
                // If not looping, remove from tracking when done
                if (!loop)
                {
                    StartCoroutine(RemoveWhenDone(soundID, clip.length));
                }
            }
            
            return soundID;
        }
        
        /// <summary>
        ///     Play a voice sound by ID
        /// </summary>
        public override int Play(string voiceSoundID, float volume = 1.0f, bool loop = false)
        {
            if (voiceLookup.TryGetValue(voiceSoundID, out VoiceSound sound))
            {
                // Use sound-specific volume if not explicitly overridden
                if (volume == 1.0f) volume = sound.volume;
                
                int id = Play(sound.clip, volume, loop);
                
                return id;
            }
            
            this.LogWarning($"Voice sound '{voiceSoundID}' not found.");
            return -1;
        }
        
        /// <summary>
        ///     Play a random voice clip from the list
        /// </summary>
        public int Play(float volume = 1.0f, bool loop = false)
        {
            if (voiceSounds.Count == 0)
            {
                this.LogWarning("No voice sounds available to play randomly.");
                return -1;
            }
            
            // Select a random voice sound
            int randomIndex = Random.Range(0, voiceSounds.Count);
            VoiceSound randomSound = voiceSounds[randomIndex];
            
            // Use sound-specific volume if not explicitly overridden
            if (volume == 1.0f) volume = randomSound.volume;
            
            return Play(randomSound.clip, volume, loop);
        }
        
        /// <summary>
        ///     Play character talk sounds for a given text length
        /// </summary>
        public void PlayTalkSounds(string characterID, string text, float talkSpeed = 0.0f)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            // Stop any current talking
            StopTalking();
            
            // If no talk speed specified, use the default
            if (talkSpeed <= 0.0f) talkSpeed = characterTalkSpeed;
            
            // Start the talk routine
            currentTalkRoutine = StartCoroutine(TalkRoutine(characterID, text, talkSpeed));
        }
        
        /// <summary>
        ///     Stop current talking sound routine
        /// </summary>
        public void StopTalking()
        {
            if (currentTalkRoutine != null)
            {
                StopCoroutine(currentTalkRoutine);
                currentTalkRoutine = null;
                
                // Also stop any currently playing voice
                if (currentVoiceID >= 0)
                {
                    Stop(currentVoiceID);
                    currentVoiceID = -1;
                }
                
                OnTalkEnd?.Invoke("");
            }
        }
        
        /// <summary>
        ///     Coroutine to play character talk sounds for text
        /// </summary>
        private IEnumerator TalkRoutine(string characterID, string text, float talkSpeed)
        {
            OnTalkStart?.Invoke(characterID);
            
            // If this character doesn't have a voice sound, return early
            if (!voiceLookup.ContainsKey(characterID))
            {
                this.LogWarning($"No voice sound found for character '{characterID}'");
                yield break;
            }
            
            // Play a sound for each character (or every few characters)
            for (int i = 0; i < text.Length; i++)
            {
                // Skip spaces and punctuation for sound effects
                if (char.IsWhiteSpace(text[i]) || char.IsPunctuation(text[i]))
                {
                    yield return new WaitForSeconds(talkSpeed * 2); // Longer pause for spaces/punctuation
                    continue;
                }
                
                // Play the character's voice sound
                Play(characterID);
                
                // Wait before the next character
                yield return new WaitForSeconds(talkSpeed);
            }
            
            OnTalkEnd?.Invoke(characterID);
            currentTalkRoutine = null;
        }
        
        /// <summary>
        ///     Play a random voice sound from the available options
        /// </summary>
        public int PlayRandomVoiceSound(float volume = 1.0f)
        {
            if (voiceSounds.Count == 0) return -1;
            
            // Pick a random voice sound
            int randomIndex = Random.Range(0, voiceSounds.Count);
            VoiceSound sound = voiceSounds[randomIndex];
            
            return Play(sound.clip, volume);
        }
        
        /// <summary>
        ///     Add a new voice sound programmatically
        /// </summary>
        public void AddVoiceSound(string id, AudioClip clip, float volume = 1.0f, float pitchVariation = 1.0f)
        {
            if (clip != null && !string.IsNullOrEmpty(id))
            {
                // Create voice sound
                var sound = new VoiceSound
                {
                    id = id,
                    clip = clip,
                    volume = volume,
                    pitchVariation = pitchVariation
                };
                
                // Add to lookup and list
                voiceLookup[id] = sound;
                voiceSounds.Add(sound);
            }
        }
        
        /// <summary>
        ///     Helper coroutine to remove a sound from tracking once it's done playing
        /// </summary>
        private IEnumerator RemoveWhenDone(int soundID, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            string soundID_string = null;
            
            // Find the sound ID for this sound
            foreach (var kvp in voiceLookup)
            {
                if (activeSounds.TryGetValue(soundID, out AudioSource source) && 
                    source.clip == kvp.Value.clip)
                {
                    soundID_string = kvp.Key;
                    break;
                }
            }
            
            activeSounds.Remove(soundID);
            if (currentVoiceID == soundID)
            {
                currentVoiceID = -1;
                
                // Fire end event if we found the ID
                if (soundID_string != null)
                {
                    OnTalkEnd?.Invoke(soundID_string);
                }
            }
        }
    }
} 