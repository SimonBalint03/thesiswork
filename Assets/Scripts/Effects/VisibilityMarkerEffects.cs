using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VisibilityMarkerEffects : MonoBehaviour
{
    public Vector2 range;
    private void Start()
    {
        transform.position = new Vector3(transform.position.x, Random.Range(range.x,range.y), transform.position.z);
    }
}
