using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using MyBox;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class InteractButton : MonoBehaviour, IPointerClickHandler
{
    public delegate void InteractButtonClickedAction(Type type, City city, bool firstTime);
    public static event InteractButtonClickedAction OnInteractButtonClicked;
    
    public enum Type
    {
        City
    }

    public Type type;
    [ConditionalField("type",false,Type.City)] public City city;
    public HexTile hexTile;
    public TextMeshProUGUI text;
    public bool interactable = false;
    [SerializeField]private bool firstTime = true;

    private void OnEnable()
    {
        GetComponent<Button>().interactable = interactable;
    }
    
    public void InvokeAction()
    {
        OnInteractButtonClicked?.Invoke(type,city,firstTime);
        firstTime = false;
        AudioManager.Instance.PlayCityOpenSound();
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        GetComponent<Button>().interactable = value;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
        {
            Player.Instance.TryMove(hexTile);
        }
    }
}
