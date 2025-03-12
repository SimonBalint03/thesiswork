using System;
using System.Collections.Generic;
using System.Linq;
using Hexes.TileType;
using MyBox;
using Restrictions;
using Thresholds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hexes
{
    [Serializable]
    public class HexBoard : MonoBehaviour
    {
        [Separator("Base")]
        public int mapSeed;
        public bool randomSeed;
        public List<StoredSeed> storedSeeds = new List<StoredSeed>();
        public int maxCitiesToGenerate;
        public int maxRiversToGenerate;
        
        public Vector2Int mapSize;
        public GameObject hexPrefab;
        
        [Separator("Properties")]
        public HexTile starterTile;

        public enum DebugColorMap
        {
            Height,
            Hill,
            Percipitation,
            Temperature,
        }
        
        [Serializable]
        public struct StoredSeed
        {
            public string name;
            public int seed;
        }
        
        [Header("DEBUG")] 
        public bool enableNoiseMapDebugColors = false;
        public DebugColorMap debugColorMap = DebugColorMap.Height;
        public bool removeNoiseDebug = false;
        public GameObject debugPoint;
        
        private List<GameObject> debugPoints = new List<GameObject>();
        
        private HexTile[,] _tileComponents;
        
        private GameObject[,] _tileGameObjects;

        private PerlinNoiseMap _perlinNoiseMap;
    
        private MaterialStorage _materialStorage;
        private MeshStorage _meshStorage;

        private CityGenerator _cityGenerator;
        public CityGenerator CityGenerator
        {
            get => _cityGenerator;
            set => _cityGenerator = value;
        }

        private RiverGenerator _riverGenerator;
        private Material _getMaterialFromNoiseValue;

        public Dictionary<string, Material> MaterialsDictionary = new Dictionary<string, Material>();
        public HexTile[,] TileComponents => _tileComponents;
        public GameObject[,] TileGameObjects => _tileGameObjects;

        private void Awake()
        {
            if (randomSeed)
            {
                mapSeed = (int)DateTime.UtcNow.Ticks;
            }
            _perlinNoiseMap = new PerlinNoiseMap(mapSeed, mapSize);
            _materialStorage = GetComponent<MaterialStorage>();
            _meshStorage = GetComponent<MeshStorage>();
            GenerateBoardBase();

            _cityGenerator = new CityGenerator(_tileComponents,4,maxCitiesToGenerate);
            Debug.Log("CITIES: "+_cityGenerator.Cities.Count);
            
            _riverGenerator = new RiverGenerator(_tileComponents,2,maxRiversToGenerate);
            Debug.Log("RIVERS: "+_riverGenerator.RiverTiles.Keys.Count);

            OptimizeMeshesAfterRiverGen();

            CreateLakeFromLoopingRivers();


            // foreach (HexTile tile in _tileComponents)
            // {
            //     if (tile.IsRiver)
            //     {
            //         Instantiate(debugPoint,tile.transform.position,Quaternion.identity);
            //     }
            // }


            //DebugShowRange(6);

        }

        private void Update()
        {
            if (enableNoiseMapDebugColors)
            {
                DebugColorize();
                enableNoiseMapDebugColors = false;
            }

            if (removeNoiseDebug)
            {
                for (int i = 0; i < debugPoints.Count; i++)
                {
                    Destroy(debugPoints[i]);
                }
                removeNoiseDebug = false;
            }
            
        }

        private void GenerateBoardBase()
        {
            Transform hexParent = new GameObject("Hexagons").transform;
            hexParent.parent = transform;

            _tileGameObjects = new GameObject[mapSize.x, mapSize.y];
            _tileComponents = new HexTile[mapSize.x, mapSize.y];
            for (int q = 0; q < mapSize.x; q++)
            {
                for (int r = 0; r < mapSize.y; r++)
                {
                    float x = Mathf.Sqrt(3) * (q + 0.5f);
                    float z = 1.5f * r;

                    // Offset for odd rows
                    if (r % 2 != 0)
                    {
                        x += Mathf.Sqrt(3) / 2f;
                    }

                    Vector3 hexPosition = new Vector3(x, 0f, z);
                    _tileGameObjects[q, r] = Instantiate(hexPrefab, hexPosition, Quaternion.identity, hexParent);
                    HexTile hexTile = _tileGameObjects[q, r].GetComponent<HexTile>();
                    _tileComponents[q, r] = hexTile;
                    hexTile.Position = new Vector2Int(q, r); // Oszlop - Sort csinál..
                    //hexTile.RealWorldPosition = hexPosition; // Nem jó!!
                    List<float> noiseMapValues = new List<float> { _perlinNoiseMap.HeightMap[q,r], _perlinNoiseMap.HillMap[q,r]};
                
                    // Set Attributes
                    hexTile.HeightType = HeightTypeRestrictions.GetTypeFromNoise(noiseMapValues);
                    hexTile.PrecipitaionType = PrecipitationTypeRestrictions.GetTypeFromNoise(_perlinNoiseMap.PrecipitationMap[q, r]);
                    hexTile.TemperatureType = TemperatureTypeRestrictions.GetTypeFromNoise(_perlinNoiseMap.TemperatureMap[q, r]);
                    hexTile.BiomeType = BiomeTypeRestrictions.GetTypeFromNoise(_perlinNoiseMap.TemperatureMap[q, r],_perlinNoiseMap.PrecipitationMap[q, r]);
                
                    ApplyMaterial(_tileGameObjects[q,r],noiseMapValues);
                    ApplyMesh(_tileGameObjects[q,r]);
                }
            }
        
            // After, offset the container to center the map.
            Vector3 offset = _tileGameObjects[_tileGameObjects.GetLength(0) - 1, _tileGameObjects.GetLength(1) - 1].transform.position;
            transform.position = -offset / 2;
            
            // Need to set NearbyTiles for every tile.
            SetNearbyTiles();

            // Set DistFromWater for every tile.
            foreach (HexTile tile in _tileComponents) { tile.GenerateDistFromWater(); }
            
        }

        // Helpers 
        private void SetNearbyTiles()
        {
            for (int q = 0; q < _tileComponents.GetLength(0); q++)
            {
                for (int r = 0; r < _tileComponents.GetLength(1); r++)
                {
                    HexTile hexTile = _tileComponents[q, r];
                    List<HexTile.NearbyTile> nearbyTiles = new List<HexTile.NearbyTile>();
                    try
                    {
                        // Top right, dir: 0
                        nearbyTiles.Add(hexTile.Position.y % 2 == 0
                            ? new HexTile.NearbyTile(3, _tileComponents[q, r + 1])
                            : new HexTile.NearbyTile(3, _tileComponents[q + 1, r + 1]));
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        // Right
                        nearbyTiles.Add(new HexTile.NearbyTile(4, _tileComponents[q + 1, r]));
                    }
                    catch
                    {
                        // ignored
                    }
                    try
                    {
                        // Bottom right
                        nearbyTiles.Add(hexTile.Position.y % 2 == 0
                            ? new HexTile.NearbyTile(5, _tileComponents[q, r - 1])
                            : new HexTile.NearbyTile(5, _tileComponents[q + 1, r - 1]));
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        // Bottom left
                        nearbyTiles.Add(hexTile.Position.y % 2 == 0
                            ? new HexTile.NearbyTile(0, _tileComponents[q - 1, r - 1])
                            : new HexTile.NearbyTile(0, _tileComponents[q, r - 1]));
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        // Left
                        nearbyTiles.Add(new HexTile.NearbyTile(1, _tileComponents[q - 1, r]));
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        // Top left
                        nearbyTiles.Add(hexTile.Position.y % 2 == 0
                            ? new HexTile.NearbyTile(2, _tileComponents[q - 1, r + 1])
                            : new HexTile.NearbyTile(2, _tileComponents[q, r + 1]));
                    }
                    catch
                    {
                        // ignored
                    }

                    hexTile.nearbyTiles = nearbyTiles;
                    
                }
            }
        }
        
        // Mesh Helpers
        private void ApplyMesh(GameObject tile)
        {
            tile.GetComponent<MeshFilter>().mesh = GetMeshFromHeight(tile.GetComponent<HexTile>());
        }
        private Mesh GetMeshFromHeight(HexTile hexTile)
        {
            return hexTile.HeightType switch
            {
                HeightType.DeepWater => _meshStorage.waterDeep,
                HeightType.MidWater => _meshStorage.waterMid,
                HeightType.ShallowWater => _meshStorage.waterShallow,
                HeightType.Flat => _meshStorage.flatBase,
                HeightType.Hill => _meshStorage.hillBase,
                HeightType.Mountain => _meshStorage.GetRandomMountain(),
                _ => _meshStorage.flatBase
            };
        }

        private void OptimizeMeshesAfterRiverGen()
        {
            for (int x = 0; x < _tileGameObjects.GetLength(0); x++)
            {
                for (int z = 0; z < _tileGameObjects.GetLength(1); z++)
                {
                    if (_tileComponents[x, z].IsRiver)
                    {
                        if (_tileComponents[x, z].HeightType == HeightType.Hill)
                        {
                            _tileComponents[x, z].GetComponent<MeshFilter>().mesh = _meshStorage.hillRiverBase;

                        } else {
                            _tileComponents[x, z].GetComponent<MeshFilter>().mesh = _meshStorage.flatRiverBase;
                        }
                    }
                    else
                    {
                        if (_tileComponents[x, z].nearbyTiles.Any(tile => tile.hexTile.HeightType != _tileComponents[x, z].HeightType || !tile.hexTile.IsDiscovered))
                        {
                            if (_tileComponents[x, z].HeightType != HeightType.Mountain)
                            {
                                _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterials = new[]
                                {
                                    _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterial,
                                    _materialStorage.sideMat
                                };
                            }
                            else
                            {
                                _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterials = new[]
                                {
                                    _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterial
                                };
                            }
                            
                        }
                        else if(!_tileComponents[x,z].IsRiver)
                        {
                            _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterials = new[]
                            {
                                _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterial
                            };
                        }
                        
                        if (_tileComponents[x, z].IsWater())
                        {
                            _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterials = new[]
                            {
                                _tileComponents[x, z].GetComponent<MeshRenderer>().sharedMaterial
                            };
                        }
                    }
                }
            }
        }

        private void CreateLakeFromLoopingRivers()
        {
            foreach (HexTile tile in _tileComponents)
            {
                if (tile.nearbyTiles.TrueForAll(nearbyTile => nearbyTile.hexTile.IsRiver && !nearbyTile.hexTile.IsCity))
                {
                    tile.GetComponent<MeshFilter>().mesh = _meshStorage.waterShallow;
                    tile.GetComponent<MeshRenderer>().material = _materialStorage.waterMat;
                    tile.HeightType = HeightType.ShallowWater;
                    
                    tile.nearbyTiles.ForEach(nearbyTile => nearbyTile.hexTile.GetComponent<MeshFilter>().mesh = _meshStorage.waterShallow);
                    tile.nearbyTiles.ForEach(nearbyTile => nearbyTile.hexTile.GetComponent<MeshRenderer>().materials = new []{_materialStorage.waterMat});
                    tile.nearbyTiles.ForEach(nearbyTile => nearbyTile.hexTile.SetRiver(false));
                    tile.nearbyTiles.ForEach(nearbyTile => nearbyTile.hexTile.HeightType = HeightType.ShallowWater);
                    
                }
            }

            foreach (HexTile hexTile in _tileComponents)
            {
                hexTile.materialStorage = _materialStorage;
                hexTile.UpdateTileMaterials();
            }
        }
        
        // Material Helpers
        private Material GetMaterialFromNoiseValue(HexTile hexTile)
        {
            return hexTile.HeightType switch
            {
                HeightType.DeepWater => _materialStorage.deepWaterMat,
                HeightType.MidWater => _materialStorage.waterMat,
                HeightType.ShallowWater => _materialStorage.shallowWaterMat,
                HeightType.Mountain => _materialStorage.mountainMat,
                _ => hexTile.BiomeType switch
                {
                    BiomeType.RainForest => _materialStorage.rainForestMat,
                    BiomeType.TemperateRainForest => _materialStorage.temperateRainForestMat,
                    BiomeType.Swamp => _materialStorage.swampMat,
                    BiomeType.SeasonalForest => _materialStorage.seasonalForestMat,
                    BiomeType.Forest => _materialStorage.forestMat,
                    BiomeType.Savanna => _materialStorage.savannaMat,
                    BiomeType.Shrubland => _materialStorage.shrublandMat,
                    BiomeType.Taiga => _materialStorage.taigaMat,
                    BiomeType.Desert => _materialStorage.desertMat,
                    BiomeType.Plains => _materialStorage.plainsMat,
                    BiomeType.IceDesert => _materialStorage.iceDesertMat,
                    BiomeType.Tundra => _materialStorage.tundrMat,
                    _ => _materialStorage.iceDesertMat
                }
            };
        }
        private void ApplyMaterial(GameObject tile, List<float> noiseMapValues)
        {
            Material[] materials = tile.GetComponent<MeshRenderer>().sharedMaterials; 
            
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = GetMaterialFromNoiseValue(tile.GetComponent<HexTile>());
            }

            if (tile.GetComponent<HexTile>().HeightType != HeightType.Mountain)
            {
                materials[2] = _materialStorage.sideMat;
            }
            
            
            tile.GetComponent<MeshRenderer>().sharedMaterials = materials;


            //tile.GetComponent<MeshRenderer>().materials.All(_ => GetMaterialFromNoiseValue(tile.GetComponent<HexTile>()));
            //tile.GetComponent<Renderer>().material = GetMaterialFromNoiseValue(tile.GetComponent<HexTile>());
        }

        public void ConvertTileToLake(HexTile tile)
        {
            tile.GetComponent<MeshFilter>().mesh = _meshStorage.waterShallow;
            tile.GetComponent<MeshRenderer>().material = _materialStorage.waterMat;
            tile.HeightType = HeightType.ShallowWater;
        }

        /*private void AssingMaterialDictionary(Material[] materials)
        {
            MaterialsDictionary.Add("Top", materials[0]);
            MaterialsDictionary.Add("R_Mid", materials[1]);
            MaterialsDictionary.Add("Side", materials[2]);
            MaterialsDictionary.Add("R_3", materials[3]);
            MaterialsDictionary.Add("R_4", materials[4]);
            MaterialsDictionary.Add("R_5", materials[5]);
            MaterialsDictionary.Add("R_0", materials[6]);
            MaterialsDictionary.Add("R_1", materials[7]);
            MaterialsDictionary.Add("R_2", materials[8]);
            MaterialsDictionary.Add("R_M_0", materials[9]);
            MaterialsDictionary.Add("R_M_5", materials[10]);
            MaterialsDictionary.Add("R_M_4", materials[11]);
            MaterialsDictionary.Add("R_M_3", materials[12]);
            MaterialsDictionary.Add("R_M_2", materials[13]);
            MaterialsDictionary.Add("R_M_1", materials[14]);
        }*/
    
        // Debug Helpers
        
        private void DebugColorize()
        {
            float[,] map = _perlinNoiseMap.HeightMap;

            switch (debugColorMap)
            {
                case DebugColorMap.Height:
                    map = _perlinNoiseMap.HeightMap;
                    break;
                case DebugColorMap.Hill:
                    map = _perlinNoiseMap.HillMap;
                    break;
                case DebugColorMap.Percipitation:
                    map = _perlinNoiseMap.PrecipitationMap;
                    break;
                case DebugColorMap.Temperature:
                    map = _perlinNoiseMap.TemperatureMap;
                    break;
            }
            
            
            foreach (GameObject tile in _tileGameObjects)
            {
                HexTile hexTile = tile.GetComponent<HexTile>(); 
                GameObject point = Instantiate(debugPoint,new Vector3(hexTile.transform.position.x,hexTile.transform.position.y+10,hexTile.transform.position.z),Quaternion.identity);
                point.GetComponent<MeshRenderer>().material.color = Color.LerpUnclamped(Color.black, Color.white,
                    map[hexTile.Position.x, hexTile.Position.y]);
                debugPoints.Add(point);
            }
        }
        private void DebugShowRange(int range)
        {
            foreach (HexTile hexTile in _tileComponents[20,20].GetTilesInRange(range))
            {
                hexTile.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        
        [ButtonMethod(ButtonMethodDrawOrder.BeforeInspector)]
        private void AddNewSeed()
        {
            var element = new StoredSeed
            {
                name = "Seed",
                seed = mapSeed
            };
            
            storedSeeds.Add(element);
        }
    }
}
