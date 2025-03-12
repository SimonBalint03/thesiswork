using System;
using System.Diagnostics;
using Hexes;
using Hexes.TileType;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Quests
{
    public enum ObjectiveType
    {
        FindGeography,
        FindBuilding,
        FindCity
    }

    public enum GeographyType
    {
        Forest,
        River,
        Mountain,
        Lake
    }
    
    [System.Serializable]
    public class Objective
    {
        public int id;
        public string title;
        public int currentProgress;
        public int requiredProgress;
        public bool isCompleted;
        public ObjectiveType type;
        
        [ConditionalField(nameof(type),false, ObjectiveType.FindGeography)] 
        public GeographyType geographyType;
        
        
        public delegate void ObjectiveCompleted(Objective objective);
        public static event ObjectiveCompleted OnObjectiveCompleted;
        
        public Objective(string title, int requiredProgress)
        {
            this.title = title;
            isCompleted = false;
            currentProgress = 0;
            this.requiredProgress = requiredProgress;
        }

        public void UpdateProgress(int amount)
        {
            currentProgress += amount;
        }
        
        public void CheckCompletion(HexTile tile)
        {
            //Debug.Log("Tile found" + currentProgress + "/" + requiredProgress + isCompleted);
            
            if (isCompleted) { return; }

            switch (type)
            {
                case ObjectiveType.FindGeography:
                {
                    switch (geographyType)
                    {
                        case GeographyType.Forest:
                            if (tile.IsForest) { UpdateProgress(1); }
                            break;
                        case GeographyType.River:
                            if (tile.IsRiver) { UpdateProgress(1); }
                            break;
                        case GeographyType.Mountain:
                            if (tile.HeightType == HeightType.Mountain) { UpdateProgress(1); }
                            break;
                        case GeographyType.Lake:
                            if (tile.IsWater()) { UpdateProgress(1); }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (currentProgress >= requiredProgress)
                    {
                        CompleteObjective();
                    }

                    break;
                }
                case ObjectiveType.FindBuilding:
                    break;
                case ObjectiveType.FindCity:
                    if (tile.IsCity) { UpdateProgress(1); }
                    
                    if (currentProgress >= requiredProgress)
                    {
                        CompleteObjective();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CompleteObjective()
        {
            isCompleted = true;
            OnObjectiveCompleted?.Invoke(this);
            
            Debug.Log("Objective Complete: " + title + " - " + type);
        }
    }
}
