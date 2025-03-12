using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using Managers;
using MyBox;
using Quests;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIDisplay : MonoBehaviour
{
    public Quest quest;
    public TextMeshProUGUI title;
    public GameObject objective;
    public City parentCity;

    public GameObject scrollList, scrollBox;
    
    public Button cancelButton;
    
    private Dictionary<Objective, GameObject> objectives = new Dictionary<Objective, GameObject>();

    private void OnEnable()
    {
        HexTile.OnTileDiscoveredFirstTime += UpdateQuestText;

        foreach (Objective questObjective in quest.objectives)
        {
            objective.GetComponent<TextMeshProUGUI>().text = questObjective.title;
            objectives.Add(questObjective,Instantiate(objective,scrollList.transform));
        }
        
        UpdateQuestText();
        
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(quest.CancelQuest);
    }

    private void OnDisable()
    {
        HexTile.OnTileDiscoveredFirstTime -= UpdateQuestText;
    }

    private void UpdateQuestText(HexTile tile)
    {
        UpdateQuestText();
    }

    private void UpdateQuestText()
    {
        title.text = $"{parentCity.name.Trim()}: {quest.title}";

        foreach (KeyValuePair<Objective,GameObject> pair in objectives)
        {
            pair.Value.GetComponent<TextMeshProUGUI>().text = pair.Key.title + " (" + pair.Key.currentProgress + "/" + pair.Key.requiredProgress + ")";
        }

        if (scrollList.transform.childCount <= 1) { return; }
        
        for (int i = 0; i < scrollList.transform.childCount; i++)
        {
            //Debug.Log("UpdateQuestText" + scrollBox.GetComponent<RectTransform>().sizeDelta);
            scrollBox.GetComponent<RectTransform>().SetHeight(150 + 29 * i);
            GetComponent<RectTransform>().SetHeight(150 + 29 * i);
        }
    }
}
