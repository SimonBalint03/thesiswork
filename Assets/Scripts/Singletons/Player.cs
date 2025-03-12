using System;
using System.Collections;
using System.Linq;
using Hexes;
using Hexes.TileType;
using Inventory;
using Managers;
using MyBox;
using UnityEngine;

namespace Singletons
{
    public class Player : MonoBehaviour
    {
        
        public enum MovementState
        {
            Idle,
            Exploring,
            Returning,
        }
        public static Player Instance { get; set; }
        public GameObject character;
        [Separator("Base")]
        public HexTile currentTile;
        public bool isMoving = false;
        public int money;

        [Separator("Movement")]
        public double baseMovementPoints = 10;   // Base movement points
        public double itemBonusMovementPoints = 0; // Additional movement points from items
        public double currentMovementPoints;     // Current movement points for this turn
        public MovementState movementState = MovementState.Idle;
        public double returnPointMultiplier;
        
        [Separator("Behaviour")]
        public bool centerCameraAfterMovement = true;
        public bool canSeeThroughForest;
        public bool canSeeThroughHill;

        [Separator("Map")] 
        public Map map;
        
        [Separator("Inventory")]
        public int inventoryMaxCapacity = 3;
        public Inventory.Inventory inventory;
        
        private GameManager _gameManager;
        
        public delegate void MoveAction(HexTile from, HexTile to);
        public static event MoveAction OnMoveEvent;
        public delegate void SingleMoveAction(HexTile from, HexTile to);
        public static event SingleMoveAction OnSingleMoveEvent;
    
        private void Awake() 
        { 
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this) { Destroy(this); } else { Instance = this; }
            
            _gameManager = FindObjectOfType<GameManager>();

            inventory = new Inventory.Inventory(inventoryMaxCapacity);
        }

        private void Update()
        {
            
        }

        private void OnEnable()
        {
            TimeOfDay.OnNextStep += OnNextTurn;
            Map.OnSellMapEvent += MapOnOnSellMapEvent;
            OnSingleMoveEvent += PlayerOnSingleMoveEvent;
            Inventory.Inventory.OnItemRemoved += OnItemRemoved;
        }

        private void OnDisable()
        {
            TimeOfDay.OnNextStep -= OnNextTurn;
            Map.OnSellMapEvent -= MapOnOnSellMapEvent;
            OnSingleMoveEvent -= PlayerOnSingleMoveEvent;
            Inventory.Inventory.OnItemRemoved -= OnItemRemoved;
        }

        private void OnNextTurn()
        {
            if (currentTile.IsCity) { movementState = MovementState.Idle; }
            if (movementState == MovementState.Returning) { _gameManager.SetGameOver(true); }

            if (movementState == MovementState.Exploring)
            {
                movementState = MovementState.Returning;
                currentMovementPoints = (baseMovementPoints + itemBonusMovementPoints) * returnPointMultiplier;
            }
            else
            {
                currentMovementPoints = baseMovementPoints + itemBonusMovementPoints;
            }
        }
        
        private void MapOnOnSellMapEvent(Map soldMap)
        {
            AddMoney(soldMap.GetTotalPrice().RoundToInt());
        }
        
        private void PlayerOnSingleMoveEvent(HexTile from, HexTile to)
        {
            if (from.IsCity && !to.IsCity && movementState != MovementState.Returning)
            {
                movementState = MovementState.Exploring;
            }
        }
        
        private void OnItemRemoved(Item item)
        {
            item.isBuyable = true;
            switch (item.Function)
            {
                case Item.ItemFunction.SeeThroughForest:
                    canSeeThroughForest = false;
                    break;
                case Item.ItemFunction.SeeThroughHill:
                    canSeeThroughHill = false;
                    break;
                case Item.ItemFunction.IncreaseMovement:
                    itemBonusMovementPoints = 0;
                    foreach (Item itemInInv in inventory.Items.Where(i => i.Function == Item.ItemFunction.IncreaseMovement))
                    {
                        itemBonusMovementPoints += itemInInv.IncreaseMovementAmount;
                    }
                    break;
                case Item.ItemFunction.None:
                case Item.ItemFunction.DiscoverWithin2:
                case Item.ItemFunction.FullEnergy:
                
                default:
                    break;
            }
        }
        
        public void TryMove(HexTile target)
        {
            if (CanMove(target)) { Move(target); }
            else
            {
                Debug.LogWarning("Method CanMove is false");
            }
        }

        private static bool CanMove(HexTile target)
        {
            if (target.Visibility == HexTile.TileVisibility.Hidden) { Debug.LogWarning("Can't move: Hidden."); return false; }
            if (target.HeightType == HeightType.Mountain) { Debug.LogWarning("Can't move: Mountain."); return false; }
            if (Instance.isMoving){ Debug.LogWarning("Can't move: Already moving."); return false; }
            
            return true;
        }

        private void Move(HexTile target)
        {
            OnMoveEvent?.Invoke(currentTile, target);
        }

        public double GetTotalMovementPoints()
        {
            if (movementState == MovementState.Returning)
            {
                return (baseMovementPoints + itemBonusMovementPoints) * returnPointMultiplier;
            }
            
            return baseMovementPoints + itemBonusMovementPoints;
        }

        public void MoveToTransform(HexTile target, Vector3 position, bool moving = true)
        {
            if (currentTile) { OnSingleMoveEvent?.Invoke(currentTile, target); }
            Instance.isMoving = moving;
            Instance.currentTile = target;
    
            // Start smooth movement coroutine
            Instance.StartCoroutine(SmoothMove(target));
        }


        public void AddMoney(int moneyToAdd)
        {
            money += moneyToAdd;
        }
        
        private IEnumerator SmoothMove(HexTile target)
        {
            float duration = 0.5f; // Time in seconds
            float elapsed = 0f;
            Vector3 startPosition = Instance.transform.position;
            Vector3 targetPosition = target.transform.position + (Vector3.up * target.GetPropHeight() + Vector3.up);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                Instance.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null; // Wait for next frame
            }
            
            Instance.transform.position = targetPosition;
            //Instance.isMoving = false;
        }

    }
}
