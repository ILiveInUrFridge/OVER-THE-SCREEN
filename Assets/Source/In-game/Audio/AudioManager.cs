using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///     Central manager for all audio in the game.
///     This component coordinates the specialized audio players (SFX, Music, Voice)
///     and provides global audio control.
/// </summary>
public class AudioManager : MonoBehaviour, ILoggable
{
    // Default parameter names if VolumeManager is not available
    private const string DEFAULT_MASTER_PARAM = "MasterVolume";
    private const string DEFAULT_SFX_PARAM    = "SFXVolume";
    private const string DEFAULT_MUSIC_PARAM  = "MusicVolume";
    private const string DEFAULT_VOICE_PARAM  = "VoiceVolume";
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("Audio Players")]
    [SerializeField] private SFXPlayer sfxPlayer;
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private VoicePlayer voicePlayer;
    
    [Header("Audio Settings")]
    [SerializeField] private bool createPlayersIfMissing = true;
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    // Singleton pattern
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;
    
    // Cached references for quick access
    public static SFXPlayer   SFX   => Instance?.sfxPlayer;
    public static MusicPlayer Music => Instance?.musicPlayer;
    public static VoicePlayer Voice => Instance?.voicePlayer;
    
    // Reference to the volume manager if it exists
    [SerializeField] private VolumeManager volumeManager;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            InitializeAudioSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (volumeManager != null)
        {
            ConnectVolumeManager();
        }
        
        // Apply initial volumes in case they were saved
        ApplyInitialVolumes();
    }
    
    /// <summary>
    ///     Connects the VolumeManager to the AudioMixer by subscribing to slider events
    /// </summary>
    private void ConnectVolumeManager()
    {
        if (volumeManager == null || audioMixer == null) return;
        
        // Add slider listeners
        if (volumeManager.masterSlider != null)
        {
            volumeManager.masterSlider.onValueChanged.AddListener(val => SetChannelVolume(VolumeParamType.Master, val));
        }
        
        if (volumeManager.sfxSlider != null)
        {
            volumeManager.sfxSlider.onValueChanged.AddListener(val => SetChannelVolume(VolumeParamType.SFX, val));
        }
        
        if (volumeManager.musicSlider != null)
        {
            volumeManager.musicSlider.onValueChanged.AddListener(val => SetChannelVolume(VolumeParamType.Music, val));
        }
        
        if (volumeManager.voiceSlider != null)
        {
            volumeManager.voiceSlider.onValueChanged.AddListener(val => SetChannelVolume(VolumeParamType.Voice, val));
        }
    }
    
    /// <summary>
    ///     Applies the saved volumes from PlayerPrefs to the AudioMixer
    /// </summary>
    private void ApplyInitialVolumes()
    {
        if (audioMixer == null) return;
        
        // Apply master volume
        float masterVolume = PlayerPrefs.GetFloat(VolumeManager.PREFS_MASTER, 50f);
        SetChannelVolume(VolumeParamType.Master, masterVolume);
        
        // Apply SFX volume
        float sfxVolume = PlayerPrefs.GetFloat(VolumeManager.PREFS_SFX, 50f);
        SetChannelVolume(VolumeParamType.SFX, sfxVolume);
        
        // Apply music volume
        float musicVolume = PlayerPrefs.GetFloat(VolumeManager.PREFS_MUSIC, 50f);
        SetChannelVolume(VolumeParamType.Music, musicVolume);
        
        // Apply voice volume
        float voiceVolume = PlayerPrefs.GetFloat(VolumeManager.PREFS_VOICE, 50f);
        SetChannelVolume(VolumeParamType.Voice, voiceVolume);
    }
    
    /// <summary>
    ///     Sets the volume for a specific audio channel
    /// </summary>
    public void SetChannelVolume(VolumeParamType channel, float volume)
    {
        if (audioMixer == null) return;
        
        // Convert 0-100 slider value to decibels
        // Modified formula to make slider value 50 = 0dB
        float normalized = volume / 50f; // Now 50 maps to 1.0, which will be 0dB
        float dB = normalized <= 0.0001f ? -80f : Mathf.Log10(normalized) * 20f;
        
        string paramName = GetSafeVolumeParam(channel);
        audioMixer.SetFloat(paramName, dB);
    }
    
    /// <summary>
    ///     Initialize the audio system, creating required components if needed
    /// </summary>
    private void InitializeAudioSystem()
    {
        if (audioMixer == null)
        {
            this.LogWarning("No AudioMixer assigned. Audio features will be limited.");
        }
        
        if (createPlayersIfMissing)
        {
            // Create SFX Player if needed
            if (sfxPlayer == null)
            {
                GameObject sfxObj = new GameObject("SFX Player");
                sfxObj.transform.SetParent(transform);
                sfxPlayer = sfxObj.AddComponent<SFXPlayer>();
                
                AudioSource sfxSource = sfxObj.AddComponent<AudioSource>();
                TryAssignAudioMixerGroup(sfxSource, "SFX");
            }
            
            // Create Music Player if needed
            if (musicPlayer == null)
            {
                GameObject musicObj = new GameObject("Music Player");
                musicObj.transform.SetParent(transform);
                musicPlayer = musicObj.AddComponent<MusicPlayer>();
                
                AudioSource musicSource = musicObj.AddComponent<AudioSource>();
                TryAssignAudioMixerGroup(musicSource, "Music");
            }
            
            // Create Voice Player if needed
            if (voicePlayer == null)
            {
                GameObject voiceObj = new GameObject("Voice Player");
                voiceObj.transform.SetParent(transform);
                voicePlayer = voiceObj.AddComponent<VoicePlayer>();
                
                AudioSource voiceSource = voiceObj.AddComponent<AudioSource>();
                TryAssignAudioMixerGroup(voiceSource, "Voice");
            }
        }
        
        // Initialize players with volume parameters and mixer reference
        if (audioMixer != null)
        {
            if (sfxPlayer != null) sfxPlayer.Initialize(audioMixer, GetSafeVolumeParam(VolumeParamType.SFX));
            if (musicPlayer != null) musicPlayer.Initialize(audioMixer, GetSafeVolumeParam(VolumeParamType.Music));
            if (voicePlayer != null) voicePlayer.Initialize(audioMixer, GetSafeVolumeParam(VolumeParamType.Voice));
        }
    }
    
    /// <summary>
    ///     Helper method to safely assign an audio mixer group to an audio source
    /// </summary>
    private void TryAssignAudioMixerGroup(AudioSource source, string groupName)
    {
        if (audioMixer != null)
        {
            try
            {
                var groups = audioMixer.FindMatchingGroups(groupName);
                if (groups != null && groups.Length > 0)
                {
                    source.outputAudioMixerGroup = groups[0];
                }
            }
            catch (System.Exception e)
            {
                this.LogError($"Error assigning AudioMixerGroup: {e.Message}");
            }
        }
    }
    
    // Helper to consistently get volume parameter names
    public enum VolumeParamType { Master, SFX, Music, Voice }
    
    /// <summary>
    ///     Get volume parameter with fallback if VolumeManager doesn't exist
    /// </summary>
    private string GetSafeVolumeParam(VolumeParamType paramType)
    {
        try
        {
            switch (paramType)
            {
                case VolumeParamType.Master: return VolumeManager.GetMasterVolumeParam();
                case VolumeParamType.SFX: return VolumeManager.GetSFXVolumeParam();
                case VolumeParamType.Music: return VolumeManager.GetMusicVolumeParam();
                case VolumeParamType.Voice: return VolumeManager.GetVoiceVolumeParam();
                default: return DEFAULT_MASTER_PARAM;
            }
        }
        catch (System.Exception)
        {
            // VolumeManager might not exist or be accessible
            switch (paramType)
            {
                case VolumeParamType.Master: return DEFAULT_MASTER_PARAM;
                case VolumeParamType.SFX: return DEFAULT_SFX_PARAM;
                case VolumeParamType.Music: return DEFAULT_MUSIC_PARAM;
                case VolumeParamType.Voice: return DEFAULT_VOICE_PARAM;
                default: return DEFAULT_MASTER_PARAM;
            }
        }
    }
    
    /// <summary>
    ///     Mute or unmute all game audio
    /// </summary>
    public void MuteAll(bool muted)
    {
        if (audioMixer != null)
        {
            float value = muted ? -80f : 0f;
            audioMixer.SetFloat(GetSafeVolumeParam(VolumeParamType.Master), value);
        }
    }
    
    /// <summary>
    ///     Set global volume levels (0-1)
    /// </summary>
    public void SetGlobalVolume(float normalizedVolume)
    {
        if (audioMixer != null)
        {
            // Adjust to use the same scaling as SetChannelVolume
            float adjustedVolume = normalizedVolume * 50f; // Convert 0-1 to 0-50 scale
            float dB = adjustedVolume <= 0.0001f ? -80f : Mathf.Log10(adjustedVolume / 50f) * 20f;
            audioMixer.SetFloat(GetSafeVolumeParam(VolumeParamType.Master), dB);
        }
    }
    
    /// <summary>
    ///     Stop all audio (SFX, music, and voice)
    /// </summary>
    public void StopAllAudio()
    {
        if (sfxPlayer != null) sfxPlayer.StopAll();
        if (musicPlayer != null) musicPlayer.StopAll();
        if (voicePlayer != null) voicePlayer.StopAll();
    }
    
    /// <summary>
    ///     Pause all audio (useful when pausing the game)
    /// </summary>
    public void PauseAllAudio()
    {
        AudioListener.pause = true;
    }
    
    /// <summary>
    ///     Resume all audio
    /// </summary>
    public void ResumeAllAudio()
    {
        AudioListener.pause = false;
    }
} 