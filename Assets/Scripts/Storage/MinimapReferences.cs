using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

public class MinimapReferences : MonoBehaviour
{
    [Separator("Icons")] 
    public Sprite waveIcon;
    public Sprite riverIcon;
    public List<Sprite> mountainIcons;
    public List<Sprite> hillIcons;
    public List<Sprite> grassIcons;
    public List<Sprite> forestIcons;
    public Sprite BigCityIcon;
    public Sprite VillageIcon;
    public Sprite OutpostIcon;


    public Sprite GetRandomMountainIcon()
    {
        return mountainIcons[Random.Range(0, mountainIcons.Count)];
    }
    
    public Sprite GetRandomHillIcon()
    {
        return hillIcons[Random.Range(0, hillIcons.Count)];
    }
    public Sprite GetRandomGrassIcon()
    {
        return grassIcons[Random.Range(0, grassIcons.Count)];
    }
    public Sprite GetRandomForestIcon()
    {
        return forestIcons[Random.Range(0, forestIcons.Count)];
    }
}
