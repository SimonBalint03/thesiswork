using UnityEngine;
using UnityEngine.UI;
using System;
using Controllers;
using Effects;
using TMPro;

public class ConfirmScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirm;
    private Action onCancel;
    
    
    private CameraController cameraController;
    private MovementController movementController;
    private EffectsController effectsController;

    private void OnEnable()
    {
        //gameObject.SetActive(false);
        if (!cameraController)
        {
            cameraController = FindObjectOfType<CameraController>();
        }

        if (!movementController)
        {
            movementController = FindObjectOfType<MovementController>();
        }

        if (!effectsController)
        {
            effectsController = FindObjectOfType<EffectsController>();
        }
    }

    public void Show(string question, Action confirmCallback, Action cancelCallback = null)
    {
        questionText.text = question;
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(() => { onConfirm?.Invoke(); Close(); });
        cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Close(); });
        
        gameObject.SetActive(true);
        
        ToggleControllers(false);
    }

    private void Close()
    {
        ToggleControllers(true);
        gameObject.SetActive(false);
    }
    
    private void ToggleControllers(bool active)
    {
        movementController.enabled = active;
        cameraController.enabled = active;
        effectsController.enabled = active;
    }
}