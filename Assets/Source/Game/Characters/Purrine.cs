using UnityEngine;
using System;
using Game.Common;

namespace Game.Characters
{
    /// <summary>
    ///     Represents the Purrine character with all her stats and moods
    /// </summary>
    [Serializable]
    public class Purrine
    {
        // Stats range from 0 to 100
        private const int MIN_STAT = 0;
        private const int MAX_STAT = 100;
        
        [SerializeField] private int affection;
        [SerializeField] private int lust;
        [SerializeField] private int hatred;
        [SerializeField] private int trust;
        [SerializeField] private int fear;
        [SerializeField] private int energy;
        [SerializeField] private int hunger;
        [SerializeField] private int sanitation;
        [SerializeField] private Mood mood;

        public Purrine(int affection, int lust, int hatred, int trust, int fear, int energy, int hunger, int sanitation, Mood mood)
        {
            this.affection = Mathf.Clamp(affection, MIN_STAT, MAX_STAT);
            this.lust = Mathf.Clamp(lust, MIN_STAT, MAX_STAT);
            this.hatred = Mathf.Clamp(hatred, MIN_STAT, MAX_STAT);
            this.trust = Mathf.Clamp(trust, MIN_STAT, MAX_STAT);
            this.fear = Mathf.Clamp(fear, MIN_STAT, MAX_STAT);
            this.energy = Mathf.Clamp(energy, MIN_STAT, MAX_STAT);
            this.hunger = Mathf.Clamp(hunger, MIN_STAT, MAX_STAT);
            this.sanitation = Mathf.Clamp(sanitation, MIN_STAT, MAX_STAT);
            this.mood = mood;
        }

        public int Affection => affection;
        public int Lust => lust;
        public int Hatred => hatred;
        public int Trust => trust;
        public int Fear => fear;
        public int Energy => energy;
        public int Hunger => hunger;
        public int Sanitation => sanitation;
        public Mood Mood => mood;
        
        public string MoodName => mood.GetName();

        public void SetAffection(int value) => affection = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void AddAffection(int amount = 1) => affection = Mathf.Clamp(affection + amount, MIN_STAT, MAX_STAT);
        public void DecreaseAffection(int amount = 1) => affection = Mathf.Clamp(affection - amount, MIN_STAT, MAX_STAT);

        public void SetLust(int value) => lust = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void AddLust(int amount = 1) => lust = Mathf.Clamp(lust + amount, MIN_STAT, MAX_STAT);
        public void DecreaseLust(int amount = 1) => lust = Mathf.Clamp(lust - amount, MIN_STAT, MAX_STAT);

        public void SetHatred(int value) => hatred = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseHatred(int amount = 1) => hatred = Mathf.Clamp(hatred + amount, MIN_STAT, MAX_STAT);
        public void DecreaseHatred(int amount = 1) => hatred = Mathf.Clamp(hatred - amount, MIN_STAT, MAX_STAT);

        public void SetTrust(int value) => trust = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseTrust(int amount = 1) => trust = Mathf.Clamp(trust + amount, MIN_STAT, MAX_STAT);
        public void DecreaseTrust(int amount = 1) => trust = Mathf.Clamp(trust - amount, MIN_STAT, MAX_STAT);

        public void SetFear(int value) => fear = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseFear(int amount = 1) => fear = Mathf.Clamp(fear + amount, MIN_STAT, MAX_STAT);
        public void DecreaseFear(int amount = 1) => fear = Mathf.Clamp(fear - amount, MIN_STAT, MAX_STAT);

        public void SetEnergy(int value) => energy = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseEnergy(int amount = 1) => energy = Mathf.Clamp(energy + amount, MIN_STAT, MAX_STAT);
        public void DecreaseEnergy(int amount = 1) => energy = Mathf.Clamp(energy - amount, MIN_STAT, MAX_STAT);

