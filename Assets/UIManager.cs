using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private ConfirmScreen confirmScreen;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowConfirmation(string message, Action onConfirm, Action onCancel = null)
    {
        Debug.Log("Showing Confirmation");
        confirmScreen.Show(message, onConfirm, onCancel);
    }
}

