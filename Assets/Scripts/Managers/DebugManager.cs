using Hexes;
using Singletons;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class DebugManager : MonoBehaviour
    {
        public GameObject debugPanel;
    
        [Header("Main")]
        public TextMeshProUGUI fpsText;
        public TextMeshProUGUI trianglesText;
    
        [Header("Map")]
        public TextMeshProUGUI seedText;
        public TextMeshProUGUI sizeText;
        public HexBoard board;
    
        [Header("TimeOfDay")]
        private TimeOfDay _timeOfDay;
        public TextMeshProUGUI timeOfDayTime;
        public TextMeshProUGUI timeOfDayCurrentStep;
    
        [Header("Player")]
        public Player player;
        public TextMeshProUGUI playerPosition;
        public TextMeshProUGUI playerCurrentMovement;
        public TextMeshProUGUI playerMovementState;
        [Header("Player's Map")] 
        public TextMeshProUGUI numOfTilesInMap;
        public TextMeshProUGUI valueOfMap;
        private Map currentMap;
    
        private InputSystem input;
        private InputAction openWindow;

        private void Awake()
        {
            input = new InputSystem();
            openWindow = input.Debug.OpenMenu;
            _timeOfDay = GetComponent<TimeOfDay>();
            currentMap = player.map;
        }

        private void Start()
        {
            seedText.text = "Seed: " + board.mapSeed.ToString();
            sizeText.text = "Size: " + board.mapSize;
        }

        private void OnEnable()
        {
            openWindow.performed += DebugMenuOpen;
            TimeOfDay.OnNextStep += SetTimeOfDayText;
            input.Enable();
        }

        private void OnDisable()
        {
            openWindow.performed -= DebugMenuOpen;
            TimeOfDay.OnNextStep -= SetTimeOfDayText;
            input.Disable();
        }

        private void DebugMenuOpen(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
                Debug.Log("DebugMenuOpen");
            }
        }

        private void Update()
        {
            if (debugPanel.activeSelf)
            {
                if (Time.frameCount % 10 == 0)
                {
                    fpsText.text = "Fps: " + $"{1.0f / Time.unscaledDeltaTime:0}";
                    //trianglesText.text = "Triangles: " + $"{UnityStats.triangles:00 000}";

                    playerPosition.text = "Position: " + player.currentTile.Position.x +" : " + player.currentTile.Position.y;
                    playerCurrentMovement.text = "CurrentMovement: " + $"{player.currentMovementPoints:00.00}";
                    playerMovementState.text = player.movementState.ToString();

                    numOfTilesInMap.text = "NumOfTiles: " + currentMap.mapTiles.Count;
                    valueOfMap.text = "Value: " + currentMap.GetTotalPrice();

                }

                if (Time.frameCount % 120 == 0)
                {
                    currentMap = player.map;
                }
                timeOfDayCurrentStep.text = "CurrentStep { id: " + TimeOfDay.Instance.CurrentStep.Id + ", time: "+ TimeOfDay.Instance.CurrentStep.Time +", type: "+TimeOfDay.Instance.CurrentStep.TimeType+" }";

            }
        }

        private void SetTimeOfDayText()
        {
            timeOfDayCurrentStep.text = "CurrentStep { id:" + _timeOfDay.CurrentStep.Id + ", time: " + _timeOfDay.CurrentStep.Time + ", type: " + _timeOfDay.CurrentStep.TimeType + " }";
        }
    
    }
}