        public void SetHunger(int value) => hunger = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseHunger(int amount = 1) => hunger = Mathf.Clamp(hunger + amount, MIN_STAT, MAX_STAT);
        public void DecreaseHunger(int amount = 1) => hunger = Mathf.Clamp(hunger - amount, MIN_STAT, MAX_STAT);

        public void SetSanitation(int value) => sanitation = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        public void IncreaseSanitation(int amount = 1) => sanitation = Mathf.Clamp(sanitation + amount, MIN_STAT, MAX_STAT);
        public void DecreaseSanitation(int amount = 1) => sanitation = Mathf.Clamp(sanitation - amount, MIN_STAT, MAX_STAT);

        public void SetMood(Mood value) => mood = value;
        
        /// <summary>
        ///     Automatically determine mood based on current stats
        /// </summary>
        public void UpdateMood()
        {
            // Simple mood determination based on average of stats
            // This can be made more complex as needed
            
            if (energy < 20)
            {
                mood = Mood.TIRED;
                return;
            }
            
            if (hunger > 80)
            {
                mood = Mood.SICK;
                return;
            }
            
            if (sanitation < 20)
            {
                mood = Mood.SICK;
                return;
            }
            
            if (hatred > 80)
            {
                mood = Mood.ANGRY;
                return;
            }
            
            if (fear > 80)
            {
                mood = Mood.ANXIOUS;
                return;
            }
            
            if (affection > 80)
            {
                mood = Mood.HAPPY;
                return;
            }
            
            mood = Mood.NEUTRAL;
        }
    }
    
    /// <summary>
    ///     Contains data related to sexual activities
    /// </summary>
    [Serializable]
    public class SexInfo
    {
        [SerializeField] private int sexCount;
        [SerializeField] private int sensitivity;
        [SerializeField] private int cumInsideCount;
        [SerializeField] private int cumOutsideCount;
        [SerializeField] private int orgasmCount;
        [SerializeField] private bool isVirgin;

        public SexInfo(int sexCount, int sensitivity, int cumInsideCount, int cumOutsideCount, int orgasmCount, bool isVirgin)
        {
            this.sexCount = sexCount;
            this.sensitivity = sensitivity;
            this.cumInsideCount = cumInsideCount;
            this.cumOutsideCount = cumOutsideCount;
            this.orgasmCount = orgasmCount;
            this.isVirgin = isVirgin;
        }

        public int SexCount => sexCount;
        public int Sensitivity => sensitivity;
        public int CumInsideCount => cumInsideCount;
        public int CumOutsideCount => cumOutsideCount;
        public int OrgasmCount => orgasmCount;
        public bool IsVirgin => isVirgin;

        public void SetSexCount(int value) => sexCount = value;
        public void AddSexCount(int amount = 1) => sexCount += amount;
        public void RemoveSexCount(int amount = 1) => sexCount -= amount;

        public void SetSensitivity(int value) => sensitivity = value;
        public void IncreaseSensitivity(int amount = 1) => sensitivity += amount;
        public void DecreaseSensitivity(int amount = 1) => sensitivity -= amount;

        public void SetCumInsideCount(int value) => cumInsideCount = value;
        public void AddCumInsideCount(int amount = 1) => cumInsideCount += amount;
        public void RemoveCumInsideCount(int amount = 1) => cumInsideCount -= amount;

        public void SetCumOutsideCount(int value) => cumOutsideCount = value;
        public void AddCumOutsideCount(int amount = 1) => cumOutsideCount += amount;
        public void RemoveCumOutsideCount(int amount = 1) => cumOutsideCount -= amount;

        public void SetOrgasmCount(int value) => orgasmCount = value;
        public void AddOrgasmCount(int amount = 1) => orgasmCount += amount;
        public void RemoveOrgasmCount(int amount = 1) => orgasmCount -= amount;

        public void SetVirginStatus(bool value) => isVirgin = value;
    }
} 