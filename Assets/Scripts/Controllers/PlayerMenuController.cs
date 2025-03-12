using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Effects;
using Inventory;
using Managers;
using MyBox;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuController : MonoBehaviour
{
    public GameObject menu;
    [Separator("Player")]
    public GameObject playerBox;
    
    [Separator("Inventory")]
    public GameObject inventoryBox;
    public GameObject inventoryList;
    public GameObject inventoryItemPrefab;
    public Item selectedItem;
    public GameObject detailsContainer,detailsPlaceholder;
    public TextMeshProUGUI itemDetailsName,itemDetailsDescription,itemDetailsPrice,itemDetailsType;
    public Image itemDetailsIcon;
    public TextMeshProUGUI invNavButton;
    public TextMeshProUGUI capacityText;
    
    
    [Separator("Map")] 
    public GameObject mapBox;
    public GameObject previousMapList;
    [SerializeField] private GameObject previousMap;
    public TextMeshProUGUI currentMapTotalValue;
    public TextMeshProUGUI currentMapAmount;
    public Map currentMap;
    public TextMeshProUGUI mapNavButton;
    
    private List<Map> prevMaps = new List<Map>();
    
    private CameraController cameraController;
    private MovementController movementController;
    private EffectsController effectsController;
    private GameManager gameManager;
    
    public delegate void PlayerMenuOpenAction();
    public static event PlayerMenuOpenAction OnPlayerMenuOpen;
    public delegate void PlayerMenuCloseAction();
    public static event PlayerMenuCloseAction OnPlayerMenuClose;

    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        movementController = FindObjectOfType<MovementController>();
        effectsController = FindObjectOfType<EffectsController>();
        gameManager = FindObjectOfType<GameManager>();

        currentMap = Player.Instance.map;
        
        Map.OnSellMapEvent += OnSellMap;
        Inventory.Inventory.OnItemRemoved += OnItemRemoved;
    }

    private void OnDestroy()
    {
        Map.OnSellMapEvent -= OnSellMap;
        Inventory.Inventory.OnItemRemoved -= OnItemRemoved;
    }


    private void Update()
    {
        if (gameManager.isGameOver) { return; }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMenu();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (menu.activeSelf && mapBox.activeSelf)
            {
                ExitMenu();
                
            }
            else
            {
                OpenMenu(mapBox);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (menu.activeSelf && inventoryBox.activeSelf)
            {
                ExitMenu();
            }
            else
            {
                OpenMenu(inventoryBox);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (menu.activeSelf && playerBox.activeSelf)
            {
                ExitMenu();
            }
            else
            {
                OpenMenu(playerBox);
            }
        }

		if (menu.activeSelf)
        {
            UpdateCurrentMapDisplay();
            
        }
    }

    private void OnSellMap(Map map)
    {
        Debug.Log("OnSellMap");
        GameObject newMap = Instantiate(previousMap, previousMapList.transform.position, Quaternion.identity,previousMapList.transform);
        newMap.GetComponent<PrevMap>().Amount.text = "Amount: " + $"{map.mapTiles.Count:000}";
        newMap.GetComponent<PrevMap>().Price.text = "Price: " + $"{map.GetTotalPrice():0000}";
        newMap.GetComponent<PrevMap>().Date.text = "Map sold " + "TO BE IMPLEMENTED"+ " turns ago";
    }

    private void OnItemRemoved(Item item)
    {
        UpdateInventory();
    }

    private void UpdateCurrentMapDisplay()
    {
        currentMapTotalValue.text = "Total value: "+$"{currentMap.GetTotalPrice():0000}";
        currentMapAmount.text = "New areas: "+$"{currentMap.mapTiles.Count:0000}";
    }
    
    private void ToggleControllersOnPause(bool active)
    {
        movementController.enabled = active;
        cameraController.enabled = active;
        effectsController.enabled = active;
    }

    public void ExitMenu()
    {
        menu.SetActive(false);
        Time.timeScale = 1;
        ToggleControllersOnPause(true);
        OnPlayerMenuClose?.Invoke();
    }

    public void OpenMenu(GameObject menuToOpen)
    {
        mapBox.SetActive(false);
        inventoryBox.SetActive(false);
        playerBox.SetActive(false);
        
        menu.SetActive(true);
        menuToOpen.SetActive(true);
        //Time.timeScale = 0;
        ToggleControllersOnPause(false);

        if (inventoryBox.activeSelf)
        {
            UpdateInventory();
            //invNavButton.color = new Color(101f, 69f, 29f,255f); // Dark
            //mapNavButton.color = new Color(153f,120f,77f,255f); // Light
        }

        if (mapBox.activeSelf)
        {
            //mapNavButton.color = new Color(101f, 69f, 29f,255f);
            //invNavButton.color = new Color(153f, 120f, 77f, 255f);
        }
        
        OnPlayerMenuOpen?.Invoke();
    }

    private void UpdateInventory()
    {
        capacityText.text = $"Capacity: {Player.Instance.inventory.Items.Count} / {Player.Instance.inventory.MaxCapacity}";
        for (int i = 0; i < inventoryList.transform.childCount; i++)
        {
            Destroy(inventoryList.transform.GetChild(i).gameObject);
        }
        
        foreach (Item inventoryItem in Player.Instance.inventory.Items)
        {
            GameObject prefab = Instantiate(inventoryItemPrefab, inventoryList.transform);
            ItemInMenu itemInMenu = prefab.GetComponent<ItemInMenu>();
            prefab.GetComponent<Button>().onClick.AddListener(() => SetSelectedItem(inventoryItem));
            itemInMenu.deleteButton.onClick.AddListener(() => Player.Instance.inventory.RemoveItem(inventoryItem));
            if (inventoryItem.Type == Item.ItemType.Usable)
            {
                itemInMenu.useButton.gameObject.SetActive(true);
                itemInMenu.useButton.onClick.AddListener(() => Player.Instance.inventory.UseItem(inventoryItem));
            }
            else
            {
                itemInMenu.useButton.gameObject.SetActive(false);
            }
            
            itemInMenu.item = inventoryItem;
            itemInMenu.itemName.text = inventoryItem.Name;
            itemInMenu.itemIcon.sprite = inventoryItem.Sprite;
        }
        selectedItem = null;

        if (selectedItem != null)
        {
            detailsContainer.SetActive(true);
            detailsPlaceholder.SetActive(false);
        }
        else
        {
            detailsContainer.SetActive(false);
            detailsPlaceholder.SetActive(true);
        }
        
    }

    public void SetSelectedItem(Item item)
    {
        selectedItem = item;
        
        detailsContainer.SetActive(true);
        detailsPlaceholder.SetActive(false);
        
        itemDetailsName.text = item.Name;
        itemDetailsDescription.text = item.Description;
        itemDetailsPrice.text = item.Value + "";
        itemDetailsType.text = item.Type.ToString();
        itemDetailsIcon.sprite = item.Sprite;
    }
    
}
