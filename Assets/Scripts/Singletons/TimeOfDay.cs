using System;
using Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Singletons
{
    /// <summary>
    /// Represents the current time of day.
    /// Also manages the end of turns.
    /// </summary>
    public class TimeOfDay : MonoBehaviour
    {
        
        public delegate void ClickAction();
        public static event ClickAction OnNextStep;
        
        public enum TimeType { Sunrise = 0,Day = 1,Sunset = 2,Moonrise = 3,Night = 4,Moonset = 5 }
        public class Step
        {
            public int Id { get; set; }
            public int Time { get; set; }
            public TimeType TimeType { get; set; }

            public Step(int id, int time, TimeType timeType)
            {
                Id = id;
                Time = time;
                TimeType = timeType;
            }
        }

        public TimeOfDay(Step currentStep) { CurrentStep = currentStep; }

        public static TimeOfDay Instance { get; set; }
        public const int stepsInDay = 20; // Ehhez ne nagyon nyulj hozza.
        public Step CurrentStep { get; set; }
        public SunAndMoonController sunAndMoonController;
        public MovementController movementController;
            
        private void Awake() 
        { 
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this) { Destroy(this); } else { Instance = this; }
            
            CurrentStep = new Step(0, 0,TimeType.Day);
            
        }
        
        /// Gets called at the end of every turn by the Player
        public void OnNextStepButtonClicked()
        {
            #region Blocking conditions
            
                if (sunAndMoonController.isRotating) { Debug.LogWarning("Lights are rotating."); return; }
                if (movementController.player.isMoving) { Debug.LogWarning("Player is moving."); return; }
                
            #endregion
            
            
            Instance.CurrentStep.Id += 1;
            if (Instance.CurrentStep.Id % stepsInDay == 0)
            {
                Instance.CurrentStep.Time = 0;
            }
            else
            {
                Instance.CurrentStep.Time++;
            }

            switch ((float)Instance.CurrentStep.Time)
            {
                case < stepsInDay*0.1f:
                    Instance.CurrentStep.TimeType = TimeType.Sunrise;
                    break;
                case < stepsInDay*0.4f:
                    Instance.CurrentStep.TimeType = TimeType.Day;
                    break;
                case < stepsInDay*0.5f:
                    Instance.CurrentStep.TimeType = TimeType.Sunset;
                    break;
                case < stepsInDay*0.6f:
                    Instance.CurrentStep.TimeType = TimeType.Moonrise;
                    break;
                case < stepsInDay*0.9f:
                    Instance.CurrentStep.TimeType = TimeType.Night;
                    break;
                case < stepsInDay*1f:
                    Instance.CurrentStep.TimeType = TimeType.Moonset;
                    break;
                default:
                    Instance.CurrentStep.TimeType = TimeType.Sunrise;
                    break;
            }
            
            if (OnNextStep != null) OnNextStep();
        }
    }

}
