using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using Hexes.TileType;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class DecorationController : MonoBehaviour
    {
        public HexBoard board;

        [Separator("Forests")] 
        //public GameObject[] forests;
        public GameObject[] spruce_trees;
        public GameObject[] oak_trees;
        [Range(0,1)]public float forestDensity;
        [Separator("Cities")]
        public GameObject[] bigCities;
        public GameObject[] villages;
        public GameObject[] outposts;
        [Separator("Other Decorations")] 
        public GameObject[] waterfalls;
        public GameObject[] beehives;
        public Mesh[] caves;
        public GameObject[] caveDecors;
        public GameObject[] ritualStones;
        public GameObject[] giantTrees;

        [Separator("Miscellaneous")] 
        public GameObject noObjectFound;
        
        private void Start()
        {
            PlaceTrees();
            PlaceCities();
            PlaceOtherDecors();
        }

        private void PlaceTrees()
        {
            GameObject[] trees = new GameObject[100]; // Jaj..
            
            foreach (HexTile hexTile in board.TileComponents)
            {
                // is Forest
                if (hexTile.IsForest)
                {
                    trees = hexTile.BiomeType is
                        BiomeType.SeasonalForest or
                        BiomeType.RainForest or
                        BiomeType.TemperateRainForest ? oak_trees : spruce_trees;
                    
                    if (hexTile.IsCity)
                    {
                        //foreach (GameObject treeContainer in hexTile.hexGuidePoints.riverTreeContainers) { treeContainer.SetActive(false); }
                        continue;
                    }
                    
                    // Defaults
                    for (int i = 0; i < hexTile.hexGuidePoints.defaultTreeContainer.transform.childCount; i++)
                    {
                        if(Random.Range(0f,1f) > forestDensity){continue;}
                        Transform child = hexTile.hexGuidePoints.defaultTreeContainer.transform.GetChild(i);
                        hexTile.hexGuidePoints.AddTree(
                            Instantiate(trees[Random.Range(0,trees.Length)],
                            new Vector3(child.position.x, 
                                child.position.y + hexTile.GetPropHeight(),
                                child.position.z), 
                            Quaternion.Euler(0,Random.Range(0, 360),0), 
                            hexTile.decorGameObject.transform));
                    }
                    
                    // Where the rivers could be but are not.
                    foreach (int direction in hexTile.DirectionsOfWater) 
                    {
                        hexTile.hexGuidePoints.riverTreeContainers[direction].gameObject.SetActive(false);
                    }

                    foreach (GameObject treeContainer in hexTile.hexGuidePoints.riverTreeContainers)
                    {
                        if (!treeContainer.activeSelf) { continue; }
                        for (int i = 0; i < treeContainer.transform.childCount; i++)
                        {
                            if(Random.Range(0f,1f) > forestDensity){continue;}
                            hexTile.hexGuidePoints.AddTree(
                            Instantiate(trees[Random.Range(0,trees.Length)],
                                new Vector3(treeContainer.transform.GetChild(i).position.x,
                                    treeContainer.transform.GetChild(i).position.y + hexTile.GetPropHeight(),
                                    treeContainer.transform.GetChild(i).position.z),
                                Quaternion.Euler(0,Random.Range(0, 360),0),
                                hexTile.decorGameObject.transform));
                        }
                    }
                    // For river centers
                    if (!hexTile.IsRiver)
                    {
                        for (int i = 0; i < hexTile.hexGuidePoints.riverCenterTreeContainer.transform.childCount; i++)
                        {
                            if(Random.Range(0f,1f) > forestDensity){continue;}
                            hexTile.hexGuidePoints.AddTree(
                            Instantiate(trees[Random.Range(0,trees.Length)],
                                new Vector3(hexTile.hexGuidePoints.riverCenterTreeContainer.transform.GetChild(i).position.x,
                                    hexTile.hexGuidePoints.riverCenterTreeContainer.transform.GetChild(i).position.y + hexTile.GetPropHeight(),
                                    hexTile.hexGuidePoints.riverCenterTreeContainer.transform.GetChild(i).position.z),
                                Quaternion.Euler(0,Random.Range(0, 360),0),
                                hexTile.decorGameObject.transform));
                        }
                    }
                }
            }
        }

        private void PlaceCities()
        {
            foreach (HexTile tile in board.TileComponents)
            {
                if (!tile.IsCity) { continue; }

                switch (tile.City.CityType)
                {
                    case CityType.Big:
                        Instantiate(
                            bigCities[Random.Range(0, bigCities.Length)],
                            new Vector3(
                                tile.transform.position.x,
                                tile.transform.position.y + tile.GetPropHeight(),
                                tile.transform.position.z),
                            Quaternion.identity,
                            tile.decorGameObject.transform);
                        break;
                    case CityType.Village:
                        Instantiate(
                            villages[Random.Range(0, bigCities.Length)],
                            new Vector3(
                                tile.transform.position.x,
                                tile.transform.position.y + tile.GetPropHeight(),
                                tile.transform.position.z),
                            Quaternion.identity,
                            tile.decorGameObject.transform);
                        break;
                    case CityType.Outpost:
                        Instantiate(
                            outposts[Random.Range(0, bigCities.Length)],
                            new Vector3(
                                tile.transform.position.x,
                                tile.transform.position.y + tile.GetPropHeight(),
                                tile.transform.position.z),
                            Quaternion.identity,
                            tile.decorGameObject.transform);
                        break;
                    default:
                        Debug.LogError("Something is very wrong");
                        Instantiate(
                            noObjectFound,
                            new Vector3(
                                tile.transform.position.x,
                                tile.transform.position.y + tile.GetPropHeight(),
                                tile.transform.position.z),
                            Quaternion.identity,
                            tile.decorGameObject.transform);
                        break;
                }
            }
        }
        
        private void PlaceOtherDecors()
        {
            foreach (HexTile hexTile in board.TileComponents)
            {
                switch (hexTile.Decoration)
                {
                    case HexTile.TileDecoration.None:
                        break;
                    case HexTile.TileDecoration.Cave:
                        hexTile.gameObject.GetComponent<MeshFilter>().sharedMesh = caves[Random.Range(0, caves.Length)];
                        Instantiate(caveDecors[Random.Range(0, caveDecors.Length)], hexTile.gameObject.transform.position, Quaternion.identity, hexTile.decorGameObject.transform);
                        break;
                    case HexTile.TileDecoration.Vulcan:
                        break;
                    case HexTile.TileDecoration.Waterfall:
                        foreach (int waterfallDir in hexTile.DirectionsOfWaterfall)
                        {
                            Instantiate(waterfalls[Random.Range(0, waterfalls.Length)],
                                new Vector3(hexTile.hexGuidePoints.riverEnds[waterfallDir].transform.position.x,
                                    hexTile.GetPropHeight(),
                                    hexTile.hexGuidePoints.riverEnds[waterfallDir].transform.position.z),
                                hexTile.hexGuidePoints.riverEnds[waterfallDir].transform.rotation,
                                hexTile.decorGameObject.transform);
                        }
                        break;
                    case HexTile.TileDecoration.HotSpring:
                        break;
                    case HexTile.TileDecoration.GiantTree:
                        GameObject middle = hexTile.hexGuidePoints.middle;
                        if (middle != null)
                        {
                            Instantiate(giantTrees[Random.Range(0, giantTrees.Length)],
                                new Vector3(middle.transform.position.x,
                                    middle.transform.position.y + hexTile.GetPropHeight(),
                                    middle.transform.position.z),
                                Quaternion.Euler(0,Random.Range(0, 360),0),
                                hexTile.decorGameObject.transform);
                            hexTile.hexGuidePoints.GetTrees().ForEach(t => t.gameObject.SetActive(false));
                        }
                        break;
                    case HexTile.TileDecoration.Beehive:
                        GameObject tree = hexTile.hexGuidePoints.GetRandomTree();
                        if (tree != null)
                        {
                            Instantiate(beehives[Random.Range(0, beehives.Length)],
                                new Vector3(tree.transform.position.x,
                                    tree.transform.position.y + hexTile.GetPropHeight(),
                                    tree.transform.position.z),
                                Quaternion.Euler(0,Random.Range(0, 360),0),
                                hexTile.decorGameObject.transform);
                            //Destroy(tree);
                        }
                        break;
                    case HexTile.TileDecoration.WildHorses:
                        break;
                    case HexTile.TileDecoration.WildBears:
                        break;
                    case HexTile.TileDecoration.RitualStones:
                        Instantiate(ritualStones[Random.Range(0, ritualStones.Length)],
                            new Vector3(hexTile.transform.position.x,hexTile.transform.position.y + hexTile.GetPropHeight(),hexTile.transform.position.z),
                            Quaternion.identity,
                            hexTile.decorGameObject.transform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
