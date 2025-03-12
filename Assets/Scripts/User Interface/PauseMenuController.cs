using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Effects;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace User_Interface
{
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pauseMenu;
        public GameObject fadePanel;
        public Button resumeButton;
        public Button exitButton;
        public List<GameObject> otherMenus = new List<GameObject>();
        private CameraController cameraController;
        private MovementController movementController;
        private EffectsController effectsController;
        private GameManager gameManager;

        private void Awake()
        {
            cameraController = FindObjectOfType<CameraController>();
            movementController = FindObjectOfType<MovementController>();
            effectsController = FindObjectOfType<EffectsController>();
            gameManager = FindObjectOfType<GameManager>();
        }

        private void Update()
        {
            if (gameManager.isGameOver) { return; }
            if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeSelf)
            {
                Resume();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && otherMenus.All(menu => !menu.activeSelf))
            {
                Pause();
            }

        }

        public void Pause()
        {
            StartCoroutine(PauseRoutine());
        }

        public void Resume()
        {
            StartCoroutine(ResumeRoutine());
        }

        public void Exit(bool confirm = true)
        {
            if (confirm)
            {
                UIManager.Instance.ShowConfirmation("Are you sure you want to give up and exit? You will not be able to continue this game.",
                    () => { SceneManager.LoadScene(0); },  // Confirm action
                    () => { Debug.Log("Exit cancelled."); } // Cancel action
                );
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    
        IEnumerator PauseRoutine()
        {
            pauseMenu.transform.localScale = Vector3.zero;
            ToggleControllersOnPause(false);
            pauseMenu.SetActive(true);
            fadePanel.SetActive(false);
            pauseMenu.LeanScale(Vector3.one, 0.2f);
            yield return new WaitForSeconds(0.2f);
            fadePanel.transform.localScale = Vector3.one;
            fadePanel.SetActive(true);
        }

        IEnumerator ResumeRoutine()
        {
            pauseMenu.LeanScale(Vector3.zero, 0.2f);
            fadePanel.transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(0.2f);
            ToggleControllersOnPause(true);
            fadePanel.SetActive(false);
            pauseMenu.SetActive(false);
        }
    
        private void ToggleControllersOnPause(bool active)
        {
            movementController.enabled = active;
            cameraController.enabled = active;
            effectsController.enabled = active;
        }
    }
}
