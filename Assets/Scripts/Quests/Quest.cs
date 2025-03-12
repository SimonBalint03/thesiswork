using System;
using System.Collections.Generic;
using System.Linq;
using Hexes;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
    public class Quest : ScriptableObject
    {
        public int id;
        public string title;
        public string description;
        public bool isClaimed;
        public bool isCompleted;
        public bool started;
        public List<Objective> objectives;
        public Reward reward;
        
        private QuestManager manager;
        
        public delegate void QuestStarted(Quest quest);
        public static event QuestStarted OnQuestStarted;
        public delegate void QuestClaimed(Quest quest);
        public static event QuestClaimed OnQuestClaimed;
        public delegate void QuestCanceled(Quest quest);
        public static event QuestCanceled OnQuestCanceled;
        public delegate void QuestCompleted(Quest quest);
        public static event QuestCompleted OnQuestCompleted;

        public Quest(string title, string description, List<Objective> objectives, Reward reward,bool isClaimed = false, bool isCompleted = false, bool started = false)
        {
            this.id = QuestManager.IterateQuestIdCounter();
            this.title = title;
            this.description = description;
            this.isClaimed = isClaimed;
            this.isCompleted = isCompleted;
            this.started = started;
            this.objectives = objectives;
            this.reward = reward;
        }

        protected bool Equals(Quest other)
        {
            return base.Equals(other) && title == other.title && description == other.description && isClaimed == other.isClaimed && isCompleted == other.isCompleted && started == other.started;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Quest)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), title, description, isClaimed, isCompleted, started);
        }

        private void OnEnable()
        {
            manager = FindObjectOfType<QuestManager>();
            //Debug.Log("Found quest manager " + manager.acceptedQuests.Count);

            ResetAttributes();
        }

        private void ResetAttributes()
        {
            foreach (Objective objective in objectives)
            {
                objective.currentProgress = 0;
                objective.isCompleted = false;
            }
            
            isClaimed = false;
            isCompleted = false;
            started = false;
        }

        public void StartQuest()
        {
            Debug.Log("Starting Quest" + title +" "+ QuestManager.acceptedQuestsCount );
            if (started) { Debug.LogWarning("Quest already started"); return; }

            if (QuestManager.acceptedQuestsCount > 1) { Debug.LogWarning("Quest limit reached"); return; }
            
            started = true;
            OnQuestStarted?.Invoke(this);
        }

        public void CompleteQuest()
        {
            if (!isCompleted)
            {
                OnQuestCompleted?.Invoke(this);
            }
            isCompleted = true;
        }

        public void ClaimQuest()
        {
            if (!isCompleted) { Debug.LogWarning("Quest is not completed"); return; }
            
            isClaimed = true;
            
            OnQuestClaimed?.Invoke(this);
        }

        public void CancelQuest()
        {
            UIManager.Instance.ShowConfirmation("Are you sure you want to cancel this quest? It will reset all progress, you can restart it in the city.",
                () =>
                {
                    OnQuestCanceled?.Invoke(this);
                    ResetAttributes();
                },
                () => { Debug.Log("Quest cancel, cancelled."); }
            );
            
            
        }

        public void CheckObjectives(HexTile tile)
        {

            if (!started) { return; }
            
            foreach (Objective objective in objectives)
            {
                objective.CheckCompletion(tile);
            }

            if (objectives.Any(objective => !objective.isCompleted))
            {
                return;
            }
            CompleteQuest();
            
        }
        
    }
    
}
