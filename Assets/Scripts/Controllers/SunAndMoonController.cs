using System;
using Singletons;
using UnityEngine;

public class SunAndMoonController : MonoBehaviour
{
    public Light sunLight;
    public Light moonLight;
    public float transitionSpeed = 1f;
    
    public float targetFogIntensity;

    private Quaternion originalSunRotation;
    private Quaternion originalMoonRotation;
    private Quaternion targetSunRotation;
    private Quaternion targetMoonRotation;
    
    private float targetSunIntensity;
    private float targetMoonIntensity;
    
    
    public bool isRotating = false;

    private void Awake()
    {
        originalSunRotation = sunLight.transform.localRotation;
        originalMoonRotation = moonLight.transform.localRotation;
        
        moonLight.intensity = 0f;
        targetMoonIntensity = moonLight.intensity;
        sunLight.intensity = 1.5f;
        targetSunIntensity = sunLight.intensity;
        
        targetSunRotation = originalSunRotation;
        targetMoonRotation = originalMoonRotation;
    }

    private void OnEnable()
    {
        TimeOfDay.OnNextStep += MoveLight;
    }

    private void OnDisable()
    {
        TimeOfDay.OnNextStep -= MoveLight;
    }

    private void Update()
    {
        if (isRotating)
        {
            RotateSmoothly();
        }
    }

    private void MoveLight()
    {
        float timeOfDay = TimeOfDay.Instance.CurrentStep.Time;

        switch (timeOfDay)
        {
            case 17:
                sunLight.transform.localRotation = originalSunRotation;
                targetSunRotation = originalSunRotation;
                SetTargetRotation(moonLight);
                break;
            case 7:
                moonLight.transform.localRotation = originalMoonRotation;
                targetMoonRotation = originalMoonRotation;
                SetTargetRotation(sunLight);
                break;
            case 8: // Sunset
                SetTargetRotation(sunLight);
                //moonLight.intensity = 0.5f;
                //sunLight.intensity = 1f;
                
                targetFogIntensity = 0.02f;

                targetMoonIntensity = 0.4f;
                targetSunIntensity = 1f;
                break;
            case 9:
                SetTargetRotation(sunLight);
                //moonLight.intensity = 1f;
                //sunLight.intensity = 0.5f;
                
                targetMoonIntensity = 0.6f;
                targetSunIntensity = 0.5f;
                break;
            case 18: // Dawn
                SetTargetRotation(moonLight);
                //moonLight.intensity = 1.2f;
                //sunLight.intensity = 0.5f;
                targetFogIntensity = 0.02f;
                
                
                targetMoonIntensity = 0.8f;
                targetSunIntensity = 0.5f;
                break;
            case 19:
                SetTargetRotation(moonLight);
                //moonLight.intensity = 1f;
                //sunLight.intensity = 1f;
                targetFogIntensity = 0.01f;
                
                
                targetMoonIntensity = 0.6f;
                targetSunIntensity = 1f;
                break;
            case <= 7:
                SetTargetRotation(sunLight);
                //moonLight.intensity = 0f;
                //sunLight.intensity = 1.5f;
                targetFogIntensity = 0.01f;
                
                
                targetMoonIntensity = 0f;
                targetSunIntensity = 1.2f;
                break;
            default:
                SetTargetRotation(moonLight);
                //moonLight.intensity = 1.2f;
                //sunLight.intensity = 0f;
                
                targetFogIntensity = 0.03f;
                
                targetMoonIntensity = 0.8f;
                targetSunIntensity = 0f;
                break;
        }
        
        isRotating = true;
    }

    private void SetTargetRotation(Light light)
    {
        // Set the target rotation by rotating 15 degrees
        if (light == sunLight)
        {
            targetSunRotation = light.transform.localRotation * Quaternion.Euler(13f, 8f, 0f);
        }
        else if (light == moonLight)
        {
            targetMoonRotation = light.transform.localRotation * Quaternion.Euler(13f, 8f, 0f);
        }
    }

    private void RotateSmoothly()
    {
        // Smoothly rotate the sun and moon lights over time using Lerp
        sunLight.transform.localRotation = Quaternion.Lerp(sunLight.transform.localRotation, targetSunRotation, Time.deltaTime * transitionSpeed);
        moonLight.transform.localRotation = Quaternion.Lerp(moonLight.transform.localRotation, targetMoonRotation, Time.deltaTime * transitionSpeed);
        
        sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetSunIntensity, Time.deltaTime * transitionSpeed);
        moonLight.intensity = Mathf.Lerp(moonLight.intensity, targetMoonIntensity, Time.deltaTime * transitionSpeed);

        // Check if the rotation is close to the target and stop rotating
        if (Quaternion.Angle(sunLight.transform.localRotation, targetSunRotation) < 0.1f && 
            Quaternion.Angle(moonLight.transform.localRotation, targetMoonRotation) < 0.1f)
        {
            isRotating = false;
        }
        
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogIntensity, Time.deltaTime * transitionSpeed);
    }
}
