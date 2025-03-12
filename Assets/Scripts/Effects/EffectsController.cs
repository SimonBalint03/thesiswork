using System;
using Hexes;
using Hexes.TileType;
using Managers;
using MyBox;
using Singletons;
using UnityEngine;

namespace Effects
{
    public class EffectsController : MonoBehaviour
    {
        public Camera mainCamera;

        [SerializeField] private HexTile currentTileHovered;
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GetComponent<GameManager>();
        }

        private void OnEnable()
        {
            Player.OnSingleMoveEvent += OnSingleMoveEvent;
        }

        private void OnDisable()
        {
            Player.OnSingleMoveEvent -= OnSingleMoveEvent;
        }

        private void Update()
        {
            DetectObjectUnderMouse();
            Player.Instance.currentTile.EnableHighlight(Color.yellow);
        }

        void DetectObjectUnderMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has the required component
                HexTile tile = hit.collider.gameObject.GetComponent<HexTile>();
                if (tile)
                {
                    //if (tile == currentTileHovered) { return; } // If it's the same as the current tile, return.
                    if (!currentTileHovered) { currentTileHovered = tile; } // For the initial tile.

                    if (tile.Visibility == HexTile.TileVisibility.Hidden || tile.HeightType == HeightType.Mountain)
                    {
                        currentTileHovered.DisableHighlight(15f);
                        currentTileHovered = tile;
                        _gameManager.SetSelectedTile(currentTileHovered);
                        return;
                    }
                    
                    currentTileHovered.DisableHighlight(15f);
                    currentTileHovered = tile;
                    _gameManager.SetSelectedTile(currentTileHovered);
                    currentTileHovered.EnableHighlight(Color.white);
                }
            }
        }

        private void OnSingleMoveEvent(HexTile from, HexTile to)
        {
            Player.Instance.currentTile.EnableHighlight(Color.yellow);
            from.DisableHighlight(15f);
        }
    }
}
