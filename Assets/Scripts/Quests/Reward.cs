using MyBox;

namespace Quests
{
    [System.Serializable]
    public class Reward
    {
        public enum RewardType
        {
            Item,
            Money
        }
        
        public RewardType Type;

        [ConditionalField(nameof(Type), false, RewardType.Money)]
        public int moneyAmount = 0;

        public int GetReward()
        {
            return moneyAmount;
        }
        //TODO: New overwrite with item type..
    }
}
