using UnityEngine;
using System;
using OTS.Common;

namespace OTS.Scripts.Environment
{
    /// <summary>
    ///     Manages the time of day system within each game day.
    ///     Handles transitions between Morning, Afternoon, and Night.
    /// </summary>
    public class TimeOfDayManager : MonoBehaviour, ILoggable
    {
        public static TimeOfDayManager Instance { get; private set; }
        
        [Header("Time Configuration")]
        [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        /// <summary>
        ///     Event fired when the time of day changes
        /// </summary>
        public static event Action<TimeOfDay, TimeOfDay> OnTimeOfDayChanged;
        
        /// <summary>
        ///     Event fired when a new day starts (resets to Morning)
        /// </summary>
        public static event Action<int> OnNewDayStarted;
        
        public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;
        
        /// <summary>
        ///     Initialize the singleton instance
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
            
            if (enableDebugLogs)
                this.Log($"Initialized with time: {currentTimeOfDay.GetName()}");
        }
        
        /// <summary>
        ///     Advances to the next time of day
        /// </summary>
        public void AdvanceTime()
        {
            TimeOfDay previousTime = currentTimeOfDay;
            TimeOfDay nextTime = currentTimeOfDay.GetNext();
            
            SetTimeOfDay(nextTime);
            
            if (enableDebugLogs)
                this.Log($"Advanced from {previousTime.GetName()} to {nextTime.GetName()}");
        }
        
        /// <summary>
        ///     Sets the time of day directly
        /// </summary>
        /// 
        /// <param name="newTimeOfDay">
        ///     The time to set
        /// </param>
        public void SetTimeOfDay(TimeOfDay newTimeOfDay)
        {
            if (currentTimeOfDay == newTimeOfDay) return;
            
            TimeOfDay previousTime = currentTimeOfDay;
            currentTimeOfDay = newTimeOfDay;
            
            // Fire the time change event
            OnTimeOfDayChanged?.Invoke(previousTime, currentTimeOfDay);
            
            if (enableDebugLogs)
                this.Log($"Time changed from {previousTime.GetName()} to {currentTimeOfDay.GetName()}");
        }
        
        /// <summary>
        ///     Called when a new game day starts. Resets time to Morning.
        /// </summary>
        /// 
        /// <param name="dayNumber">
        ///     The new day number
        /// </param>
        public void StartNewDay(int dayNumber)
        {
            if (enableDebugLogs)
                this.Log($"Starting new day {dayNumber}");
                
            SetTimeOfDay(TimeOfDay.Morning);
            OnNewDayStarted?.Invoke(dayNumber);
        }
        
        /// <summary>
        ///     Checks if it's currently a specific time of day
        /// </summary>
        /// 
        /// <param name="timeToCheck">
        ///     The time to check for
        /// </param>
        /// 
        /// <returns>
        ///     True if it's currently the specified time
        /// </returns>
        public bool IsCurrentTime(TimeOfDay timeToCheck)
        {
            return currentTimeOfDay == timeToCheck;
        }
        
        /// <summary>
        ///     Gets the next time of day without advancing
        /// </summary>
        /// 
        /// <returns>
        ///     The next time of day
        /// </returns>
        public TimeOfDay GetNextTime()
        {
            return currentTimeOfDay.GetNext();
        }
        
        /// <summary>
        ///     Debug method to cycle through all times of day
        /// </summary>
        [ContextMenu("Debug: Advance Time")]
        public void DebugAdvanceTime()
        {
            AdvanceTime();
        }
        
        /// <summary>
        ///     Debug method to reset to morning
        /// </summary>
        [ContextMenu("Debug: Reset to Morning")]
        public void DebugResetToMorning()
        {
            SetTimeOfDay(TimeOfDay.Morning);
        }
    }
}
