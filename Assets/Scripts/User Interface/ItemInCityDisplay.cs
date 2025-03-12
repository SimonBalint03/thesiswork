using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInCityDisplay : MonoBehaviour
{
    public Item item;
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI type;
    
    public Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void UpdateAvailability()
    {
        button.interactable = item.isBuyable;
    }
}
