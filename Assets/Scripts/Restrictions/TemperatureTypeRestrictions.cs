using System;
using System.Collections.Generic;
using Hexes.TileType;
using Thresholds;
using UnityEngine;

namespace Restrictions
{
    public class TemperatureTypeRestrictions : MonoBehaviour
    {
        public static TemperatureType GetTypeFromNoise(float noiseMapValues)
        {
            switch (noiseMapValues)
            {
                case > TerrainConstants.VeryHotTempTr:
                    return TemperatureType.VeryHot;
                case > TerrainConstants.HotTempTr:
                    return TemperatureType.Hot;
                case > TerrainConstants.MildTempTr:
                    return TemperatureType.Mild;
                case > TerrainConstants.ColdTempTr:
                    return TemperatureType.Cold;
                default:
                    return TemperatureType.VeryCold;
            }
        }
    }
}
