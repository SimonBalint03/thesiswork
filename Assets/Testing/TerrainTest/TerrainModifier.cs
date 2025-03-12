using UnityEngine;

namespace TerrainTest
{
    public class TerrainModifier : MonoBehaviour
    {
        public Terrain terrain;
        public float hexRadius = 5f;
        public float heightIncrease = 0.1f;  // Height increase ratio for terrain

        void Start()
        {
            TerrainData terrainData = terrain.terrainData;
            int resolution = terrainData.heightmapResolution;
            float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    // Convert heightmap coordinates to world space
                    Vector3 worldPos = TerrainToWorld(terrainData, x, y);
                    Vector3 hexCenter = GetNearestHexCenter(worldPos);

                    if (IsPointInHex(worldPos, hexCenter, hexRadius))
                    {
                        // Raise terrain in the hex mask
                        heights[x, y] += heightIncrease;
                    }
                }
            }

            // Apply the new heightmap
            terrainData.SetHeights(0, 0, heights);
        }

        Vector3 TerrainToWorld(TerrainData terrainData, int x, int y)
        {
            float xPos = (float)x / terrainData.heightmapResolution * terrainData.size.x;
            float zPos = (float)y / terrainData.heightmapResolution * terrainData.size.z;
            return new Vector3(xPos, 0, zPos);
        }

        Vector3 GetNearestHexCenter(Vector3 point)
        {
            // Logic to find the nearest hex center
            // Assuming a basic hex grid generation (for now, use the point itself)
            return point; // Replace with actual nearest hex center logic
        }

        bool IsPointInHex(Vector3 point, Vector3 hexCenter, float hexRadius)
        {
            float dx = Mathf.Abs(point.x - hexCenter.x);
            float dz = Mathf.Abs(point.z - hexCenter.z);
            return dx <= hexRadius && dz <= hexRadius * Mathf.Sqrt(3) / 2;
        }
    }
}