using System;
using System.Collections;
using System.Collections.Generic;
using Aoiti.Pathfinding;
using Hexes;
using Hexes.TileType;
using Singletons;
using UnityEngine;

public class MoveToSetPosition : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    private Pathfinder<HexTile> pathfinder;
    private List<HexTile> pathToFollow;
    private HexTile targetTile;

    private HexBoard hexBoard;
    private Player _player;

    private void Start()
    {
        hexBoard = FindObjectOfType<HexBoard>();
        _player = FindObjectOfType<Player>();

        // Initialize the Pathfinder with heuristic distance and neighbor fetching functions
        pathfinder = new Pathfinder<HexTile>(
            HeuristicDistance,
            GetConnectedNodesAndStepCosts
        );
    }

    public void Move(string pos)
    {
        string[] position = pos.Split(",");
        MoveTo(new Vector2Int(Convert.ToInt32(position[0]),Convert.ToInt32(position[1])));
    }

    // Public method to move to a target position (given by cube coordinates)
    public void MoveTo(Vector2Int targetCubeCoords)
    {
        targetTile = hexBoard.TileComponents[targetCubeCoords.x, targetCubeCoords.y];
        Debug.Log(targetTile.Position);
        Debug.Log(_player.currentTile.Position);

        if (pathfinder.GenerateAstarPath(_player.currentTile, targetTile, out pathToFollow))
        {
            // Start the coroutine to follow the path
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    private IEnumerator FollowPath()
    {
        foreach (HexTile tile in pathToFollow)
        {
            Vector3 targetPosition = hexBoard.TileGameObjects[tile.Position.x, tile.Position.y].transform.position;

            transform.position = targetPosition;
            yield return new WaitForSeconds(1f/moveSpeed);
            // while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            // {
            //     transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            //     yield return null;
            // }
            _player.currentTile = tile;
        }
    }

    // Heuristic distance: Euclidean distance between two hex tiles
    private float HeuristicDistance(HexTile start, HexTile target)
    {
        // Convert 2D coordinates to axial coordinates (q, r) for odd-right layout
        Vector2Int startCoords = start.Position;
        Vector2Int targetCoords = target.Position;

        // Convert start and target to axial coordinates
        int startQ = startCoords.x;
        int startR = startCoords.y - (startCoords.x - (startCoords.x & 1)) / 2;

        int targetQ = targetCoords.x;
        int targetR = targetCoords.y - (targetCoords.x - (targetCoords.x & 1)) / 2;

        // Calculate the distance using the axial distance formula
        return Mathf.Max(
            Mathf.Abs(startQ - targetQ),
            Mathf.Abs(startR - targetR),
            Mathf.Abs((startQ + startR) - (targetQ + targetR))
        );
    }

    // Get neighboring tiles and step costs
    private Dictionary<HexTile, float> GetConnectedNodesAndStepCosts(HexTile current)
    {
        Dictionary<HexTile, float> neighbors = new Dictionary<HexTile, float>();

        foreach (HexTile.NearbyTile nearbyTile in current.nearbyTiles)
        {
            if (nearbyTile.hexTile.HeightType == HeightType.Mountain) { continue; }
            neighbors.Add(nearbyTile.hexTile,1f);
        }
        
        return neighbors;
    }
}
