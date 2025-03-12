using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hexes;
using Hexes.TileType;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RiverGenerator
{
    private HexTile[,] _hexTiles;
    private int _minDistBetween;
    private int _maxRiversCount;
    public Dictionary<int,List<HexTile>> RiverTiles { get; set; }
    
    
    public RiverGenerator(HexTile[,] hexTiles,int minDistBetween,int maxRiversCount)
    {
        _hexTiles = hexTiles;
        _minDistBetween = minDistBetween;
        _maxRiversCount = maxRiversCount;
        
        RiverTiles = new Dictionary<int, List<HexTile>>();
        GenerateRiver();
    }

    int triesToGenerateRiver = 1000;
    private void GenerateRiver()
    {
        for (int i = 0; i < _maxRiversCount; i++)
        {
            triesToGenerateRiver--;
            if (triesToGenerateRiver <= 0) { break; } 
            
            int triesToGoForward = 1000; // Per river
            var riverElements = new List<HexTile>();
            bool convertToLake = false;
            
            HexTile tile = RandomMountainTile(_hexTiles,_minDistBetween); // Select a Mountain to start the river from.
            if(tile == null){ return; } // Tile is null when there are no mountains.
            tile.IsRiverStart = true;
            
            Queue<int> directionQueue = new Queue<int>();
            List<HexTile.NearbyTile> orderedTiles = tile.nearbyTiles.OrderBy(nearbyTile => nearbyTile.hexTile.DistFromWater).ToList();
            foreach (HexTile.NearbyTile neighbor in orderedTiles)
            {
                directionQueue.Enqueue(neighbor.direction);
            }
            
            HexTile nextTile = null;
            int previousDir = Int32.MinValue;
            do
            {
                // If tile has water around it, end the loop. 
                if (tile.nearbyTiles.Find(nt => nt.hexTile.IsWater()) != null) { break; }

                if (!directionQueue.Any()) { convertToLake = true; break; } // If there are no directions, break.

                // while (directionQueue.Any())
                // {
                //     Debug.Log(tile.Position + " : " + directionQueue.Dequeue());
                // }
                
                int direction;
                nextTile = null;
                do
                {
                    
                    // Go toward a random direction
                    direction = DirectionFromQueue(directionQueue);
                    // Select the tile in that direction.
                    try
                    {
                        nextTile = tile.nearbyTiles.Find(nearbyTile => nearbyTile.direction == direction).hexTile;
                        triesToGoForward--;
                    }
                    catch { /* ignored */ }
                    
                } while (nextTile == null && triesToGoForward >= 0);

                if (triesToGoForward <= 0) { convertToLake = true; break; }
                // If the direction is not valid(goes back straight,left or right), restart the loop.
                if (IsOppositeDirection(direction, previousDir)) { continue; }
                // If nextTile is above tile, restart the loop.
                if(!IsBelowOrEqual(nextTile.HeightType,tile.HeightType)){ continue;}
                // If nextTile is a mountain, restart the loop.
                if (nextTile.HeightType == HeightType.Mountain) { continue; }
                // If nextTile is a city, restart the loop.
                if (nextTile.IsCity) { continue; }
                // If nextTile is already a river tile, end the loop.
                if (nextTile.IsRiver) { break; }
                // If nextTile is a water tile, end the loop.
                if(nextTile.IsWater()) { break; }
                // If nextTile has more than 1 river around it, add that tile and break.
                if (nextTile.nearbyTiles.FindAll(nt => nt.hexTile.IsRiver).Count > 1)
                { riverElements.Add(nextTile); nextTile.SetRiver(true); break; }
                
                riverElements.Add(nextTile);
                nextTile.SetRiver(true);
                
                //Instantiate(debugPoint, nextTile.transform.position, Quaternion.identity);
                
                // Set tile to the "nextTile" since we want to continue.
                tile = nextTile;
                previousDir = direction;
                
                directionQueue = new Queue<int>();
                orderedTiles = tile.nearbyTiles.OrderBy(nearbyTile => nearbyTile.hexTile.DistFromWater).ToList();
                foreach (HexTile.NearbyTile neighbor in orderedTiles)
                {
                    directionQueue.Enqueue(neighbor.direction);
                }
                
                // If a sea is hit than the method ends
            } while (!nextTile.IsWater());
            
            if (riverElements.Count < 4)
            {
                foreach (var element in riverElements) { element.SetRiver(false); }
                i -= 1;
                continue;
            }

            if (convertToLake)
            {
                tile.ConvertToLake();
            }
            
            RiverTiles.Add(i,riverElements);
        }
        
        // foreach (var kvp in RiverTiles)
        // {
        //     foreach (HexTile hexTile in kvp.Value)
        //     {
        //         Debug.Log("Key = " + kvp.Key + ", Value = " + hexTile.Position);
        //     }
        // }
        
    }

    private static bool IsBelowOrEqual(HeightType hexHeight, HeightType height)
    {
        return hexHeight <= height;
    }
    
    private static HexTile RandomMountainTile(HexTile[,] tiles,int minDistBetween)
    {
        int tries = 1000;
        HexTile tile = null;
        
        do
        {
            int x = Random.Range(0, tiles.GetLength(0));
            int y = Random.Range(0, tiles.GetLength(1));

            // If the tile has a river within the minDistBetween range, restart the loop.
            if (tiles[x, y].GetTilesInRange(minDistBetween).FindAll(t => t.IsRiver).Count > 0) { continue; }

            if (
                tiles[x,y].HeightType == HeightType.Mountain &&  // Has to be mountain.
                !tiles[x,y].IsRiver) // Can't be a river.
            {
                bool hasWaterOrRiver = tiles[x, y].nearbyTiles.Any(nTile => nTile.hexTile.IsWater() || nTile.hexTile.IsRiver);
                if(hasWaterOrRiver){continue;}
                tile = tiles[x, y];
            }

            tries--;
        } while (tile == null && tries > 0);

        return tile == null ? null : tile;
    }
    
    private int DirectionFromQueue(Queue<int> dirQueue)
    {
        return dirQueue.Dequeue();
    }
    
    private bool IsOppositeDirection(int dir1, int dir2)
    {
        if (dir2 == Int32.MinValue)
        {
            //Debug.Log("False");
            return false;
        }

        return dir1 switch
        {
            0 => dir2 is 3 or 2 or 4,
            1 => dir2 is 4 or 3 or 5,
            2 => dir2 is 5 or 0 or 4,
            3 => dir2 is 0 or 1 or 5,
            4 => dir2 is 1 or 2 or 0,
            5 => dir2 is 2 or 3 or 1,
            _ => false
        };
    }
    
    
    
}
