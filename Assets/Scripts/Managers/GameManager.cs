using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Controllers;
using Database;
using Effects;
using Hexes;
using Hexes.TileType;
using Inventory;
using MyBox;
using Singletons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [Separator("Camera Settings")]
        public CinemachineVirtualCamera virtualCamera;
        private CameraController cameraController;

        [Separator("Player Settings")] 
        public GameObject playerGo;
        
        [Separator("Items & Shop")]
        public ItemDatabase itemDatabase;
        
        private Player _player;

        [Separator("Map Settings")] 
        public HexBoard map;
        
        [Separator("Movement")]
        public HexTile currentTileSelected;
        [SerializeField]private List<HexTile> _discoveredTiles = new List<HexTile>();

        [Separator("Gameplay")] 
        public bool gameOverEnabled = true;
        
        [Separator("Loading Screen")]
        public GameObject loadingScreen;
        public GameObject background,title;
        
        [Separator("Game Over")]
        public bool isGameOver;
        public GameObject gameOverScreen;
        
        private MovementController movementController;
        private EffectsController effectsController;
        
        public delegate void SelectedTileChanged(HexTile tile);
        public static event SelectedTileChanged OnSelectedTileChanged;
        public delegate void GameOverEvent();
        public static event GameOverEvent OnGameOver;
        
        
        private void Awake()
        {
            loadingScreen.SetActive(true);
            _player = playerGo.GetComponent<Player>();
            
            movementController = FindObjectOfType<MovementController>();
            effectsController = FindObjectOfType<EffectsController>();
            
            // Place player to the starter city
            foreach (HexTile tile in map.TileComponents)
            {
                if (!tile.City.Starter) continue;
                _player.MoveToTransform(tile,tile.transform.position,false);
                map.starterTile = tile; // Set the starting tile.
                break;
            }
            // Create items database
            //itemDatabase = ScriptableObject.Instantiate(itemDatabase);
            
            // Center camera on player
            cameraController = virtualCamera.GetComponent<CameraController>();
            StartCoroutine(CenterCameraOnObject(playerGo));
            
            // Start with a turn
            TimeOfDay.Instance.OnNextStepButtonClicked();
            RecalculateVisibility();

            StartCoroutine(DissolveLoadingScreen());
        }
        
        
        // Amikor kört lép a játékos.
        private void OnNextStep()
        {
            RecalculateVisibility();
        }
        private void OnMove(HexTile from, HexTile to)
        {
            Player.Instance.inventory.GetPermanentItems().ForEach(item => Player.Instance.inventory.UseItem(item));
        }

        private void OnItemUsed(Item item)
        {
            switch (item.Function)
            {
                case Item.ItemFunction.None:
                    Debug.LogWarning("Item has no function!");
                    break;
                case Item.ItemFunction.DiscoverWithin2:
                    foreach (HexTile.NearbyTile nearbyTile in _player.currentTile.nearbyTiles)
                    {
                        nearbyTile.hexTile.SetVisibility(HexTile.TileVisibility.Active);
                        foreach (var tileNearbyTile in nearbyTile.hexTile.nearbyTiles.Where(tileNearbyTile => !_player.currentTile.nearbyTiles.Contains(tileNearbyTile)))
                        {
                            tileNearbyTile.hexTile.SetVisibility(HexTile.TileVisibility.Active);
                        }
                    }
                    break;
                case Item.ItemFunction.FullEnergy:
                    Player.Instance.currentMovementPoints =
                        Player.Instance.baseMovementPoints + Player.Instance.itemBonusMovementPoints;
                    break;
                case Item.ItemFunction.SeeThroughForest:
                    Player.Instance.canSeeThroughForest = true;
                    break;
                case Item.ItemFunction.SeeThroughHill:
                    Player.Instance.canSeeThroughHill = true;
                    break;
                case Item.ItemFunction.IncreaseMovement:
                    //Player.Instance.itemBonusMovementPoints = item.IncreaseMovementAmount; // nem jo hosszu tavon
                    break;
                case Item.ItemFunction.IncreaseInventorySpace:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (_player.isMoving)
            {
                CenterCameraOnObjectNoDelay(playerGo);
            }
        }

        public void RecalculateVisibility()
        {
            foreach (var tile in _discoveredTiles.Where(tile => tile.Visibility != HexTile.TileVisibility.Inactive))
            {
                tile.SetVisibility(HexTile.TileVisibility.Inactive);
            }
            
            _player.currentTile.SetVisibility(HexTile.TileVisibility.Active);
            List<HexTile> firstCircle = new List<HexTile>();
            // First step
            _player.currentTile.nearbyTiles.ForEach(nt => firstCircle.Add(nt.hexTile));
            // foreach (var hexTile in firstCircle.Where(hexTile => !_discoveredTiles.Contains(hexTile)))
            // {
            //     _discoveredTiles.Add(hexTile);
            // }
            foreach (HexTile tile in firstCircle)
            {
                if (!_discoveredTiles.Contains(tile)) { _discoveredTiles.Add(tile); }
                
                tile.SetVisibility(HexTile.TileVisibility.Active);
                // Second step
                if (tile.IsForest && !Player.Instance.canSeeThroughForest)
                {
                    continue;
                }

                if (tile.HeightType == HeightType.Hill && !Player.Instance.canSeeThroughHill)
                {
                    continue;
                }
                
                if (tile.HeightType == HeightType.Mountain)
                {
                    continue;
                }

                foreach (var nearbyTile in tile.nearbyTiles.Where(nearbyTile => !firstCircle.Contains(nearbyTile.hexTile)))
                {
                    nearbyTile.hexTile.SetVisibility(HexTile.TileVisibility.Active);
                    if (!_discoveredTiles.Contains(nearbyTile.hexTile))
                    {
                        _discoveredTiles.Add(nearbyTile.hexTile);
                    }
                }
            }
            



            /*
             foreach (HexTile hexTile in map.TileComponents)
            {
                if (!hexTile.IsDiscovered)
                {
                    hexTile.SetVisibility(HexTile.TileVisibility.Hidden,false);
                }
                else
                {
                    hexTile.SetVisibility(HexTile.TileVisibility.Inactive);
                    _player.currentTile.SetVisibility(HexTile.TileVisibility.Active);
                    foreach (var nt in _player.currentTile.nearbyTiles)
                    {
                        nt.hexTile.SetVisibility(HexTile.TileVisibility.Active);
                        nt.hexTile.UpdateTileMaterials();
                    }
                }
            }
            */
            

        }

        private IEnumerator CenterCameraOnObject(GameObject go)
        {
            yield return new WaitForSeconds(0.5f);
            cameraController.CenterOnObject(go);
        }

        private IEnumerator DissolveLoadingScreen()
        {
            yield return new WaitForSeconds(2f);
            title.LeanMoveLocalY(800f, 1f).setEase(LeanTweenType.easeInOutQuart);
            LerpAlpha(background.GetComponent<Image>(),0f,1f);
            yield return new WaitForSeconds(1f);
            loadingScreen.SetActive(false);
        }
        
        private void CenterCameraOnObjectNoDelay(GameObject o)
        {
            cameraController.CenterOnObject(o);
        }


        public void SetSelectedTile(HexTile tile)
        {
            if (currentTileSelected)
            {
                if (currentTileSelected.Position == tile.Position)
                {
                    return;
                }
            }
            
            currentTileSelected = tile;
            OnSelectedTileChanged?.Invoke(currentTileSelected);
        }

        public void SetGameOver(bool value)
        {
            if (!gameOverEnabled)
            {
                if (value) { Debug.LogWarning("Game over disabled."); } return;
            }
            
            isGameOver = value;
            if (value)
            {
                StartCoroutine(GameOverRoutine());
                Debug.LogWarning("Game Over");
                OnGameOver?.Invoke();
            }
        }

        IEnumerator GameOverRoutine()
        {
            gameOverScreen.SetActive(true);
            gameOverScreen.transform.localScale = Vector3.zero;
            gameOverScreen.LeanScale(Vector3.one, 0.5f);
            ToggleControllersOnPause(false);
            yield return new WaitForSeconds(0.5f);
        }
        
        private void ToggleControllersOnPause(bool active)
        {
            movementController.enabled = active;
            cameraController.enabled = active;
            effectsController.enabled = active;
        }
        
        public void LerpAlpha(Image image, float targetAlpha, float duration)
        {
            StartCoroutine(LerpAlphaCoroutine(image, targetAlpha, duration));
        }

        private IEnumerator LerpAlphaCoroutine(Image image, float targetAlpha, float duration)
        {
            if (image == null) yield break;

            Color color = image.color;
            float startAlpha = color.a;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                image.color = new Color(color.r, color.g, color.b, newAlpha);
                yield return null;
            }
            
            image.color = new Color(color.r, color.g, color.b, targetAlpha);
        }
        
        #region Events

        private void OnEnable()
        {
            TimeOfDay.OnNextStep += OnNextStep;
            Player.OnMoveEvent += OnMove;
            Inventory.Inventory.OnItemUsed += OnItemUsed;
        }

        private void OnDisable()
        {
            TimeOfDay.OnNextStep -= OnNextStep;
            Player.OnMoveEvent -= OnMove;
            Inventory.Inventory.OnItemUsed -= OnItemUsed;
        }

        #endregion
        
        #region Debug

        public void RevealMap()
        {
            foreach (HexTile hexTile in map.TileComponents)
            {
                hexTile.SetVisibility(HexTile.TileVisibility.Active,false);
            }
        }
        
        public void UndoRevealMap()
        {
            foreach (HexTile hexTile in map.TileComponents)
            {
                if (!hexTile.IsDiscovered)
                {
                    hexTile.SetVisibility(HexTile.TileVisibility.Hidden,false);
                }
                else
                {
                    hexTile.SetVisibility(HexTile.TileVisibility.Inactive);
                    _player.currentTile.SetVisibility(HexTile.TileVisibility.Active);
                    _player.currentTile.nearbyTiles.ForEach(nt => nt.hexTile.SetVisibility(HexTile.TileVisibility.Active));
                }
            }
        }

        #endregion
    }
}
