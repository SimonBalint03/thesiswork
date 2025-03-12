using System.Collections;
using System.Collections.Generic;
using Libraries;
using UnityEngine;

public class PerlinNoiseMap
{
    private int _seed;
    private float _offsetX, _offsetY;
    private int _width, _height;

    public float[,] HeightMap { get; set; }
    public float[,] HillMap { get; set; }
    public float[,] PrecipitationMap { get; set; }
    public float[,] TemperatureMap { get; set; }

    public PerlinNoiseMap(int seed, Vector2Int size)
    {
        _seed = seed;
        _width = size.x;
        _height = size.y;
        
        Random.InitState(_seed);
        _offsetX = Random.value * 10000;
        _offsetY = Random.value * 10000;

        HeightMap = GenerateHeightMap(BoardScale.HEIGHT_SCALE);
        HillMap = GenerateHillMap(BoardScale.HILL_SCALE);
        PrecipitationMap = GeneratePrecipitationMap(BoardScale.PRECIPITAITON_SCALE);
        TemperatureMap = GenerateTemperatureMap(BoardScale.TEMPERATURE_SCALE);

    }

    private float[,] GenerateHillMap( float scale)
    {
        float[,] noiseMap = new float[_width, _height];
        
        // Create and configure FastNoise object
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetSeed(_seed);
        noise.SetFrequency(2.2f);
        
        noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
        noise.SetFractalOctaves(3);
        noise.SetFractalLacunarity(1f);
        noise.SetFractalGain(0f);
        noise.SetFractalWeightedStrength(1f);
        noise.SetFractalPingPongStrength(2f);
        
        //noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
        //noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Sub);
        //noise.SetCellularJitter(1);
        

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;

                noiseMap[x, y] = noise.GetNoise(xCoord, yCoord);
            }
        }
        
        
        return noiseMap;
    }
    private float[,] GenerateHeightMap(float scale)
    {
        float[,] noiseMap = new float[_width, _height];

        //Random seed for the map
        //Random.InitState((int)System.DateTime.Now.Ticks);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;
                
                float sample = PerlinNoise(xCoord, yCoord);
                noiseMap[x, y] = sample;
            }
        }
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;

                float sample = 0.5f*PerlinNoise(2*xCoord, 2*yCoord);
                noiseMap[x, y] += sample;
            }
        }
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;

                float sample = 0.25f*PerlinNoise(4*xCoord, 4*yCoord);
                noiseMap[x, y] += sample;
            }
        }
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;

                float sample = 0.125f*PerlinNoise(8*xCoord, 8*yCoord);
                noiseMap[x, y] += sample;
            }
        }
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                noiseMap[x, y] /= (1 + 0.5f + 0.25f + 0.125f);
            }
        }

        return noiseMap;
    }
    private float[,] GeneratePrecipitationMap(float scale)
    {
        _seed = Random.Range(0, int.MaxValue);
        
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        
        float[,] noiseMap = new float[_width, _height];
        
        noise.SetSeed(_seed);
        noise.SetFrequency(0.1f);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalWeightedStrength(0f);
        

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float xCoord = (float)x / scale + _offsetX;
                float yCoord = (float)y / scale + _offsetY;

                noiseMap[x, y] = noise.GetNoise(xCoord, yCoord);
            }
        }
        
        
        return noiseMap;
    }
    private float[,] GenerateTemperatureMap(float scale)
    {
        float[,] temperatureMap = new float[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // Calculate the distance from the top and bottom rows
                float distanceToTopBottom = Mathf.Min(y, _height - 1 - y);

                // Normalize the distance to get a value between 0 and 1
                //float normalizedDistance = Mathf.Clamp01(distanceToTopBottom / (_height / 2f));

                // Use Perlin noise for additional variation
                float perlinValue = Mathf.PerlinNoise(x / scale + _offsetX, y / scale + _offsetY);

                // Assign temperature based on the normalized distance and noise
                temperatureMap[x, y] = ( perlinValue += Random.Range(0.05f,0.2f));
            }
        }
        return temperatureMap;
    }
    
    float PerlinNoise(float x, float y)
    {
        float value = Mathf.PerlinNoise(x, y);
        return value;
    }
}
