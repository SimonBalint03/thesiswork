using System;
using System.Collections;
using System.Collections.Generic;
using Singletons;
using UnityEngine;

public class CityLightsController : MonoBehaviour
{
    public List<Light> lights = new List<Light>();
    
    private SunAndMoonController sunAndMoonController;

    private void Awake()
    {
        sunAndMoonController = FindObjectOfType<SunAndMoonController>();
    }

    private void OnEnable()
    {
        TimeOfDay.OnNextStep += OnNextStep;
    }

    private void OnDisable()
    {
        TimeOfDay.OnNextStep -= OnNextStep;
    }

    private void OnNextStep()
    {
        foreach (Light light1 in lights)
        {
            light1.intensity = sunAndMoonController.moonLight.intensity / 2;
        }
    }
}
