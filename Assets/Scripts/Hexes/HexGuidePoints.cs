using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class HexGuidePoints : MonoBehaviour
{
    public GameObject[] corners = new GameObject[6];
    [Separator("Trees")]
    public GameObject[] riverTreeContainers = new GameObject[6];
    public GameObject riverCenterTreeContainer;
    public GameObject defaultTreeContainer;
    public GameObject[] riverEnds = new GameObject[6];

    public GameObject middle;
    
    private List<GameObject> treesPlaced = new List<GameObject>();
    
    void Start()
    {
        GameObject corners_go = transform.Find("Base").Find("Corners").gameObject;
        for (int i = 0; i < corners_go.transform.childCount; i++)
        {
            corners[i] = corners_go.transform.GetChild(i).gameObject;
        }
        
        middle = transform.Find("Base").Find("Middle").gameObject;
    }

    public void AddTree(GameObject tree)
    {
        treesPlaced.Add(tree);
    }

    public List<GameObject> GetTrees()
    {
        return treesPlaced;
    }

    public GameObject GetRandomTree()
    {
        if (treesPlaced.Count == 0) {return null;}
        return treesPlaced[Random.Range(0, treesPlaced.Count)];
    }
}
