using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using OTS.Core;
using OTS.Common;
using OTS.Characters;
using OTS.Items;
using OTS.Scripts.Environment;

namespace OTS
{
    /// <summary>
    ///     Manages the overall game state and session
    /// </summary>
    public class GameManager : MonoBehaviour, ILoggable
    {
        public static GameManager Instance { get; private set; }
        
        private Session currentSession;
        
        public Session CurrentSession => currentSession;
        public bool IsGameActive => currentSession != null;

        /// <summary>
        ///     Awake is called when the script instance is being loaded
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLocale();
        }

        /// <summary>
        ///     Initialize the locale
        /// </summary>
        private void InitializeLocale()
        {
            // Initialize localization system
            if (LocalizationSettings.HasSettings)
            {
                var initOp = LocalizationSettings.InitializationOperation;
                
                if (initOp.IsDone)
                {
                    LanguageDropdown.LoadSavedLanguage();
                }
                else
                {
                    LocalizationSettings.InitializationOperation.Completed += _ =>
                    {
                        LanguageDropdown.LoadSavedLanguage();
                    };
                }
            }
        }
        
        /// <summary>
        ///     Exits the game
        ///     
        ///     Kinda no shit
        /// </summary>
        public void ExitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        /// <summary>
        ///     Starts a new game with optional save slot
        /// </summary>
        /// 
        /// <param name="saveSlot">
        ///     Save slot to use, or null for temporary session
        /// </param>
        public void StartGame(int? saveSlot = null)
        {
            Purrine purrine = new Purrine(0, 0, 0, 0, 0, 0, 0, 0, Mood.NEUTRAL);
            Player player   = new Player("Player", new List<Item>(), GPU.MEOWTX_1000);
            SexInfo sexInfo = new SexInfo(0, 0, 0, 0, 0, true);

            GameInfo gameInfo = new GameInfo(0, 0, purrine, player, sexInfo);

            currentSession = new Session(saveSlot, gameInfo);

            this.Log("New game started");

            // Set up initial game state
            InitialGameSetup();
        }
        
        /// <summary>
        ///     Loads a game from the specified save slot
        /// </summary>
        /// 
        /// <param name="saveSlot">
        ///     Save slot to load from
        /// </param>
        /// 
        /// <returns>
        ///     True if load was successful, false otherwise
        /// </returns>
        public bool LoadGame(int saveSlot)
        {
            Session loadedSession = Session.Load(saveSlot);
            
            if (loadedSession == null)
            {
                this.LogWarning($"Failed to load game from slot {saveSlot}");
                return false;
            }
            
            currentSession = loadedSession;
            this.Log($"Game loaded from slot {saveSlot}");
            return true;
        }
        
        /// <summary>
        ///     Saves the current game session
        /// </summary>
        /// 
        /// <returns>
        ///     True if save was successful, false otherwise
        /// </returns>
        public bool SaveGame()
        {
            if (currentSession == null)
            {
                this.LogWarning("Cannot save: No active game session");
                return false;
            }
            
            if (!currentSession.SaveSlot.HasValue)
            {
                this.LogWarning("Cannot save: Session has no save slot");
                return false;
            }
            
            currentSession.Save();
            this.Log($"Game saved to slot {currentSession.SaveSlot.Value}");
            return true;
        }
        
        /// <summary>
        ///     End current game session
        /// </summary>
        public void EndGame()
        {
            currentSession = null;
            this.Log("Game ended");
        }
        
        /// <summary>
        ///     Set up initial game state when starting a new game
        /// </summary>
        private void InitialGameSetup()
        {
            // Add starter items
            // Item starterItem = new Item(1, "Basic Computer", 0, "A basic computer for basic needs", 1);
            // currentSession.GameInfo.Player.Items.Add(starterItem);
            
            // Set initial action points
            currentSession.GameInfo.AddActionPoints(3);
        }
        
        /// <summary>
        ///     Process daily events when a day passes
        /// </summary>
        public void ProcessDayEnd()
        {
            if (currentSession == null) return;
            
            GameInfo gameInfo = currentSession.GameInfo;
            
            // Progress day
            gameInfo.ProgressDay();
            
            // Reset action points
            gameInfo.AddActionPoints(3);
            
            // Update Purrine's stats
            Purrine purrine = gameInfo.Purrine;
            purrine.IncreaseHunger(10);
            purrine.DecreaseSanitation(5);
            purrine.DecreaseEnergy(5);
            
            // Update mood based on stats
            purrine.UpdateMood();
            
            // Notify environment system of new day
            if (TimeOfDayManager.Instance != null)
            {
                TimeOfDayManager.Instance.StartNewDay(gameInfo.CurrentDay);
            }
            
            this.Log($"Day {gameInfo.CurrentDay} began. Purrine's mood: {purrine.MoodName}");
        }
        
        /// <summary>
        ///     Advance the time of day (Morning -> Afternoon -> Night)
        /// </summary>
        public void AdvanceTimeOfDay()
        {
            if (TimeOfDayManager.Instance != null)
            {
                TimeOfDayManager.Instance.AdvanceTime();
            }
            else
            {
                this.LogWarning("TimeOfDayManager not found! Cannot advance time.");
            }
        }
        
        /// <summary>
        ///     Set the time of day directly
        /// </summary>
        /// <param name="timeOfDay">The time to set</param>
        public void SetTimeOfDay(TimeOfDay timeOfDay)
        {
            if (TimeOfDayManager.Instance != null)
            {
                TimeOfDayManager.Instance.SetTimeOfDay(timeOfDay);
            }
            else
            {
                this.LogWarning("TimeOfDayManager not found! Cannot set time.");
            }
        }
        
        /// <summary>
        ///     Get the current time of day
        /// </summary>
        /// <returns>The current time of day, or Morning if TimeOfDayManager is not available</returns>
        public TimeOfDay GetCurrentTimeOfDay()
        {
            return TimeOfDayManager.Instance?.CurrentTimeOfDay ?? TimeOfDay.Morning;
        }
    }
} 