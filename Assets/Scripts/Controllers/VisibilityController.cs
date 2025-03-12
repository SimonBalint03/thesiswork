using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using Managers;
using Singletons;
using UnityEngine;

public class VisibilityController : MonoBehaviour
{
    public HexBoard map;

    private GameManager _gameManager;

    private void Awake()
    {
        foreach (HexTile tile in map.TileComponents)
        {
            tile.SetVisibility(false);
        }
    }
}
