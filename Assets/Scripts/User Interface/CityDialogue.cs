using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Database;
using Effects;
using Hexes;
using Inventory;
using Managers;
using MyBox;
using Quests;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CityDialogue : MonoBehaviour
{
    public GameObject dialogueBox;
    [Separator("Current Map")]
    public TextMeshProUGUI currentMapTotalValue;
    public TextMeshProUGUI currentMapAmount;
    [Separator("City & Quests")] 
    public TextMeshProUGUI cityName;
    public TextMeshProUGUI citySize;
    public QuestDisplay quest1,quest2;
    [Separator("Shop")]
    public TextMeshProUGUI money;
    public ItemInCityDisplay item1;
    public ItemInCityDisplay item2;
    
    public City parentCity;
    
    private Map currentMap;

    private CameraController cameraController;
    private MovementController movementController;
    private EffectsController effectsController;
    private QuestManager questManager;
    private SpriteService spriteService;
    private GameManager gameManager;
    
    private ItemDatabase itemDatabase;

    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        movementController = FindObjectOfType<MovementController>();
        effectsController = FindObjectOfType<EffectsController>();
        questManager = FindObjectOfType<QuestManager>();
        spriteService = FindObjectOfType<SpriteService>();
        gameManager = FindObjectOfType<GameManager>();
        
        itemDatabase = gameManager.itemDatabase;
        currentMap = Player.Instance.map;
    }

    private void OnEnable()
    {
        InteractButton.OnInteractButtonClicked += VerifyAndOpen;
        Map.AfterSellEvent += AfterMapSell;
        Quest.OnQuestClaimed += OnQuestClaimed;
        Quest.OnQuestStarted += OnQuestStarted;
        Inventory.Inventory.OnBuyItem += OnBuyItem;
    }

    private void OnDisable()
    {
        InteractButton.OnInteractButtonClicked -= VerifyAndOpen;
        Map.AfterSellEvent -= AfterMapSell;
        Quest.OnQuestClaimed -= OnQuestClaimed;
        Quest.OnQuestStarted -= OnQuestStarted;
        Inventory.Inventory.OnBuyItem -= OnBuyItem;
    }

    private void VerifyAndOpen(InteractButton.Type type, City city,bool firstTime)
    {
        //Debug.Log(type);
        if (type != InteractButton.Type.City)
        {
            return;
        }
        
        parentCity = city; // ??? if parentCity == null, pc = city

        dialogueBox.SetActive(true);
        Time.timeScale = 0;
        ToggleControllersOnPause(false);
        UpdateText();
        if (firstTime)
        {
            AssignQuests();
            AssignShop();
        }
        
        UpdateQuests();
        UpdateShop();
    }

    private void AssignQuests()
    {
        parentCity.quests.Add(questManager.GetRandomQuestWithNewId());
        questManager.availableQuests.Remove(parentCity.quests[0]);
        
        parentCity.quests.Add(questManager.GetRandomQuestWithNewId());
        questManager.availableQuests.Remove(parentCity.quests[1]);

    }

    private void AssignShop()
    {
        item1.item = itemDatabase.GetRandomItemWithNewId();
        parentCity.shopItems.Add(item1.item);
        
        item2.item = itemDatabase.GetRandomItemWithNewId();
        parentCity.shopItems.Add(item2.item);
    }

    private void AfterMapSell(Map map)
    {
        UpdateText();
    }
    
    private void OnQuestClaimed(Quest quest)
    {
        UpdateQuests();
        UpdateText();
    }
    
    private void OnQuestStarted(Quest quest)
    {
        UpdateQuests();
    }
    
    private void OnBuyItem()
    {
        UpdateShop();
        UpdateText();
    }

    private void Update()
    {
        // Only update when dialogue is open.
        if (!dialogueBox.activeSelf) { return; }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitDialogue();

        }
    }

    private void ToggleControllersOnPause(bool active)
    {
        movementController.enabled = active;
        cameraController.enabled = active;
        effectsController.enabled = active;
    }

    public void ExitDialogue()
    {
        dialogueBox.SetActive(false);
        Time.timeScale = 1;
        ToggleControllersOnPause(true);
    }

    private void UpdateText()
    {
        currentMapAmount.text = "New areas: " + $"{currentMap.mapTiles.Count:000}";
        currentMapTotalValue.text = "Price: " + $"{currentMap.GetTotalPrice():000.0}";

        cityName.text = parentCity.name;
        citySize.text = parentCity.CityType.ToString();
        
        money.text = Player.Instance.money.ToString();
        
    }
    private void UpdateQuests()
    {
        // Quest 1
        UpdateQuest(quest1, parentCity.quests[0]);
        // Quest 2
        UpdateQuest(quest2, parentCity.quests[1]);
    }
    private void UpdateQuest(QuestDisplay questDisplay, Quest quest)
    {
        questDisplay.title.text = quest.title;
        questDisplay.description.text = quest.description;
        questDisplay.rewardIcon.sprite = spriteService.moneySprite1;
        questDisplay.rewardText.text = quest.reward.moneyAmount+"";
        Button questButton = questDisplay.GetComponent<Button>();
        
        questButton.onClick.RemoveAllListeners();

        if (quest.isClaimed)
        {
            questButton.interactable = false;
            questDisplay.completedIcon.color = questButton.colors.disabledColor;
            questDisplay.rewardIcon.color = questButton.colors.disabledColor;
        }
        else if (quest.isCompleted)
        {
            questButton.onClick.AddListener(quest.ClaimQuest);
            questButton.interactable = true;
            questDisplay.completedIcon.color = Color.white;
            questDisplay.rewardIcon.color = Color.white;
        }
        else if (quest.started)
        {
            questButton.interactable = false;
            questDisplay.completedIcon.color = questButton.colors.highlightedColor;
            questDisplay.rewardIcon.color = questButton.colors.highlightedColor;
        }
        else
        {
            questButton.onClick.AddListener(quest.StartQuest);
            questButton.interactable = true;
            questDisplay.completedIcon.color = Color.white;
            questDisplay.rewardIcon.color = Color.white;
        }

        questDisplay.completedIcon.enabled = quest.isCompleted;
    }

    private void UpdateShop()
    {
        UpdateShopItem(item1,parentCity.shopItems[0]);
        UpdateShopItem(item2,parentCity.shopItems[1]);
    }

    private void UpdateShopItem(ItemInCityDisplay display, Item item)
    {
        //Debug.Log("UpdateShopItem" + item.Name + item.isBuyable);
        display.icon.sprite = item.Sprite;
        display.itemName.text = item.Name;
        display.description.text = item.Description;
        display.cost.text = item.Value+"";
        display.type.text = item.Type.ToString();
        display.item = item;

        display.UpdateAvailability();
        display.button.onClick.RemoveAllListeners();
        
        if (item.isBuyable)
        {
            display.button.onClick.AddListener(() => Player.Instance.inventory.BuyItem(item));
        }
    }
    

}
