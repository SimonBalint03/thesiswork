using System;
using System.Collections.Generic;
using Hexes;
using MyBox;
using Quests;
using Singletons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Managers
{
    public class QuestManager : MonoBehaviour
    {
        public Player player;

        [Separator("Quests")]
        public bool duplicateQuests;
        [ConditionalField("duplicateQuests")] public int duplicationAmount;
        public List<Quest> availableQuests;
        public List<Quest> acceptedQuests;
        
        public static int acceptedQuestsCount = 0;
    
    
        private static int questCounter = 0;

        public static int IterateQuestIdCounter()
        {
            return questCounter++;
        }

        private void OnEnable()
        {
            HexTile.OnTileDiscoveredFirstTime += CheckCompletion;
            Quest.OnQuestStarted += OnQuestStarted;
            Quest.OnQuestClaimed += OnQuestClaimed;
            Quest.OnQuestCanceled += OnQuestCanceled;

            if (duplicateQuests)
            {
                List<Quest> newQuests = new List<Quest>();
                
                for (int i = 0; i < duplicationAmount; i++)
                {
                    foreach (Quest quest in availableQuests)
                    {
                        var newQuest = Instantiate(quest);
                        newQuests.Add(newQuest);
                    }
                }
                
                availableQuests.AddRange(newQuests);
                
            }
        }

        private void OnDisable()
        {
            HexTile.OnTileDiscoveredFirstTime -= CheckCompletion;
            Quest.OnQuestStarted -= OnQuestStarted;
            Quest.OnQuestClaimed -= OnQuestClaimed;
            Quest.OnQuestCanceled -= OnQuestCanceled;
            acceptedQuestsCount = 0; // bugfix
        }
        
        public Quest GetRandomQuestWithNewId()
        {
            if (availableQuests == null || availableQuests.Count == 0)
            {
                Debug.LogWarning("No quests in the database.");
                return null;
            }

            int randomIndex = Random.Range(0, availableQuests.Count);
            Quest result = availableQuests[randomIndex];
            result.id = IterateQuestIdCounter();
            return result;
        }

        private void OnQuestStarted(Quest quest)
        {
            if (acceptedQuests.Contains(quest)) { Debug.LogWarning("Quest already in collection."); return; }

            if (acceptedQuests.Count > 1) { Debug.LogWarning("No more slots for quest."); return; }
            
            acceptedQuests.Add(quest);
            acceptedQuestsCount++;
        }
        
        private void OnQuestClaimed(Quest quest)
        {
            // Award rewards.
            Player.Instance.money += quest.reward.GetReward();
            
            // Disable in city dialogue.
            // Remove quest tracker.
            
            // Remove from acceptedQuests
            acceptedQuests.Remove(quest);
            acceptedQuestsCount--;
        }
        
        private void OnQuestCanceled(Quest quest)
        {
            // Remove from acceptedQuests
            acceptedQuests.Remove(quest);
            acceptedQuestsCount--;
        }

        private void CheckCompletion(HexTile tile)
        {
            foreach (Quest quest in acceptedQuests)
            {
                quest.CheckObjectives(tile);
            }
        }
    }
}
