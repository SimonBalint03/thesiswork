using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Hexes;
using Managers;
using MyBox;
using Quests;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUiDisplay : MonoBehaviour
{
    [Separator("Time")]
    public Image timeImage;
    public Sprite sunrise;
    public Sprite day,sunset,moonrise,night,moonset;
    
    [Separator("Player info")]
    public Slider playerMovement;
    public Image predictedMovement;
    public TextMeshProUGUI money;
    public TextMeshProUGUI totalTurns;
    [Separator("Player state")]
    public IconAnimator stateIcon;
    public List<Sprite> idleIcons, expIcons, retIcons;

    private float lastRemainingPoints;
    
    private void OnEnable()
    {
        TimeOfDay.OnNextStep += OnTimeNextStep;
        Player.OnSingleMoveEvent += OnPlayerSingleMove;
        Map.OnSellMapEvent += OnSellMapEvent;
        MovementController.OnPredictedPathChanged += OnPredictedPathChanged;
        Quest.OnQuestClaimed += OnQuestClaimed;
        Inventory.Inventory.OnBuyItem += OnBuyItem;
        
        UpdateTimeImage();
        UpdatePlayerInfo();
    }

    private void OnDisable()
    {
        TimeOfDay.OnNextStep -= OnTimeNextStep;
        Player.OnSingleMoveEvent -= OnPlayerSingleMove;
        Map.OnSellMapEvent -= OnSellMapEvent;
        MovementController.OnPredictedPathChanged -= OnPredictedPathChanged;
        Quest.OnQuestClaimed -= OnQuestClaimed;
        Inventory.Inventory.OnBuyItem -= OnBuyItem;
    }

    private void OnTimeNextStep()
    {
        UpdateTimeImage();
        UpdatePlayerInfo();
        UpdatePredictedMovement(null,lastRemainingPoints);
    }
    
    private void OnPlayerSingleMove(HexTile from, HexTile to)
    {
        UpdatePlayerInfo();
        UpdatePredictedMovement(null,lastRemainingPoints);
    }
    private void OnSellMapEvent(Map map)
    {
        UpdatePlayerInfo();
    }
    
    private void OnPredictedPathChanged(List<HexTile> path, float remainingPoints)
    {
        lastRemainingPoints = remainingPoints;
        UpdatePredictedMovement(path,remainingPoints);
    }
    
    private void OnQuestClaimed(Quest quest)
    {
        UpdatePlayerInfo();
    }
    
    private void OnBuyItem()
    {
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        playerMovement.maxValue = (float)Player.Instance.GetTotalMovementPoints();
        playerMovement.value = (float)Player.Instance.currentMovementPoints;
        
        money.text = Player.Instance.money.ToString();
        totalTurns.text = TimeOfDay.Instance.CurrentStep.Id - 1 + "";

        switch (Player.Instance.movementState)
        {
            case Player.MovementState.Idle:
                stateIcon.sprites = idleIcons;
                stateIcon.ResetFlip();
                predictedMovement.color = Color.white;
                break;
            case Player.MovementState.Exploring:
                stateIcon.sprites = expIcons;
                stateIcon.ResetFlip();
                predictedMovement.color = Color.white;
                break;
            case Player.MovementState.Returning:
                stateIcon.sprites = retIcons;
                stateIcon.FlipImage(true,false);
                predictedMovement.color = Color.red;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateTimeImage()
    {
        timeImage.sprite = TimeOfDay.Instance.CurrentStep.TimeType switch
        {
            TimeOfDay.TimeType.Sunrise => sunrise,
            TimeOfDay.TimeType.Day => day,
            TimeOfDay.TimeType.Sunset => sunset,
            TimeOfDay.TimeType.Moonrise => moonrise,
            TimeOfDay.TimeType.Night => night,
            TimeOfDay.TimeType.Moonset => moonset,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void UpdatePredictedMovement(List<HexTile> path, float remainingPoints)
    {
        float totalMovementPoints = (float)Player.Instance.GetTotalMovementPoints();
        //float currentMovementPoints = (float)Player.Instance.currentMovementPoints;

        //Debug.Log("UpdatePredictedMovement" + remainingPoints / totalMovementPoints);
        
        predictedMovement.fillAmount = remainingPoints / totalMovementPoints;
    }
}
