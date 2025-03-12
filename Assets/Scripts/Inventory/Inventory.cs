using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class Inventory
    {
        [SerializeField] private List<Item> items; // Use a field instead of a property
        [SerializeField] private int maxCapacity; // Use a field instead of a property

        public delegate void BuyItemAction();
        public static event BuyItemAction OnBuyItem;
        
        public delegate void ItemRemovedAction(Item item);
        public static event ItemRemovedAction OnItemRemoved;
        
        public delegate void ItemUsedAction(Item item);
        public static event ItemUsedAction OnItemUsed;

        public List<Item> Items
        {
            get => items;
            set => items = value;
        }

        public int MaxCapacity
        {
            get => maxCapacity;
            set => maxCapacity = value;
        }

        public Inventory(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            Items = new List<Item>();
        }

        // Add an item to the inventory
        public bool AddItem(Item item)
        {
            if (IsFull())
            {
                Debug.LogWarning("Inventory is full!");
                return false;
            }

            Items.Add(item);
            Debug.Log($"Added {item.Name} to inventory.");
            return true;
        }

        // Remove an item from the inventory
        public void RemoveItem(Item item,bool used = false)
        {
            if (used)
            {
                RemoveInstance(item);
            }
            else
            {
                UIManager.Instance.ShowConfirmation("Are you sure you want to delete this item? You can buy it back again, in the city.",
                    () => { RemoveInstance(item); },  // Confirm action
                    () => { Debug.Log("Item deleted."); } // Cancel action
                );
            }
        }

        public void RemoveInstance(Item item)
        {
            Items.Remove(item);
            item.isBuyable = true;

            OnItemRemoved?.Invoke(item);
            Debug.Log($"Removed {item.Name} from inventory.");
            
            switch (item.Function)
            {
                case Item.ItemFunction.IncreaseMovement:
                    break;
                case Item.ItemFunction.IncreaseInventorySpace:
                    Player.Instance.inventory.MaxCapacity -= item.IncreaseInvSpaceAmount;
                    Player.Instance.inventoryMaxCapacity -= item.IncreaseInvSpaceAmount;
                    if (Player.Instance.inventory.Items.Count > Player.Instance.inventory.MaxCapacity)
                    {
                        Debug.Log("Clearing inventory. Not enough capacity after removing inventory increase item. " + (Player.Instance.inventory.Items.Count - Player.Instance.inventory.MaxCapacity));
                        int tries = 100;
                        int index = 0;
                        do
                        {
                            tries--;
                            if (Player.Instance.inventory.Items[index].Function == Item.ItemFunction.IncreaseInventorySpace) {
                                index++; 
                                continue;
                            }

                            Player.Instance.inventory.Items[index].isBuyable = true;
                            Items.Remove(Player.Instance.inventory.Items[index]);

                            OnItemRemoved?.Invoke(Player.Instance.inventory.Items[index]);
                            Debug.Log($"Removed {Player.Instance.inventory.Items[index].Name} from inventory. " +
                                      (Player.Instance.inventory.Items.Count > Player.Instance.inventory.MaxCapacity));
                                
                                

                        } while (tries > 0 && Player.Instance.inventory.Items.Count > Player.Instance.inventory.MaxCapacity);
                    }
                        
                    break;
                case Item.ItemFunction.None:
                case Item.ItemFunction.DiscoverWithin2:
                case Item.ItemFunction.FullEnergy:
                case Item.ItemFunction.SeeThroughForest:
                case Item.ItemFunction.SeeThroughHill:
                default:
                    break;
            }
        }
        
        public void UseItem(Item item)
        {
            OnItemUsed?.Invoke(item);
            
            // USABLE ONCE
            if (item.Type == Item.ItemType.Usable)
            {
                UIManager.Instance.ShowConfirmation($"Are you sure you want to use {item.Name}?",
                    () =>
                    {
                        RemoveItem(item,true);
                        Debug.Log($"Used up {item.Name}: {item.Description}");
                    },
                    () =>
                    {
                        Debug.Log("Item use cancelled.");
                    } 
                );
            }
        }

        public List<Item> GetPermanentItems()
        {
            return items.Where(item => item.Type == Item.ItemType.Permanent).ToList();
        }

        public void BuyItem(Item item)
        {
            if (item.Value > Player.Instance.money)
            {
                Debug.LogWarning("Not enough money to buy item.");
                return;
            }

            if (!AddItem(item)) return;
            
            Player.Instance.money -= item.Value;
            item.isBuyable = false;

            switch (item.Function)
            {
                case Item.ItemFunction.IncreaseMovement:
                    Player.Instance.itemBonusMovementPoints += item.IncreaseMovementAmount;
                    break;
                case Item.ItemFunction.IncreaseInventorySpace:
                    Player.Instance.inventory.MaxCapacity += item.IncreaseInvSpaceAmount;
                    Player.Instance.inventoryMaxCapacity += item.IncreaseInvSpaceAmount;
                    break;
                case Item.ItemFunction.None:
                case Item.ItemFunction.DiscoverWithin2:
                case Item.ItemFunction.FullEnergy:
                case Item.ItemFunction.SeeThroughForest:
                case Item.ItemFunction.SeeThroughHill:
                default:
                    break;
            }

            OnBuyItem?.Invoke();

        }

        // Check if the inventory is full
        public bool IsFull()
        {
            return Items.Count >= MaxCapacity;
        }
        
    }
}
