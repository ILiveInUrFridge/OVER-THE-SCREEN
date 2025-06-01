using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace Game.Audio
{
    /// <summary>
    ///     Manages volume settings through UI sliders
    /// </summary>
    public class VolumeManager : MonoBehaviour, ILoggable
    {
        [Header("Sliders")]
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Slider voiceSlider;

        [Header("Input Fields")]
        public TMP_InputField masterInput;
        public TMP_InputField musicInput;
        public TMP_InputField sfxInput;
        public TMP_InputField voiceInput;

        [Header("Reset Buttons")]
        public Button masterResetButton;
        public Button musicResetButton;
        public Button sfxResetButton;
        public Button voiceResetButton;

        [Header("Defaults (0-100)")]
        public float defaultMaster = 50f;
        public float defaultMusic  = 50f;
        public float defaultSFX    = 50f;
        public float defaultVoice  = 50f;

        [Header("AudioMixer (Optional)")]
        public AudioMixer audioMixer; 
        
        // PlayerPrefs keys for saving volume settings
        public const string PREFS_MASTER = "VolumeSettingsMaster";
        public const string PREFS_MUSIC  = "VolumeSettingsMusic";
        public const string PREFS_SFX    = "VolumeSettingsSFX";
        public const string PREFS_VOICE  = "VolumeSettingsVoice";
        
        // AudioMixer parameter names
        private const string MASTER_VOL_PARAM = "MasterVolume";
        private const string MUSIC_VOL_PARAM  = "MusicVolume";
        private const string SFX_VOL_PARAM    = "SFXVolume";
        private const string VOICE_VOL_PARAM  = "VoiceVolume";
        
        // Minimum decibels value (effectively muted)
        private const float MIN_DB = -80f;
        
        // Flag to prevent endless loops when values are updated from code
        private bool updatingUI = false;

        private void Start()
        {
            LoadSavedVolumes();
            SetupEventListeners();
        }
        
        private void LoadSavedVolumes()
        {
            // Load saved volumes or use defaults
            masterSlider.value = PlayerPrefs.GetFloat(PREFS_MASTER, defaultMaster);
            musicSlider.value = PlayerPrefs.GetFloat(PREFS_MUSIC, defaultMusic);
            sfxSlider.value = PlayerPrefs.GetFloat(PREFS_SFX, defaultSFX);
            voiceSlider.value = PlayerPrefs.GetFloat(PREFS_VOICE, defaultVoice);
            
            // Force initial update of UI and mixer
            UpdateAllVolumeSettings();
        }
        
        private void SetupEventListeners()
        {
            // Add slider listeners
            masterSlider.onValueChanged.AddListener(val => OnSliderChanged(val, masterInput, MASTER_VOL_PARAM, PREFS_MASTER));
            musicSlider.onValueChanged.AddListener(val => OnSliderChanged(val, musicInput, MUSIC_VOL_PARAM, PREFS_MUSIC));
            sfxSlider.onValueChanged.AddListener(val => OnSliderChanged(val, sfxInput, SFX_VOL_PARAM, PREFS_SFX));
            voiceSlider.onValueChanged.AddListener(val => OnSliderChanged(val, voiceInput, VOICE_VOL_PARAM, PREFS_VOICE));

            // Add input-field listeners
            masterInput.onEndEdit.AddListener(str => OnInputChanged(str, masterSlider, masterInput, MASTER_VOL_PARAM, PREFS_MASTER));
            musicInput.onEndEdit.AddListener(str => OnInputChanged(str, musicSlider, musicInput, MUSIC_VOL_PARAM, PREFS_MUSIC));
            sfxInput.onEndEdit.AddListener(str => OnInputChanged(str, sfxSlider, sfxInput, SFX_VOL_PARAM, PREFS_SFX));
            voiceInput.onEndEdit.AddListener(str => OnInputChanged(str, voiceSlider, voiceInput, VOICE_VOL_PARAM, PREFS_VOICE));

            // Add reset button listeners
            masterResetButton.onClick.AddListener(() => ResetVolume(masterSlider, masterInput, defaultMaster, MASTER_VOL_PARAM, PREFS_MASTER));
            musicResetButton.onClick.AddListener(() => ResetVolume(musicSlider, musicInput, defaultMusic, MUSIC_VOL_PARAM, PREFS_MUSIC));
            sfxResetButton.onClick.AddListener(() => ResetVolume(sfxSlider, sfxInput, defaultSFX, SFX_VOL_PARAM, PREFS_SFX));
            voiceResetButton.onClick.AddListener(() => ResetVolume(voiceSlider, voiceInput, defaultVoice, VOICE_VOL_PARAM, PREFS_VOICE));
        }
        
        private void UpdateAllVolumeSettings()
        {
            OnSliderChanged(masterSlider.value, masterInput, MASTER_VOL_PARAM, PREFS_MASTER);
            OnSliderChanged(musicSlider.value, musicInput, MUSIC_VOL_PARAM, PREFS_MUSIC);
            OnSliderChanged(sfxSlider.value, sfxInput, SFX_VOL_PARAM, PREFS_SFX);
            OnSliderChanged(voiceSlider.value, voiceInput, VOICE_VOL_PARAM, PREFS_VOICE);
        }

        /// <summary>
        ///     Fired whenever the user drags a slider
        /// </summary>
        private void OnSliderChanged(float sliderValue, TMP_InputField inputField, string volumeParam, string prefKey)
        {
            if (updatingUI) return;
            
            updatingUI = true;
            
            // Update text
            inputField.text = Mathf.RoundToInt(sliderValue).ToString();

            // Save to PlayerPrefs
            PlayerPrefs.SetFloat(prefKey, sliderValue);
            PlayerPrefs.Save();

            // Update AudioMixer or direct volume
            UpdateVolumeLevel(sliderValue, volumeParam);
            
            updatingUI = false;
        }
        
        /// <summary>
        ///     Updates volume in either AudioMixer or direct audio settings
        /// </summary>
        private void UpdateVolumeLevel(float sliderValue, string volumeParam)
        {
            // Update through AudioManager if available, which will handle all audio sources
            if (AudioManager.Instance != null)
            {
                AudioManager.VolumeParamType paramType;
                
                switch (volumeParam)
                {
                    case MASTER_VOL_PARAM:
                        paramType = AudioManager.VolumeParamType.Master;
                        break;
                    case SFX_VOL_PARAM:
                        paramType = AudioManager.VolumeParamType.SFX;
                        break;
                    case MUSIC_VOL_PARAM:
                        paramType = AudioManager.VolumeParamType.Music;
                        break;
                    case VOICE_VOL_PARAM:
                        paramType = AudioManager.VolumeParamType.Voice;
                        break;
                    default:
                        paramType = AudioManager.VolumeParamType.Master;
                        break;
                }
                
                AudioManager.Instance.SetChannelVolume(paramType, sliderValue);
            }
            // Fall back to direct AudioMixer control if no AudioManager is available
            else if (audioMixer != null)
            {
                // Convert to decibels for AudioMixer
                float dB = VolumeToDecibels(sliderValue);
                bool success = audioMixer.SetFloat(volumeParam, dB);
            }
            else
            {
                this.LogWarning("No AudioManager or AudioMixer available to control volume");
            }
        }

        /// <summary>
        ///     Handles user typing in volume text fields
        /// </summary>
        private void OnInputChanged(string textValue, Slider slider, TMP_InputField inputField, string volumeParam, string prefKey)
        {
            if (float.TryParse(textValue, out float parsed))
            {
                parsed = Mathf.Clamp(parsed, 0f, 100f);
                
                if (updatingUI) return;
                updatingUI = true;
                slider.value = parsed;
                updatingUI = false;
                
                // Slider's onValueChanged will handle the rest
            }
            else
            {
                // Parsing failed, revert text to current slider value
                inputField.text = Mathf.RoundToInt(slider.value).ToString();
            }
        }

        /// <summary>
        ///     Resets a volume channel to its default value
        /// </summary>
        private void ResetVolume(Slider slider, TMP_InputField inputField, float defaultVal, string volumeParam, string prefKey)
        {
            if (updatingUI) return;
            updatingUI = true;
            slider.value = defaultVal; // Triggers OnSliderChanged
            updatingUI = false;
            
            // Update directly to avoid potential timing issues
            OnSliderChanged(defaultVal, inputField, volumeParam, prefKey);
        }

        /// <summary>
        ///     Convert 0-100 volume scale to decibels for AudioMixer
        /// </summary>
        private float VolumeToDecibels(float volume)
        {
            if (volume <= 0.01f)
                return MIN_DB; // treat near-zero as silence

            // Modified formula to make slider value 50 = 0dB
            float normalized = volume / 50f; // Now 50 maps to 1.0, which will be 0dB
            return Mathf.Log10(normalized) * 20f;
        }
        
        /// <summary>
        ///     Static method to mute/unmute specific audio channels from other scripts
        /// </summary>
        public void SetChannelMute(string volumeParam, bool muted)
        {
            if (audioMixer == null) return;
            
            audioMixer.SetFloat(volumeParam, muted ? MIN_DB : VolumeToDecibels(GetVolumeForChannel(volumeParam)));
        }
        
        /// <summary>
        ///     Helper to get current volume value for a channel
        /// </summary>
        private float GetVolumeForChannel(string volumeParam)
        {
            switch (volumeParam)
            {
                case MASTER_VOL_PARAM: return masterSlider.value;
                case MUSIC_VOL_PARAM:  return musicSlider.value;
                case SFX_VOL_PARAM:    return sfxSlider.value;
                case VOICE_VOL_PARAM:  return voiceSlider.value;

                default:               return defaultMaster;
            }
        }
        
        /// <summary>
        ///     Static access to AudioMixer parameter names for other scripts
        /// </summary>
        public static string GetMasterVolumeParam() => MASTER_VOL_PARAM;
        public static string GetMusicVolumeParam()  => MUSIC_VOL_PARAM;
        public static string GetSFXVolumeParam()    => SFX_VOL_PARAM;
        public static string GetVoiceVolumeParam()  => VOICE_VOL_PARAM;
    }
}