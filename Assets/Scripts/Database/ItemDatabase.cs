using System;
using System.Collections.Generic;
using Inventory;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Database
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<Item> AllItems; // List of all possible items
        private int iterator = 0;

        // Method to get a random item and remove it from the db.
        public Item GetRandomItemWithNewId()
        {
            if (AllItems == null || AllItems.Count == 0)
            {
                Debug.LogWarning("No items in the database.");
                return null;
            }

            int randomIndex = Random.Range(0, AllItems.Count);
            Item result = new Item(
                AllItems[randomIndex].Id+"_"+ (iterator++),
                AllItems[randomIndex].Name,
                AllItems[randomIndex].Description,
                AllItems[randomIndex].Value,
                AllItems[randomIndex].Sprite,
                AllItems[randomIndex].isBuyable,
                AllItems[randomIndex].Function,
                AllItems[randomIndex].IncreaseMovementAmount,
                AllItems[randomIndex].IncreaseInvSpaceAmount,
                AllItems[randomIndex].Type
                );
            return result;
        }
        
    }
}