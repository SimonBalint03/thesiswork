using System;
using System.Collections.Generic;
using Hexes.TileType;
using Thresholds;
using UnityEngine;

namespace Restrictions
{
    public class PrecipitationTypeRestrictions : MonoBehaviour
    {
        public static PrecipitaionType GetTypeFromNoise(float noiseMapValues)
        {
            switch (noiseMapValues)
            {
                case > TerrainConstants.StormyTr:
                    return PrecipitaionType.Stormy;
                case > TerrainConstants.RainyTr:
                    return PrecipitaionType.Rainy;
                case > TerrainConstants.MildRainTr:
                    return PrecipitaionType.Mild;
                case > TerrainConstants.RarelyRainTr:
                    return PrecipitaionType.Rarely;
                default:
                    return PrecipitaionType.Dry;
                    
                    
            }
        }
    }
}
