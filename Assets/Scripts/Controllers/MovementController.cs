using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aoiti.Pathfinding;
using Hexes;
using Hexes.TileType;
using Managers;
using Singletons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Controllers
{
    public class MovementController : MonoBehaviour
    {
        [Header("References")]
        public Player player;
        public HexBoard map;
        [Header("Movement Settings")]
        public float moveSpeed = 5f; // Speed of movement
        
        private Pathfinder<HexTile> pathfinder;
        private List<HexTile> pathToFollow;
        private HexTile lastTile;
        private double totalPoints;

        private GameManager _gameManager;
        private int UILayer;
        
        [SerializeField]private List<HexTile> highlightedTiles = new List<HexTile>();
        
        public delegate void PredictedPathChanged(List<HexTile> path, float remainingPoints);
        public static event PredictedPathChanged OnPredictedPathChanged;
        public delegate void StoppedMoving();
        public static event StoppedMoving OnMovementStopped;
        public delegate void StartedMoving();
        public static event StartedMoving OnMovementStarted;


        private void Awake()
        {
            // Initialize the Pathfinder with heuristic distance and neighbor fetching functions
            pathfinder = new Pathfinder<HexTile>(
                HeuristicDistance,
                GetConnectedNodesAndStepCosts
            );
            
            _gameManager = GetComponent<GameManager>();
            UILayer = LayerMask.NameToLayer("UI");

        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!IsPointerOverUIElement())
                {
                    player.TryMove(_gameManager.currentTileSelected);
                    
                } 
            }
        }

        private void OnEnable()
        {
            Player.OnMoveEvent += OnMove;
            GameManager.OnSelectedTileChanged += OnOnSelectedTileChanged;
        }

        private void OnDisable()
        {
            Player.OnMoveEvent -= OnMove;
            GameManager.OnSelectedTileChanged -= OnOnSelectedTileChanged;
        }

        private void OnOnSelectedTileChanged(HexTile tile)
        {
            CalculateAndShowPredictedPath(tile);
        }

        private void CalculateAndShowPredictedPath(HexTile tile)
        {
            foreach (HexTile hexTile in highlightedTiles)
            {
                hexTile.DisableHighlight(20f);
            }
            
            highlightedTiles = new List<HexTile>();

            if (
                tile.IsWater() ||
                tile.HeightType == HeightType.Mountain ||
                Player.Instance.currentMovementPoints < 1
            )
            {
                return;
            }
            
            if (pathfinder.GenerateAstarPath(player.currentTile, tile, out pathToFollow))
            {
                if (!PlayerCanCompletePath(pathToFollow, out lastTile,out totalPoints))
                {
                    OnPredictedPathChanged?.Invoke(pathToFollow, (float)totalPoints);
                    if (lastTile.Position == Player.Instance.currentTile.Position)
                    {
                        foreach (HexTile hexTile in pathToFollow)
                        {
                            hexTile.DisableHighlight(20f);
                        }
                        return;
                    }
                    foreach (HexTile hexTile in pathToFollow)
                    {
                        if (hexTile.Position == lastTile.Position)
                        {
                            if (!highlightedTiles.Contains(hexTile))
                            {
                                hexTile.EnableHighlight(Color.red);
                                highlightedTiles.Add(hexTile);
                            }
                            
                            break;
                        }
                        if (!highlightedTiles.Contains(hexTile))
                        {
                            hexTile.EnableHighlight(Color.grey);
                            highlightedTiles.Add(hexTile);
                        }
                    }
                    return;
                }
                //Debug.Log("Path found");
                OnPredictedPathChanged?.Invoke(pathToFollow, (float)totalPoints);

                foreach (HexTile hexTile in pathToFollow)
                {
                    if (!highlightedTiles.Contains(hexTile))
                    {
                        hexTile.EnableHighlight(Color.grey);
                        highlightedTiles.Add(hexTile);
                    }
                }
            }
            else
            {
                OnPredictedPathChanged?.Invoke(pathToFollow, (float)Player.Instance.currentMovementPoints);
                foreach (HexTile hexTile in pathToFollow)
                {
                    hexTile.DisableHighlight(20f);
                }
            }
        }

        private void OnMove(HexTile from, HexTile to)
        {
            if (pathfinder.GenerateAstarPath(player.currentTile, to, out pathToFollow))
            {
                if (!PlayerCanCompletePath(pathToFollow, out lastTile, out totalPoints))
                {
                    player.currentMovementPoints = totalPoints;
                    if (player.currentTile.Position == lastTile.Position)
                    {
                        Debug.LogWarning("Player is on the same tile as the target.");
                        return;
                    }
                    Debug.LogWarning("Player cannot complete path. Moving to the last tile the movement allows.");
                    Debug.Log("Moving from:" + from.Position + " to " + lastTile.Position);
                    StopCoroutine(FollowPath(lastTile));
                    StartCoroutine(FollowPath(lastTile));
                    return;
                }
                
                // Start the coroutine to follow the path
                //Debug.Log("Moving from:" + from.Position + " to " + to.Position);
                player.currentMovementPoints = totalPoints;
                StopCoroutine(FollowPath());
                StartCoroutine(FollowPath());
            }
            else
            {
                Debug.LogWarning("No path found");
            }
        }
        
        private IEnumerator FollowPath()
        {
            OnMovementStarted?.Invoke();
            foreach (HexTile tile in pathToFollow)
            {
                Vector3 targetPosition = map.TileGameObjects[tile.Position.x, tile.Position.y].transform.position;

                player.MoveToTransform(tile,targetPosition);
                yield return new WaitForSeconds(1f/moveSpeed);
                _gameManager.RecalculateVisibility();
                
            }
            OnMovementStopped?.Invoke();
            player.isMoving = false;
        }
        
        private IEnumerator FollowPath(HexTile tileToStopAt)
        {
            OnMovementStarted?.Invoke();
            foreach (HexTile tile in pathToFollow)
            {
                Vector3 targetPosition = map.TileGameObjects[tile.Position.x, tile.Position.y].transform.position;

                player.MoveToTransform(tile,targetPosition);
                yield return new WaitForSeconds(1f/moveSpeed);
                _gameManager.RecalculateVisibility();

                if (tile.Position == tileToStopAt.Position)
                {
                    OnMovementStopped?.Invoke();
                    break;
                } // When at the "tileToStopAt"'s position, break.
            }
            player.isMoving = false;
        }

        private bool PlayerCanCompletePath(List<HexTile> path, out HexTile lastTile, out double totalPoints)
        {
            totalPoints = player.currentMovementPoints;
            for (var i = 0; i < path.Count; i++)
            {
                var tile = path[i];
                totalPoints -= tile.MovementCost;
                if (totalPoints < 0)
                {
                    try
                    {
                        lastTile = path[i-1];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        lastTile = player.currentTile;
                    }
                    
                    totalPoints += tile.MovementCost; // Bugsolving??
                    //player.currentMovementPoints = totalPoints;
                    
                    //Debug.Log("Last tile: " + lastTile.Position);
                    //Debug.LogWarning("Not enough movement points");
                    return false;
                }
            }

            if (path.Count == 0) { lastTile = player.currentTile; return false; }
            lastTile = path[^1]; // The last in the "path" List.
            //player.currentMovementPoints = totalPoints;
            
            //Debug.Log("Movement remaining: " + totalPoints);
            return true;
        }
        
        
        // Calculate heuristic distance for odd-right hexagonal grid (2D layout)
        public float HeuristicDistance(HexTile start, HexTile target)
        {
            // Convert 2D grid coordinates (row, col) to cube coordinates for odd-r layout
            Vector2Int startCoords = start.Position; // Assuming HexTile stores 2D coords as GridCoords (row, col)
            Vector2Int targetCoords = target.Position;

            // Convert start grid coordinates to cube coordinates
            Vector3 startCube = OddRToCube(startCoords);

            // Convert target grid coordinates to cube coordinates
            Vector3 targetCube = OddRToCube(targetCoords);

            // Use the cube distance formula
            return Mathf.Max(
                Mathf.Abs(startCube.x - targetCube.x),
                Mathf.Abs(startCube.y - targetCube.y),
                Mathf.Abs(startCube.z - targetCube.z)
            );
        }

        // Convert 2D grid (row, col) to cube coordinates for odd-r layout
        private Vector3 OddRToCube(Vector2Int gridCoords)
        {
            int col = gridCoords.y;  // Column in 2D grid (x-axis in Unity is the column)
            int row = gridCoords.x;  // Row in 2D grid (y-axis in Unity is the row)

            int cubeX = col;
            int cubeZ = row - (col - (col & 1)) / 2;
            int cubeY = -cubeX - cubeZ;

            return new Vector3(cubeX, cubeY, cubeZ);
        }

        // Get neighboring tiles and step costs
        private Dictionary<HexTile, float> GetConnectedNodesAndStepCosts(HexTile current)
        {
            Dictionary<HexTile, float> neighbors = new Dictionary<HexTile, float>();

            foreach (HexTile.NearbyTile nearbyTile in current.nearbyTiles)
            {
                // Blocking conditions
                if (nearbyTile.hexTile.HeightType == HeightType.Mountain) { continue; }
                if( nearbyTile.hexTile.IsWater()) { continue; }
                if( nearbyTile.hexTile.Visibility == HexTile.TileVisibility.Hidden) { continue; }

                switch (nearbyTile.hexTile.HeightType)
                {
                    case HeightType.Flat:
                        neighbors.Add(nearbyTile.hexTile, nearbyTile.hexTile.IsRiver ? 1.5f : 1f);
                        break;
                    case HeightType.Hill:
                        neighbors.Add(nearbyTile.hexTile, nearbyTile.hexTile.IsRiver ? 1.75f : 1.25f);
                        break;
                    case HeightType.Mountain:
                        neighbors.Add(nearbyTile.hexTile,100f);
                        break;
                    default:
                        neighbors.Add(nearbyTile.hexTile,1f);
                        break;
                }
                
            }
        
            return neighbors;
        }
        //Returns 'true' if we touched or hovering on Unity UI element.
        public bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }


        //Returns 'true' if we touched or hovering on Unity UI element.
        private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == UILayer)
                    return true;
            }
            return false;
        }


        //Gets all event system raycast results of current mouse or touch position.
        static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }
    }
}
