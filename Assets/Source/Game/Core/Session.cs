using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Game.Characters;
using Game.Items;
using Game.Common;

namespace Game.Core
{
    /// <summary>
    ///     Represents a game session with save/load functionality
    /// </summary>
    [Serializable]
    public class Session : ILoggable
    {
        [SerializeField] private int? saveSlot;
        [SerializeField] private GameInfo gameInfo;

        public Session(int? saveSlot, GameInfo gameInfo)
        {
            this.saveSlot = saveSlot;
            this.gameInfo = gameInfo;
        }

        public int? SaveSlot => saveSlot;
        public GameInfo GameInfo => gameInfo;

        public void Save()
        {
            if (!saveSlot.HasValue)
            {
                this.LogWarning("Cannot save session without a save slot");
                return;
            }

            // TODO: Do some magic shit to save functionality using JSON serialization
            string json = JsonUtility.ToJson(this, true);
            string path = GetSavePath(saveSlot.Value);
            File.WriteAllText(path, json);
            
            this.Log($"Game saved to slot {saveSlot.Value}");
        }

        public static Session Load(int saveSlot)
        {
            // TODO: Do some magic shit to load functionality using JSON serialization
            string path = GetSavePath(saveSlot);
            
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Session] No save file found at slot {saveSlot}");
                return null;
            }
            
            string json = File.ReadAllText(path);
            Session session = JsonUtility.FromJson<Session>(json);
            
            Debug.Log($"[Session] Game loaded from slot {saveSlot}");
            return session;
        }
        
        private static string GetSavePath(int saveSlot)
        {
            string directory = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            return Path.Combine(directory, $"save_{saveSlot}.json");
        }
    }

    /// <summary>
    ///     Contains all game state information
    /// </summary>
    [Serializable]
    public class GameInfo
    {
        [SerializeField] private int currentDay;
        [SerializeField] private int actionPoints;
        [SerializeField] private Purrine purrine;
        [SerializeField] private Player player;
        [SerializeField] private SexInfo sexInfo;

        public GameInfo(int currentDay, int actionPoints, Purrine purrine, Player player, SexInfo sexInfo)
        {
            this.currentDay = currentDay;
            this.actionPoints = actionPoints;
            this.purrine = purrine;
            this.player = player;
            this.sexInfo = sexInfo;
        }

        public int CurrentDay => currentDay;
        public int ActionPoints => actionPoints;
        public Purrine Purrine => purrine;
        public Player Player => player;
        public SexInfo SexInfo => sexInfo;

        public void ProgressDay() => currentDay++;
        public void AddActionPoints(int amount = 1) => actionPoints += amount;
        public void DecreaseActionPoints(int amount = 1) => actionPoints -= amount;
    }
    
    /// <summary>
    ///     Represents the player character with inventory and stats
    /// </summary>
    [Serializable]
    public class Player
    {
        [SerializeField] private string name;
        [SerializeField] private List<Item> items;
        [SerializeField] private GPU gpu;

        public Player(string name, List<Item> items, GPU gpu)
        {
            this.name = name;
            this.items = items;
            this.gpu = gpu;
        }

        public string Name => name;
        public List<Item> Items => items;
        public GPU Gpu => gpu;
    }
} 