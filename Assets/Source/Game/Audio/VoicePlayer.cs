using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Utilities;

namespace Game.Audio
{
    [System.Serializable]
    public class VoiceLine
    {
        public string id;
        public AudioClip clip;
        [TextArea]
        public string subtitle;
        [Range(0, 1)]
        public float volume = 1.0f;
    }

    /// <summary>
    ///     Player responsible for voice lines and dialogue
    /// </summary>
    public class VoicePlayer : AudioPlayer, ILoggable
    {
        [Header("Voice Settings")]
        [SerializeField] private bool interruptCurrentVoice = false;
        [SerializeField] private float voiceDelayBetweenClips = 0.2f;
        
        [Header("Voice Library")]
        [SerializeField] private List<VoiceLine> voiceLines = new List<VoiceLine>();
        
        // Dictionary for quick lookup of voice lines
        private Dictionary<string, VoiceLine> voiceLookup = new Dictionary<string, VoiceLine>();
        
        // Singleton pattern
        private static VoicePlayer _instance;
        public static VoicePlayer Instance => _instance;
        
        // Queue for sequential voice playback
        private Queue<string> voiceQueue = new Queue<string>();
        private bool isPlayingQueue = false;
        private int currentVoiceID = -1;
        
        // Event that fires when a voice line starts playing (useful for subtitles)
        public System.Action<string, string> OnVoiceLineStart;
        public System.Action<string> OnVoiceLineEnd;
        
        /// <summary>
        ///     Awake is called when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                base.Awake();
                
                // Build voice line lookup
                BuildVoiceLineLookup();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        ///     Build voice line lookup dictionary
        /// </summary>
        private void BuildVoiceLineLookup()
        {
            voiceLookup.Clear();
            
            foreach (var line in voiceLines)
            {
                if (line.clip != null && !string.IsNullOrEmpty(line.id))
                {
                    voiceLookup[line.id] = line;
                }
            }
        }
        
        /// <summary>
        ///     Play a voice clip with given volume
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
        ///     Play a voice line by ID
        /// </summary>
        public override int Play(string voiceLineID, float volume = 1.0f, bool loop = false)
        {
            if (voiceLookup.TryGetValue(voiceLineID, out VoiceLine line))
            {
                // Use line-specific volume if not explicitly overridden
                if (volume == 1.0f) volume = line.volume;
                
                int id = Play(line.clip, volume, loop);
                
                // Fire event for subtitle display
                OnVoiceLineStart?.Invoke(voiceLineID, line.subtitle);
                
                return id;
            }
            
            this.LogWarning($"Voice line '{voiceLineID}' not found.");
            return -1;
        }
        
        /// <summary>
        ///     Add voice lines to a queue to play in sequence
        /// </summary>
        public void QueueVoiceLines(params string[] lineIDs)
        {
            foreach (var id in lineIDs)
            {
                if (voiceLookup.ContainsKey(id))
                {
                    voiceQueue.Enqueue(id);
                }
                else
                {
                    this.LogWarning($"Voice line '{id}' not found for queuing.");
                }
            }
            
            if (!isPlayingQueue)
            {
                StartCoroutine(PlayVoiceQueue());
            }
        }
        
        /// <summary>
        ///     Coroutine to play voice clips in sequence from the queue
        /// </summary>
        private IEnumerator PlayVoiceQueue()
        {
            isPlayingQueue = true;
            
            while (voiceQueue.Count > 0)
            {
                string nextLineID = voiceQueue.Dequeue();
                
                if (voiceLookup.TryGetValue(nextLineID, out VoiceLine line))
                {
                    int id = Play(nextLineID);
                    
                    // Wait for the clip to finish
                    yield return new WaitForSeconds(line.clip.length + voiceDelayBetweenClips);
                    
                    // Fire end event
                    OnVoiceLineEnd?.Invoke(nextLineID);
                }
            }
            
            isPlayingQueue = false;
        }
        
        /// <summary>
        ///     Clear the voice queue without playing any more clips
        /// </summary>
        public void ClearVoiceQueue()
        {
            voiceQueue.Clear();
        }
        
        /// <summary>
        ///     Add a new voice line programmatically
        /// </summary>
        public void AddVoiceLine(string id, AudioClip clip, string subtitle = "", float volume = 1.0f)
        {
            if (clip != null && !string.IsNullOrEmpty(id))
            {
                // Create voice line
                var line = new VoiceLine
                {
                    id = id,
                    clip = clip,
                    subtitle = subtitle,
                    volume = volume
                };
                
                // Add to lookup and list
                voiceLookup[id] = line;
                voiceLines.Add(line);
            }
        }
        
        /// <summary>
        ///     Helper coroutine to remove a sound from tracking once it's done playing
        /// </summary>
        private IEnumerator RemoveWhenDone(int soundID, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            string lineID = null;
            
            // Find the line ID for this sound
            foreach (var kvp in voiceLookup)
            {
                if (activeSounds.TryGetValue(soundID, out AudioSource source) && 
                    source.clip == kvp.Value.clip)
                {
                    lineID = kvp.Key;
                    break;
                }
            }
            
            activeSounds.Remove(soundID);
            if (currentVoiceID == soundID)
            {
                currentVoiceID = -1;
                
                // Fire end event if we found the line ID
                if (lineID != null)
                {
                    OnVoiceLineEnd?.Invoke(lineID);
                }
            }
        }
        
        /// <summary>
        ///     Get the subtitle text for a voice line ID
        /// </summary>
        public string GetSubtitleForLine(string lineID)
        {
            if (voiceLookup.TryGetValue(lineID, out VoiceLine line))
            {
                return line.subtitle;
            }
            return string.Empty;
        }
    }
} 