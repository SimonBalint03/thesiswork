using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Hexes;
using Hexes.TileType;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CityGenerator
{
    public List<City> Cities { get; set; }
    
    private HexTile[,] _hexTiles;
    private int _minDistBetween;
    private int _maxCitiesCount;
    private int _width;
    private int _height;

    public CityGenerator(HexTile[,] hexTiles,int minDistBetween,int maxCitiesCount)
    {
        _hexTiles = hexTiles;
        _minDistBetween = minDistBetween;
        _maxCitiesCount = maxCitiesCount;
        _width = _hexTiles.GetLength(0);
        _height = _hexTiles.GetLength(1);
        
        GenerateCities();
    }

    private void GenerateCities()
    {
        Cities = new List<City>();
        List<HexTile> falseTiles = new List<HexTile>(); //Ennek lehetne jobb neve.. Tárolja azt ami egy város körül van és nem lehet már oda rakni újat.

        int triesToGenerate = 1000;

        for (int i = 0; i < triesToGenerate; i++)
        {
            if(Cities.Count >= _maxCitiesCount){ return; }
            HexTile randomTile;
            // Az első város a pálya közepén legyen
            if (Cities.Count == 0)
            {
                randomTile = _hexTiles[Random.Range((int)(_hexTiles.GetLength(0) / 2) - 5, (int)(_hexTiles.GetLength(0) / 2) + 5),
                    Random.Range((int)(_hexTiles.GetLength(1) / 2) - 5, (int)(_hexTiles.GetLength(1) / 2) + 5)];
            }
            else
            {
                randomTile = _hexTiles[Random.Range(0, _hexTiles.GetLength(0)),Random.Range(0, _hexTiles.GetLength(1))];
            }
            

            if (randomTile.IsWater() || randomTile.IsCity || randomTile.HeightType == HeightType.Mountain || falseTiles.Contains(randomTile)) { continue; }

            City randomCity = new City(randomTile.Position, (CityType)Random.Range(0, 3));
            if (Cities.Count == 0) { randomCity = new City(randomTile.Position, CityType.Big, true); }
            randomCity.name = CityNameService.GetRandomCityName();
            
            Cities.Add(randomCity);
            
            randomTile.City = randomCity;
            randomTile.IsCity = true;
            falseTiles.AddRange(randomTile.GetTilesInRange(_minDistBetween));
        }
    }
}
