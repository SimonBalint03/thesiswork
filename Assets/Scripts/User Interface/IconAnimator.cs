using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

public class IconAnimator : MonoBehaviour
{
    [Separator("Animation Settings")]
    public List<Sprite> sprites; 
    public float animationSpeed = 1f; 

    private Image _imageComponent; 
    private int _currentFrame = 0;
    private float _timer = 0f; 

    private bool flipped = false;
    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
        if (_imageComponent == null)
        {
            Debug.LogError("No Image component found on this GameObject.");
            enabled = false; // Disable the script if no Image component is found
        }

        if (sprites is { Count: > 0 })
        {
            _imageComponent.sprite = sprites[0];
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= animationSpeed)
        {
            _timer = 0f; 
            _currentFrame = (_currentFrame + 1) % sprites.Count;
            _imageComponent.sprite = sprites[_currentFrame];
        }
    }
    
    public void FlipImage(bool flipX, bool flipY)
    {
        if (flipped) { return; }
        RectTransform rectTransform = _imageComponent.rectTransform;

        if (flipX)
        {
            rectTransform.localScale = new Vector3(-rectTransform.localScale.x, rectTransform.localScale.y, rectTransform.localScale.z);
            flipped = true;
        }

        if (flipY)
        {
            rectTransform.localScale = new Vector3(rectTransform.localScale.x, -rectTransform.localScale.y, rectTransform.localScale.z);
            flipped = true;
        }
    }

    public void ResetFlip()
    {
        RectTransform rectTransform = _imageComponent.rectTransform;
        rectTransform.localScale = new Vector3(Mathf.Abs(rectTransform.localScale.x), Mathf.Abs(rectTransform.localScale.y), rectTransform.localScale.z);
        flipped = false;
    }
    
    public void SetAnimationSpeed(float newSpeed)
    {
        animationSpeed = newSpeed;
    }
    
    public void SetSprites(List<Sprite> newSprites)
    {
        if (newSprites != null && newSprites.Count > 0)
        {
            sprites = newSprites;
            _currentFrame = 0; // Reset to the first frame
            _imageComponent.sprite = sprites[0]; // Update the sprite
        }
        else
        {
            Debug.LogWarning("New sprite list is empty or null.");
        }
    }
}
