using System;
using System.Collections.Generic;
using Inventory;
using MyBox;
using Quests;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hexes
{
    [Serializable]
    public enum CityType
    {
        Big,
        Village,
        Outpost
    }
    [Serializable]
    public class City
    {
        [Separator("Generation Data")]
        public Vector2Int position;
        public CityType cityType;
        public bool starter = false;
        [Separator("Attributes")]
        public string name;
        public List<Quest> quests = new List<Quest>();
        public List<Item> shopItems = new List<Item>();

        public City(Vector2Int position, CityType cityType)
        {
            this.position = position;
            this.cityType = cityType;
        }

        public City(Vector2Int position, CityType cityType, bool starter)
        {
            this.position = position;
            this.cityType = cityType;
            this.starter = starter;
        }

        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        public CityType CityType
        {
            get => cityType;
            set => cityType = value;
        }

        public bool Starter
        {
            get => starter;
            set => starter = value;
        }
    }
}
