using UnityEngine;
using System;

namespace OTS.Items
{
    /// <summary>
    ///     Represents an item in the player's inventory
    /// </summary>
    [Serializable]
    public class Item
    {
        [SerializeField] private int id;
        [SerializeField] private string name;
        [SerializeField] private int price;
        [SerializeField] private string description;
        [SerializeField] private int amount;

        public Item(int id, string name, int price, string description, int amount)
        {
            this.id = id;
            this.name = name;
            this.price = price;
            this.description = description;
            this.amount = amount;
        }

        public int Id => id;
        public string Name => name;
        public int Price => price;
        public string Description => description;
        public int Amount => amount;
        
        public void IncreaseAmount(int value = 1) => amount += value;
        public void DecreaseAmount(int value = 1) => amount = Math.Max(0, amount - value);
    }

    /// <summary>
    ///     Represents an item available for purchase in shops
    /// </summary>
    [Serializable]
    public class ShopItem
    {
        [SerializeField] private int id;
        [SerializeField] private string name;
        [SerializeField] private int price;
        [SerializeField] private string description;

        public ShopItem(int id, string name, int price, string description)
        {
            this.id = id;
            this.name = name;
            this.price = price;
            this.description = description;
        }

        public int Id => id;
        public string Name => name;
        public int Price => price;
        public string Description => description;
        
        /// <summary>
        ///     Convert a shop item to an inventory item
        /// </summary>
        /// 
        /// <param name="amount">
        ///     Amount to add to inventory
        /// </param>
        /// 
        /// <returns>
        ///     A new inventory item
        /// </returns>
        public Item ToInventoryItem(int amount = 1)
        {
            return new Item(id, name, price, description, amount);
        }
    }
} 