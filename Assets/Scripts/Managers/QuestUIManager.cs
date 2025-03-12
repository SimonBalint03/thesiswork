using System;
using System.Collections;
using System.Collections.Generic;
using Hexes;
using Managers;
using MyBox;
using Quests;
using Singletons;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    public QuestManager questManager;
    public GameObject questUI, scrollList;
    
    [SerializeField]private List<GameObject> activeUIs = new List<GameObject>();

    private void OnEnable()
    {
        Quest.OnQuestStarted += OnQuestStarted;
        Quest.OnQuestClaimed += OnQuestClaimed;
        Quest.OnQuestCanceled += OnQuestCanceled;
    }

    private void OnDisable()
    {
        Quest.OnQuestStarted -= OnQuestStarted;
        Quest.OnQuestClaimed -= OnQuestClaimed;
        Quest.OnQuestCanceled -= OnQuestCanceled;
    }

    private void OnQuestStarted(Quest quest)
    {
        Debug.Log("Quest UI element created");
        questUI.GetComponent<QuestUIDisplay>().quest = quest;
        questUI.GetComponent<QuestUIDisplay>().parentCity = Player.Instance.currentTile.City;
        activeUIs.Add(Instantiate(questUI, scrollList.transform));
    }
    
    private void OnQuestClaimed(Quest quest)
    {
        for (int i = 0; i < activeUIs.Count; i++)
        {
            if (activeUIs[i].GetComponent<QuestUIDisplay>().quest != quest) continue;
            Destroy(activeUIs[i]);
            activeUIs.RemoveAt(i);
            return;
        }
    }
    
    private void OnQuestCanceled(Quest quest)
    {
        for (int i = 0; i < activeUIs.Count; i++)
        {
            if (activeUIs[i].GetComponent<QuestUIDisplay>().quest != quest) continue;
            Destroy(activeUIs[i]);
            activeUIs.RemoveAt(i);
            return;
        }
    }
}
