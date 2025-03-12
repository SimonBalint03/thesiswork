using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River
{
    [Serializable]
    public enum RiverType
    {
        LargeRiver = 0,
        River = 1,
        SmallRiver = 2,
    }
    
    public enum RiverFlowDirection
    {
        
    }

    public RiverType Type { get; set; }
    public Vector2Int ParentBoardPosition { get; set; }
    public List<GameObject> FlowFromGameObjects { get; set; } = new List<GameObject>();
    public List<GameObject> FlowToGameObjects { get; set; } = new List<GameObject>();

    public River(RiverType type, Vector2Int parentBoardPosition)
    {
        Type = type;
        ParentBoardPosition = parentBoardPosition;
    }
}
