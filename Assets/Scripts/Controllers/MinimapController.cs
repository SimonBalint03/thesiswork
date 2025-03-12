using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using Hexes.TileType;
using Singletons;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public Player player;
    public HexBoard map;
    public MinimapReferences references;
    [Header("Camera Settings")]
    public float dragSpeed = 20f; // Speed of camera dragging
    public float zoomSpeed = 5f; // Speed of camera zooming
    public float minY = 5f; // Minimum Y position (zoomed in)
    public float maxY = 50f; // Maximum Y position (zoomed out)
    public RectTransform minimapRectTransform; // The UI RectTransform of the minimap texture
    
    private Vector3 dragOrigin; // Where the drag started
    private bool isDragging = false;
    
    public bool isInteractable = false;
    private float targetY;

    private void Start()
    {
        StartCoroutine(MoveCameraAbovePlayer());
        
        foreach (HexTile hexTile in map.TileComponents)
        {
            switch (hexTile.BiomeType) 
            {
                case BiomeType.RainForest:
                    hexTile.minimap.color.color = new Color(0.13f, 0.35f, 0.13f); // Dark green (muted)
                    break;
                case BiomeType.TemperateRainForest:
                    hexTile.minimap.color.color = new Color(0.20f, 0.40f, 0.20f); // Lime green (muted)
                    break;
                case BiomeType.Swamp:
                    hexTile.minimap.color.color = new Color(0.30f, 0.40f, 0.30f); // Swamp green (muted)
                    break;
                case BiomeType.SeasonalForest:
                    hexTile.minimap.color.color = new Color(0.25f, 0.35f, 0.15f); // Olive green (muted)
                    break;
                case BiomeType.Forest:
                    hexTile.minimap.color.color = new Color(0.10f, 0.30f, 0.10f); // Forest green (muted)
                    break;
                case BiomeType.Savanna:
                    hexTile.minimap.color.color = new Color(0.60f, 0.50f, 0.20f); // Goldenrod (muted)
                    break;
                case BiomeType.Shrubland:
                    hexTile.minimap.color.color = new Color(0.40f, 0.30f, 0.15f); // Saddle brown (muted)
                    break;
                case BiomeType.Taiga:
                    hexTile.minimap.color.color = new Color(0.50f, 0.60f, 0.70f); // Light blue (muted, for snowy taiga)
                    break;
                case BiomeType.Desert:
                    hexTile.minimap.color.color = new Color(0.80f, 0.75f, 0.60f); // Wheat (muted, light sand color)
                    break;
                case BiomeType.Plains:
                    hexTile.minimap.color.color = new Color(0.50f, 0.70f, 0.50f); // Pale green (muted)
                    if (Random.Range(0f,1f) > 0.8f)
                    {
                        hexTile.minimap.icon.sprite = references.GetRandomGrassIcon();
                    }
                    break;
                case BiomeType.IceDesert:
                    hexTile.minimap.color.color = new Color(0.80f, 0.85f, 0.85f); // Ice blue (muted)
                    break;
                case BiomeType.Tundra:
                    hexTile.minimap.color.color = new Color(0.50f, 0.60f, 0.50f); // Tundra green (muted)
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (hexTile.HeightType)
            {
                case HeightType.ShallowWater or HeightType.MidWater or HeightType.DeepWater:
                    hexTile.minimap.color.color = new Color(0f / 255f, 128f / 255f, 255f / 255f);
                    hexTile.minimap.icon.sprite = references.waveIcon;
                    hexTile.minimap.outline.enabled = false;
                    break;
                case HeightType.Mountain:
                    hexTile.minimap.color.color = new Color(128f / 255f, 128f / 255f, 128f / 255f);
                    hexTile.minimap.icon.sprite = references.GetRandomMountainIcon();
                    hexTile.minimap.outline.enabled = false;
                    break;
                case HeightType.Flat:
                    break;
                case HeightType.Hill:
                    hexTile.minimap.color.color = DarkenColor(hexTile.minimap.color.color,0.3f);
                    hexTile.minimap.icon.sprite = references.GetRandomHillIcon();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // TEMP
            hexTile.minimap.color.enabled = false;

            if (hexTile.IsForest)
            {
                if (Random.Range(0f, 1f) > 0.25f)
                {
                    hexTile.minimap.icon.sprite = references.GetRandomForestIcon();
                }
            }

            if (hexTile.IsRiver)
            {
                /*
                hexTile.minimap.riverAnchor.SetActive(true);
                hexTile.minimap.riverCenter.SetActive(true);

                foreach (int dir in hexTile.DirectionsOfWater)
                {
                    hexTile.minimap.riverWings[dir].SetActive(true);
                }
                */

                hexTile.minimap.icon.sprite = references.riverIcon;
            }
            else
            {
                hexTile.minimap.riverAnchor.SetActive(false);
            }

            if (hexTile.IsCity)
            {
                switch (hexTile.City.CityType)
                {
                    case CityType.Big:
                        hexTile.minimap.icon.sprite = references.BigCityIcon;
                        break;
                    case CityType.Village:
                        hexTile.minimap.icon.sprite = references.VillageIcon;
                        break;
                    case CityType.Outpost:
                        hexTile.minimap.icon.sprite = references.OutpostIcon;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void Update()
    {
        if (isInteractable)
        {
            HandleDrag();
            HandleZoom();
        }
    }

    private void OnEnable()
    {
        Player.OnSingleMoveEvent += OnPlayerSingleMove;
        PlayerMenuController.OnPlayerMenuClose += OnPlayerMenuClose;
        PlayerMenuController.OnPlayerMenuOpen += OnPlayerMenuOpen;
    }

    private void OnDisable()
    {
        Player.OnSingleMoveEvent -= OnPlayerSingleMove;
        PlayerMenuController.OnPlayerMenuClose -= OnPlayerMenuClose;
        PlayerMenuController.OnPlayerMenuOpen -= OnPlayerMenuOpen;
    }

    private void OnPlayerSingleMove(HexTile from, HexTile to)
    {
        StartCoroutine(MoveCameraAbovePlayer());
    }

    private void OnPlayerMenuClose()
    {
        isInteractable = false;
        minimapCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y+15, player.transform.position.z);
    }
    private void OnPlayerMenuOpen()
    {
        isInteractable = true;
    }

    private IEnumerator MoveCameraAbovePlayer()
    {
        yield return new WaitForSeconds(0.8f);
        minimapCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y+15, player.transform.position.z);
    }
    
    
    public static Color DarkenColor(Color color, float amount)
    {
        amount = Mathf.Clamp01(amount);
        return new Color(
            color.r * (1 - amount),
            color.g * (1 - amount),
            color.b * (1 - amount),
            color.a
        );
    }

    public static Color LightenColor(Color color, float amount)
    {
        amount = Mathf.Clamp01(amount);
        return new Color(
            Mathf.Clamp01(color.r * (1 + amount)),
            Mathf.Clamp01(color.g * (1 + amount)),
            Mathf.Clamp01(color.b * (1 + amount)),
            color.a
        );
    }
    void HandleDrag()
    {
        // Check if the mouse is over the minimap texture
        if (RectTransformUtility.RectangleContainsScreenPoint(minimapRectTransform, Input.mousePosition))
        {
            // Check if the left mouse button is pressed
            if (Input.GetMouseButtonDown(0))
            {
                // Record the starting position of the drag in screen space
                dragOrigin = Input.mousePosition;
                isDragging = true;
            }
        }

        // If the left mouse button is held down, move the camera
        if (Input.GetMouseButton(0) && isDragging)
        {
            // Calculate the mouse delta in screen space
            Vector3 mouseDelta = Input.mousePosition - dragOrigin;

            // Convert the mouse delta to world space movement
            Vector3 worldDelta = mouseDelta;
            
            // Adjust the movement based on the camera's height (y position)
            float heightFactor = minimapCamera.transform.position.y / maxY;
            worldDelta *= dragSpeed * heightFactor;

            // Move the camera along the X and Z axes based on the mouse delta
            Vector3 newPosition = minimapCamera.transform.position;
            newPosition.x -= worldDelta.x; // Move left-right along the X axis
            newPosition.z -= worldDelta.y; // Move up-down along the Z axis

            // Apply the new position to the camera
            minimapCamera.transform.position = newPosition;

            // Update the drag origin for the next frame
            dragOrigin = Input.mousePosition;
        }

        // Stop dragging when the mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        // Get the scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Adjust the camera's Y position based on the scroll input
        if (scroll != 0)
        {
            float newY = minimapCamera.transform.position.y - scroll * zoomSpeed;
            newY = Mathf.Clamp(newY, minY, maxY); // Clamp the Y position

            // Update the camera's position
            minimapCamera.transform.position = new Vector3(
                minimapCamera.transform.position.x,
                newY,
                minimapCamera.transform.position.z
            );
        }
    }

    private Vector3 GetWorldPositionOnMinimap(Vector2 screenPosition)
    {
        // Convert the screen position to a point on the minimap texture
        RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRectTransform, screenPosition, null, out Vector2 localPoint);

        // Convert the local point to a normalized position (0 to 1) on the texture
        Vector2 normalizedPoint = Rect.PointToNormalized(minimapRectTransform.rect, localPoint);

        // Convert the normalized point to world space
        Vector3 worldPosition = minimapCamera.ViewportToWorldPoint(new Vector3(normalizedPoint.x, normalizedPoint.y, minimapCamera.nearClipPlane));
        worldPosition.z = 0; // Ensure the Z position is 0 (2D camera)

        return worldPosition;
    }
}
