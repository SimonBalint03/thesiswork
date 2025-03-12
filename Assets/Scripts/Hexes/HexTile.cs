using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hexes.TileType;
using MyBox;
using Storage;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Hexes
{


    public class HexTile : MonoBehaviour
    {
        private static readonly int Hidden = Shader.PropertyToID("_Hidden");
        private static readonly int Inactive = Shader.PropertyToID("Inactive");
        private static readonly int Active = Shader.PropertyToID("Active");

        [Serializable]
        public class NearbyTile : IComparable<NearbyTile>
        {
            public int direction;
            public HexTile hexTile;

            public NearbyTile(int direction, HexTile hexTile)
            {
                this.direction = direction;
                this.hexTile = hexTile;
            }

            protected bool Equals(NearbyTile other)
            {
                return direction == other.direction && Equals(hexTile, other.hexTile);
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((NearbyTile)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(direction, hexTile);
            }

            public int CompareTo(NearbyTile other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (other is null) return 1;
                return direction.CompareTo(other.direction);
            }
        }

        public enum TileVisibility
        {
            Hidden = 0,
            Inactive = 1,
            Active = 2,
        }
        
        public enum TileDecoration
        {
            None,
            Cave,
            Vulcan,
            Waterfall,
            HotSpring,
            GiantTree,
            Beehive,
            WildHorses,
            WildBears,
            RitualStones
        }

        private Vector3 _realWorldPosition;

        [Separator("Base")] [SerializeField] private Vector2Int position;
        public List<NearbyTile> nearbyTiles;
        [SerializeField] [InitializationField] private HeightType heightType;
        [SerializeField] [InitializationField] private TemperatureType temperatureType;
        [SerializeField] [InitializationField] private PrecipitaionType precipitaionType;
        [SerializeField] [InitializationField] private BiomeType biomeType;

        [Separator("Water")] [SerializeField] private bool isRiver;
        [SerializeField] [ConditionalField(nameof(isRiver))] private int distFromWater = 0;
        [SerializeField] [ConditionalField(nameof(isRiver))] private List<int> directionsOfWater = new List<int>();
        [SerializeField] [ConditionalField(nameof(isRiver))] private bool isRiverStart;
        

        [Separator("City")] [SerializeField] private bool isCity;
        [SerializeField] [ConditionalField(nameof(isCity))] private City city;

        [Separator("Decoration")] 
        [SerializeField] private bool isForest;
        [SerializeField] private TileDecoration decoration = TileDecoration.None;
        [SerializeField] private List<int> directionsOfWaterfall = new List<int>();

        [Separator("Visibility & Discovery")] 
        [SerializeField] private TileVisibility visibility;
        [SerializeField] private bool isSold = false;

        [SerializeField] private bool isDiscovered = false;

        [Separator("Movement")] [SerializeField] [InitializationField]  private double movementCost;
        [Separator("References")] public MeshRenderer visibilityMarker;
        public MeshRenderer meshRenderer;
        public GameObject highlightGameObject, decorGameObject, grassContainerGO;
        public HexGuidePoints hexGuidePoints;
        public MaterialStorage materialStorage;
        public Mesh colliderMesh;
        public Minimap minimap;

        // Events
        public delegate void TileVisibilityChanged(HexTile tile, TileVisibility visibility, bool shouldDiscover);
        public static event TileVisibilityChanged OnTileVisibilityChanged;
        public delegate void TileDiscovered(HexTile tile);
        public static event TileDiscovered OnTileDiscoveredFirstTime;
        

        private void Start()
        {
            UpdateTileMaterials();
            UpdateColliders();
            UpdateHighlightHeight();
            UpdateMovementCost();
            UpdateDecorationFlags();
            UpdateGrassRendering();
        }

        public void UpdateTileMaterials()
        {
            Material[] materials = GetComponent<MeshRenderer>().materials;
            List<Material>
                riverMaterialsList = new List<Material>(); // Store materials that influence the river texture.

            if (isRiver)
            {
                foreach (NearbyTile nearbyTile in nearbyTiles)
                {

                    if (nearbyTile.hexTile.isRiver || nearbyTile.hexTile.IsWater() || nearbyTile.hexTile.IsRiverStart)
                    {
                        switch (nearbyTile.direction)
                        {
                            case 0:
                                riverMaterialsList.Add(materials[6]);
                                riverMaterialsList.Add(materials[13]);
                                riverMaterialsList.Add(materials[9]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                            case 1:
                                riverMaterialsList.Add(materials[7]);
                                riverMaterialsList.Add(materials[14]);
                                riverMaterialsList.Add(materials[13]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                            case 2:
                                riverMaterialsList.Add(materials[8]);
                                riverMaterialsList.Add(materials[14]);
                                riverMaterialsList.Add(materials[12]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                            case 3:
                                riverMaterialsList.Add(materials[3]);
                                riverMaterialsList.Add(materials[12]);
                                riverMaterialsList.Add(materials[11]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                            case 4:
                                riverMaterialsList.Add(materials[4]);
                                riverMaterialsList.Add(materials[11]);
                                riverMaterialsList.Add(materials[10]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                            case 5:
                                riverMaterialsList.Add(materials[5]);
                                riverMaterialsList.Add(materials[10]);
                                riverMaterialsList.Add(materials[9]);
                                if(!directionsOfWater.Contains(nearbyTile.direction))
                                {
                                    directionsOfWater.Add(nearbyTile.direction);
                                }
                                break;
                        }
                    }
                }

                riverMaterialsList.Add(materials[1]);

                for (var i = 0; i < materials.Length; i++)
                {
                    if (riverMaterialsList.Contains(materials[i]))
                    {
                        materials[i] = materialStorage.riverMat;
                    }
                }

                GetComponent<MeshRenderer>().sharedMaterials = materials;
            }
        }
        private void UpdateColliders()
        {
            GetComponent<MeshCollider>().sharedMesh = colliderMesh;
        }
        private void UpdateHighlightHeight()
        {
            highlightGameObject.transform.localPosition = new Vector3(
                highlightGameObject.transform.localPosition.x,
                GetPropHeight() + 0.1f,
                highlightGameObject.transform.localPosition.z);
        }
        private void UpdateMovementCost()
        {
            switch (heightType)
            {
                case HeightType.DeepWater:
                    movementCost = 1;
                    break;
                case HeightType.MidWater:
                    movementCost = 1;
                    break;
                case HeightType.ShallowWater:
                    movementCost = 1;
                    break;
                case HeightType.Flat:
                    movementCost = isRiver ? 1.5 : 1;
                    break;
                case HeightType.Hill:
                    movementCost = isRiver ? 1.75 : 1.5;
                    break;
                case HeightType.Mountain:
                    movementCost = Double.NaN;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateGrassRendering()
        {
            if (IsWater() || heightType == HeightType.Mountain)
            {
                grassContainerGO.SetActive(false);
                return;
            }

            grassContainerGO.transform.localPosition = new Vector3(0, 0 + GetPropHeight(), 0);
        }
        private void UpdateDecorationFlags()
        {
            // Forest
            if (biomeType is BiomeType.Forest or BiomeType.SeasonalForest 
                && heightType != HeightType.Mountain 
                && !IsWater())
            {
                isForest = true;
            }
            else
            {
                isForest = false;
            }
            // Waterfall
            if (decoration == TileDecoration.None && isRiver)
            {
                foreach (NearbyTile nearbyTile in nearbyTiles)
                {
                    if (directionsOfWater.Contains(nearbyTile.direction))
                    {
                        if (nearbyTile.hexTile.GetPropHeight() < GetPropHeight() && (!nearbyTile.hexTile.IsWater() || heightType == HeightType.Hill))
                        {
                            directionsOfWaterfall.Add(nearbyTile.direction);
                            decoration = TileDecoration.Waterfall;
                        }
                    }
                }
            }
            // Beehive
            if (decoration == TileDecoration.None && biomeType is BiomeType.SeasonalForest or BiomeType.RainForest or BiomeType.TemperateRainForest) {
                if (Random.Range(0f,1f)<0.015f)
                {
                    decoration = TileDecoration.Beehive;
                }
            }
            // Cave
            if (decoration == TileDecoration.None && heightType == HeightType.Mountain)
            {
                if (Random.Range(0f,1f)<0.02f)
                {
                    decoration = TileDecoration.Cave;
                }
            }
            // Ritual stones
            if (decoration == TileDecoration.None && heightType == HeightType.Flat && biomeType == BiomeType.Plains)
            {
                if (Random.Range(0f,1f)<0.01f)
                {
                    decoration = TileDecoration.RitualStones;
                }
            }
            // Giant tree
            if (decoration == TileDecoration.None &&
                heightType is HeightType.Flat or HeightType.Hill &&
                !isRiver &&
                !isCity &&
                biomeType is BiomeType.SeasonalForest or BiomeType.RainForest or BiomeType.TemperateRainForest)
            {
                if (Random.Range(0f,1f)<0.01f)
                {
                    decoration = TileDecoration.GiantTree;
                }
            }
            
            
            
        }
        public void GenerateDistFromWater()
        {
            if (IsWater()) return;
            int tries = 0;
            do
            {
                tries++;
                if (GetTilesInRange(tries).Any(tile => tile.IsWater()))
                {
                    distFromWater = tries;
                    break;
                }
                
            } while (tries < 40);
        }
        #region Constructor & Properties

        public HexTile(Vector3 realWorldPosition, TileVisibility visibility, Vector2Int position, HeightType heightType, TemperatureType temperatureType, PrecipitaionType precipitaionType, BiomeType biomeType, bool isRiver, bool isCity, City city, bool isForest, int distFromWater, List<NearbyTile> nearbyTiles)
        {
            _realWorldPosition = realWorldPosition;
            this.visibility = visibility;
            this.position = position;
            this.heightType = heightType;
            this.temperatureType = temperatureType;
            this.precipitaionType = precipitaionType;
            this.biomeType = biomeType;
            this.isRiver = isRiver;
            this.isCity = isCity;
            this.city = city;
            this.isForest = isForest;
            this.distFromWater = distFromWater;
            this.nearbyTiles = nearbyTiles;
        }

        public Vector3 RealWorldPosition
        {
            get => _realWorldPosition;
            set => _realWorldPosition = value;
        }

        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        public HeightType HeightType
        {
            get => heightType;
            set => heightType = value;
        }

        public TemperatureType TemperatureType
        {
            get => temperatureType;
            set => temperatureType = value;
        }

        public PrecipitaionType PrecipitaionType
        {
            get => precipitaionType;
            set => precipitaionType = value;
        }

        public BiomeType BiomeType
        {
            get => biomeType;
            set => biomeType = value;
        }

        public bool IsRiver
        {
            get => isRiver;
            set => isRiver = value;
        }

        public bool IsRiverStart
        {
            get => isRiverStart;
            set => isRiverStart = value;
        }

        public bool IsCity
        {
            get => isCity;
            set => isCity = value;
        }
        
        public City City
        {
            get => city;
            set => city = value;
        }

        public bool IsForest
        {
            get => isForest;
            set => isForest = value;
        }

        public bool IsDiscovered
        {
            get => isDiscovered;
            set => isDiscovered = value;
        }

        public int DistFromWater
        {
            get => distFromWater;
            set => distFromWater = value;
        }

        public TileVisibility Visibility
        {
            get => visibility;
            set => visibility = value;
        }

        public double MovementCost
        {
            get => movementCost;
            set => movementCost = value;
        }

        public List<int> DirectionsOfWater
        {
            get => directionsOfWater;
            set => directionsOfWater = value;
        }

        public bool IsSold
        {
            get => isSold;
            set => isSold = value;
        }

        public TileDecoration Decoration
        {
            get => decoration;
            set => decoration = value;
        }

        public List<int> DirectionsOfWaterfall
        {
            get => directionsOfWaterfall;
            set => directionsOfWaterfall = value;
        }

        public int CompareTo(HexTile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var heightTypeComparison = heightType.CompareTo(other.heightType);
            if (heightTypeComparison != 0) return heightTypeComparison;
            var temperatureTypeComparison = temperatureType.CompareTo(other.temperatureType);
            if (temperatureTypeComparison != 0) return temperatureTypeComparison;
            var precipitaionTypeComparison = precipitaionType.CompareTo(other.precipitaionType);
            if (precipitaionTypeComparison != 0) return precipitaionTypeComparison;
            var biomeTypeComparison = biomeType.CompareTo(other.biomeType);
            if (biomeTypeComparison != 0) return biomeTypeComparison;
            var isRiverComparison = isRiver.CompareTo(other.isRiver);
            if (isRiverComparison != 0) return isRiverComparison;
            var isCityComparison = isCity.CompareTo(other.isCity);
            if (isCityComparison != 0) return isCityComparison;
            var isForestComparison = isForest.CompareTo(other.isForest);
            if (isForestComparison != 0) return isForestComparison;
            return distFromWater.CompareTo(other.distFromWater);
        }

        protected bool Equals(HexTile other)
        {
            return base.Equals(other) && position.Equals(other.position);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HexTile)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), position);
        }

        #endregion
        public List<HexTile> GetTilesInRange(int range)
        {
            HashSet<HexTile> tilesInRange = new HashSet<HexTile>(); // Use HashSet to avoid duplicates
            Queue<HexTile> queue = new Queue<HexTile>(); // Queue for BFS
            Dictionary<HexTile, int> tileDistances = new Dictionary<HexTile, int>(); // Store distances from the start tile

            queue.Enqueue(this);
            tileDistances[this] = 0;

            while (queue.Count > 0)
            {
                HexTile currentTile = queue.Dequeue();
                int currentDistance = tileDistances[currentTile];

                if (currentDistance < range)
                {
                    foreach (NearbyTile nearbyTile in currentTile.nearbyTiles)
                    {
                        HexTile neighbor = nearbyTile.hexTile;

                        if (!tileDistances.ContainsKey(neighbor)) // If the neighbor hasn't been visited yet
                        {
                            queue.Enqueue(neighbor);
                            tileDistances[neighbor] = currentDistance + 1;
                            tilesInRange.Add(neighbor);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            tilesInRange.Add(this);
            return new List<HexTile>(tilesInRange);
        }
        public bool IsWater()
        {
            return heightType is HeightType.ShallowWater or HeightType.MidWater or HeightType.DeepWater;
        }
        public void SetRiver(bool value)
        {
            isRiver = value;
        }
        public void SetVisibility(TileVisibility tileVisibility,bool shouldDiscover = true)
        {
            visibility = tileVisibility;
            SetVisibility(shouldDiscover);
        }
        public void SetVisibility(bool shouldDiscover = true)
        {
            if (shouldDiscover)
            {
                if (!isDiscovered) { OnTileDiscoveredFirstTime?.Invoke(this); }
                isDiscovered = true;
            }
            
            switch (visibility)
            {
                case TileVisibility.Hidden:
                    // Rendering
                    visibilityMarker.enabled = true;
                    meshRenderer.enabled = false;
                    decorGameObject.SetActive(false);
                    // Minimap
                    minimap.gameObject.SetActive(false);
                    
                    OnTileVisibilityChanged?.Invoke(this,TileVisibility.Hidden, shouldDiscover);
                    break;
                case TileVisibility.Inactive:
                    // Rendering
                    visibilityMarker.enabled = false;
                    meshRenderer.enabled = true;
                    decorGameObject.SetActive(true);
                    // Minimap
                    minimap.gameObject.SetActive(true);
                    
                    OnTileVisibilityChanged?.Invoke(this,TileVisibility.Inactive, shouldDiscover);
                    break;
                case TileVisibility.Active:
                    // Rendering
                    visibilityMarker.enabled = false;
                    meshRenderer.enabled = true;
                    decorGameObject.SetActive(true);
                    // Minimap
                    minimap.gameObject.SetActive(true);
                    
                    OnTileVisibilityChanged?.Invoke(this,TileVisibility.Active, shouldDiscover);
                    break;
                default:
                    visibilityMarker.enabled = true;
                    meshRenderer.enabled = false;
                    decorGameObject.SetActive(false);
                    break;
            }
            
            
        }
        public void EnableHighlight (Color color)
        {
            StopAllCoroutines();
            SpriteRenderer highlight = highlightGameObject.GetComponent<SpriteRenderer>();
            highlight.color = color;
            highlight.enabled = true;
        }
        public void DisableHighlight(float fadeSpeed)
        {
            StopCoroutine(DisableHighlightCoroutine(fadeSpeed));
            StartCoroutine(DisableHighlightCoroutine(fadeSpeed));
        }
        private IEnumerator DisableHighlightCoroutine(float fadeSpeed)
        {
            SpriteRenderer highlight = highlightGameObject.GetComponent<SpriteRenderer>();
            Color transparent = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0);
            while (highlight.color.a > 0.1f)
            {
                highlight.color = Color.Lerp(highlight.color,transparent, fadeSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            
            highlight.enabled = false;
        }

        public float GetPropHeight()
        {
            switch (heightType)
            {
                case HeightType.DeepWater or HeightType.MidWater or HeightType.ShallowWater:
                    return 0f;
                case HeightType.Flat:
                    return 0.2f;
                case HeightType.Hill:
                    return 0.4f;
                case HeightType.Mountain:
                    return 0.6f;
                default:
                    return 0f;
            }
        }

        public float GetPriceOnMap()
        {
            float result = 0f;
            switch (heightType)
            {
                case HeightType.DeepWater or HeightType.MidWater or HeightType.ShallowWater:
                    result += PriceStorage.WATER;
                    break;
                case HeightType.Flat:
                    result += PriceStorage.FLAT;
                    break;
                case HeightType.Hill:
                    result += PriceStorage.HILL;
                    break;
                case HeightType.Mountain:
                    result += PriceStorage.MOUNTAIN;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (BiomeType)
            {
                case BiomeType.RainForest:
                    result += PriceStorage.RAINFOREST;
                    break;
                case BiomeType.TemperateRainForest:
                    result += PriceStorage.TEMPRAINFOREST;
                    break;
                case BiomeType.Swamp:
                    result += PriceStorage.SWAMP;
                    break;
                case BiomeType.SeasonalForest:
                    result += PriceStorage.SEASONALFOREST;
                    break;
                case BiomeType.Forest:
                    result += PriceStorage.FORESTBIOME;
                    break;
                case BiomeType.Savanna:
                    result += PriceStorage.SAVANNA;
                    break;
                case BiomeType.Shrubland:
                    result += PriceStorage.SHRUBLAND;
                    break;
                case BiomeType.Taiga:
                    result += PriceStorage.TAIGA;
                    break;
                case BiomeType.Desert:
                    result += PriceStorage.DESERT;
                    break;
                case BiomeType.Plains:
                    result += PriceStorage.PLAINS;
                    break;
                case BiomeType.IceDesert:
                    result += PriceStorage.ICEDESERT;
                    break;
                case BiomeType.Tundra:
                    result += PriceStorage.TUNDRA;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isCity)
            {
                result += PriceStorage.CITY;
            }

            if (isRiver)
            {
                result += PriceStorage.RIVER;
            }

            if (isForest)
            {
                result += PriceStorage.FOREST;
            }

            switch (decoration)
            {
                case TileDecoration.None:
                    break;
                case TileDecoration.Cave:
                    result += PriceStorage.CAVE;
                    break;
                case TileDecoration.Vulcan:
                    result += PriceStorage.VULCAN; 
                    break;
                case TileDecoration.Waterfall:
                    result += PriceStorage.WATERFALL;
                    break;
                case TileDecoration.HotSpring:
                    result += PriceStorage.HOTSPRING;
                    break;
                case TileDecoration.GiantTree:
                    result += PriceStorage.GIANTTREE;
                    break;
                case TileDecoration.Beehive:
                    result += PriceStorage.BEEHIVE;
                    break;
                case TileDecoration.WildHorses:
                    result += PriceStorage.WILDHORSES;
                    break;
                case TileDecoration.WildBears:
                    result += PriceStorage.WILDBEARS;
                    break;
                case TileDecoration.RitualStones:
                    result += PriceStorage.RITUALSTONE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;

        }

        public void ConvertToLake()
        {
            //Debug.Log("Converting to Lake" + Position);
            isRiver = false;
            // VERY-VERY BAD
            HexBoard board = FindObjectOfType<HexBoard>();
            board.ConvertTileToLake(this);
        }
    }
}
