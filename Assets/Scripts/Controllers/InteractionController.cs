using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hexes;
using Singletons;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractionController : MonoBehaviour
{
    public Canvas worldSpaceCanvas;
    public GameObject interactButton;
    public HexBoard board;
    
    private Dictionary<HexTile,GameObject> _activeButtons = new Dictionary<HexTile,GameObject>();

    private void OnEnable()
    {
        HexTile.OnTileVisibilityChanged += UpdateButtonVisibility;
    }

    private void OnDisable()
    {
        HexTile.OnTileVisibilityChanged -= UpdateButtonVisibility;
    }
    private void Awake()
    {
        foreach (HexTile hexTile in board.TileComponents)
        {
            if (!hexTile.IsCity) { continue; }
            
            GameObject button = Instantiate(interactButton, Vector3.zero, Quaternion.identity,worldSpaceCanvas.transform);
            InteractButton buttonScript = button.GetComponent<InteractButton>();
            buttonScript.type = InteractButton.Type.City;
            buttonScript.city = hexTile.City;
            buttonScript.hexTile = hexTile;
            buttonScript.text.text = hexTile.City.name;
            _activeButtons.Add(hexTile, button);
            
            HideButton(button);
        }
        
        UpdateButtonVisibility(null, HexTile.TileVisibility.Hidden, false);
    }

    private void UpdateButtonVisibility(HexTile tile, HexTile.TileVisibility visibility, bool shouldDiscover)
    {
        foreach (KeyValuePair<HexTile,GameObject> activeButton in _activeButtons)
        {
            if (activeButton.Key.Visibility != HexTile.TileVisibility.Hidden)
            {
                //Debug.Log(activeButton.Key.Position +" : "+ Player.Instance.currentTile.Position);
                activeButton.Value.GetComponent<InteractButton>().SetInteractable(activeButton.Key.Position == Player.Instance.currentTile.Position);
                ShowButton(activeButton.Value, activeButton.Key.transform);
            }
            else
            {
                activeButton.Value.GetComponent<InteractButton>().interactable = false;
                HideButton(activeButton.Value);
            }
        }

        //Debug.Log("Moving from: "+ from.Position + " to " + to.Position);
        // City
        /*if (to.IsCity)
        {
            if (!_activeButtons.ContainsKey(to))
            {
                GameObject button = Instantiate(interactButton, Vector3.zero, Quaternion.identity,worldSpaceCanvas.transform);
                button.gameObject.GetComponent<InteractButton>().type = InteractButton.Type.City;
                button.gameObject.GetComponent<InteractButton>().city = to.City;
                ShowButton(button,to.transform);
            
                List<GameObject> tileButtons = new List<GameObject>();
                tileButtons.Add(button);
                _activeButtons.Add(to, tileButtons);
            }
            else
            {
                foreach (GameObject button in _activeButtons.FirstOrDefault(pair => pair.Key == to).Value)
                {
                    ShowButton(button,to.transform);
                }
            }
            return;
        }
        if (from.IsCity)
        {
            foreach (var (hexTile, list) in _activeButtons)
            {
                if (hexTile.Position == from.Position)
                {
                    list.ForEach(HideButton);
                }
            }
        }*/
    }

    private void ShowButton(GameObject button,Transform position)
    {
        //return;
        button.transform.position = new Vector3(position.position.x, position.position.y + 2.5f, position.position.z );
        button.transform.localRotation = Quaternion.identity;
        button.SetActive(true);
    }

    private void HideButton(GameObject button)
    {
        button.SetActive(false);
    }
    
}
