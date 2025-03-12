using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDisplay : MonoBehaviour
{
    public TextMeshProUGUI title,description;
    public Image completedIcon;
    [Separator("Reward")]
    public Image rewardIcon;
    public TextMeshProUGUI rewardText;
}
