using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshStorage : MonoBehaviour
{
    public Mesh flatBase;
    public Mesh flatRiverBase;
    public Mesh hillBase;
    public Mesh hillRiverBase;
    public Mesh[] mountainBase;
    
    public Mesh waterShallow;
    public Mesh waterMid;
    public Mesh waterDeep;

    public Mesh GetRandomMountain()
    {
        return mountainBase[Random.Range(0, mountainBase.Length)];
    }
}
