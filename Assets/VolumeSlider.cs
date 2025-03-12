using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum Type
    {
        Music,
        SFX,
        Ambient
    }
    private Slider slider;
    [SerializeField] private Type type = Type.Music;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite muted;
    [SerializeField] private Sprite unMuted;
    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.value = type switch
        {
            Type.Music => PlayerPrefs.GetFloat("MusicVolume", 1f),
            Type.SFX => PlayerPrefs.GetFloat("SFXVolume", 1f),
            Type.Ambient => PlayerPrefs.GetFloat("AmbientVolume", 1f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void Update()
    {
        icon.sprite = slider.value == 0 ? muted : unMuted;
    }

    public void ControlMusicVolume()
    {
        AudioManager.Instance.SetMusicVolume(slider.value);
    }
    public void ControlSFXVolume()
    {
        AudioManager.Instance.SetSFXVolume(slider.value);
    }
    public void ControlAmbientVolume()
    {
        AudioManager.Instance.SetAmbientVolume(slider.value);
    }
}
