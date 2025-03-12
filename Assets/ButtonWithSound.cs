using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonWithSound : MonoBehaviour
{
    public void PlaySound()
    {
        AudioManager.Instance.OnButtonClick();
    }
}