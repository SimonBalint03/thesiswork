using System;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Inventory
{
    [Serializable]
    public class Item
    {
        
        public enum ItemFunction
        {
            None,
            DiscoverWithin2,
            FullEnergy,
            SeeThroughForest,
            SeeThroughHill,
            IncreaseMovement,
            IncreaseInventorySpace
        }
        
        public enum ItemType
        {
            Usable, // Can be sold or used once
            Permanent // Can also be sold but gives a permanent boost
        }
        
        public string Id;
        public string Name;
        public string Description;
        public int Value;
        public Sprite Sprite;
        public bool isBuyable;
        public ItemFunction Function;
        [ConditionalField(nameof(Function),false,ItemFunction.IncreaseMovement)]public double IncreaseMovementAmount;
        [ConditionalField(nameof(Function),false,ItemFunction.IncreaseInventorySpace)]public int IncreaseInvSpaceAmount;
        public ItemType Type;

        public Item(
            string id,
            string name,
            string description,
            int value,
            Sprite sprite,
            bool isBuyable,
            ItemFunction function,
            double increaseMovementAmount,
            int increaseInvSpaceAmount,
            ItemType type)
        {
            Id = id;
            Name = name;
            Description = description;
            Value = value;
            Sprite = sprite;
            this.isBuyable = isBuyable;
            Function = function;
            IncreaseMovementAmount = increaseMovementAmount;
            IncreaseInvSpaceAmount = increaseInvSpaceAmount;
            Type = type;
        }

        protected bool Equals(Item other)
        {
            return Id == other.Id && Name == other.Name && Description == other.Description && Value == other.Value && Equals(Sprite, other.Sprite) && isBuyable == other.isBuyable && Function == other.Function && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Item)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Description, Value, Sprite, isBuyable, (int)Function, (int)Type);
        }
    }
}
