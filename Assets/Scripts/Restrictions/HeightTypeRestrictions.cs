using System;
using System.Collections.Generic;
using Hexes.TileType;
using Thresholds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Restrictions
{
    public class HeightTypeRestrictions : MonoBehaviour
    {
        
        public static HeightType GetTypeFromNoise(List<float> noiseMapValues)
        {
            float variation = Random.Range(0f, 0.01f);
            
            switch (noiseMapValues[0]+variation)
            {
                case < TerrainConstants.DeepWaterTr:
                    return HeightType.DeepWater;
                case < TerrainConstants.MidWaterTr:
                    return HeightType.MidWater;
                case < TerrainConstants.ShallowWaterTr:
                    return HeightType.ShallowWater;
                default:
                    switch (noiseMapValues[1])
                    {
                        case < TerrainConstants.HillTr:
                            return HeightType.Flat;
                        case < TerrainConstants.MountainTr:
                            return HeightType.Hill;
                        default:
                            return HeightType.Mountain;
                    }
            }
        }
    }
}
