using System;
using System.Collections.Generic;
using System.Linq;
using Hexes;
using UnityEngine;

namespace Singletons
{
    public class Map : MonoBehaviour
    {
        public class MapTile
        {
            private Vector2Int position;
            private float price;
            private HexTile tile;
            
            public MapTile(HexTile tile)
            {
                this.tile = tile;
                position = this.tile.Position;
                price = this.tile.GetPriceOnMap();
            }

            public Vector2Int Position
            {
                get => position;
                set => position = value;
            }

            public float Price
            {
                get => price;
                set => price = value;
            }

            public HexTile Tile
            {
                get => tile;
                set => tile = value;
            }
        }
        
        public HexBoard board;
        
        public List<MapTile> mapTiles = new List<MapTile>();
        
        public delegate void SellMapAction(Map map);
        public static event SellMapAction OnSellMapEvent;
        public delegate void AfterSellMapAction(Map map);
        public static event AfterSellMapAction AfterSellEvent;


        private void OnEnable()
        {
            HexTile.OnTileVisibilityChanged += OnTileVisibilityChanged;
        }

        private void OnDisable()
        {
            HexTile.OnTileVisibilityChanged -= OnTileVisibilityChanged;
        }

        private void OnTileVisibilityChanged(HexTile tile, HexTile.TileVisibility visibility, bool shouldDiscover)
        {
            // Add newly discovered tiles.
            if (visibility == HexTile.TileVisibility.Active && shouldDiscover && !tile.IsSold)
            {
                if (mapTiles.All(mapTile => mapTile.Tile != tile))
                {
                    mapTiles.Add(new MapTile(tile));
                    //Debug.Log("Added tile to Map" + tile.Position);
                }
            }
        }

        public void DebugSellMap()
        {
            SellMap();
        }

        public void SellMap()
        {
            if (mapTiles.Count < 1)
            {
                return;
            }
            
            foreach (MapTile tile in mapTiles)
            {
                tile.Tile.IsSold = true;
            }
            
            OnSellMapEvent?.Invoke(this);
            
            mapTiles = new List<MapTile>();
            
            AfterSellEvent?.Invoke(this);
            
        }

        public float GetTotalPrice()
        {
            return mapTiles.Sum(tile => tile.Price);
        }
    }
}
