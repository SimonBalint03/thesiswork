using System;
using System.Collections.Generic;
using Hexes.TileType;
using Thresholds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Restrictions
{
    public class BiomeTypeRestrictions : MonoBehaviour
    {
        
        public static BiomeType GetTypeFromNoise(float temp,float prec)
        {
            float variation = Random.Range(0f, 0.05f);
            
            prec += variation;
            temp += variation;
            switch (temp)
            {
                case > TerrainConstants.VeryHotTempTr:
                    return prec switch
                    {
                        > TerrainConstants.RainyTr => BiomeType.Plains,
                        > TerrainConstants.RarelyRainTr => BiomeType.Plains,
                        _ => BiomeType.Plains
                    };
                case > TerrainConstants.HotTempTr:
                    return prec switch
                    {
                        > TerrainConstants.RainyTr => BiomeType.SeasonalForest,
                        > TerrainConstants.MildRainTr => BiomeType.Forest,
                        > TerrainConstants.RarelyRainTr => BiomeType.Plains,
                        _ => BiomeType.Plains
                    };
                case > TerrainConstants.MildTempTr:
                    return prec switch
                    {
                        > TerrainConstants.RainyTr => BiomeType.SeasonalForest,
                        > TerrainConstants.RarelyRainTr => BiomeType.Forest,
                        _ => BiomeType.Plains
                    };
                case > TerrainConstants.ColdTempTr:
                    return BiomeType.Tundra;
                case > TerrainConstants.VeryColdTempTr:
                    return BiomeType.Tundra;
                default:
                    return BiomeType.Tundra;
            }
        }
    }
}
