using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hexes;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public HexBoard board;
    
    private void Awake()
    {
        string savePath = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath); // Ensure the directory exists
        }

        //SaveBoard(board,savePath);
    }

    public void SaveBoard(HexBoard board, string filePath)
    {
        filePath = filePath + "/map-" + board.mapSeed.ToString() + ".json";
        string json = JsonUtility.ToJson(board);
        File.WriteAllText(filePath, json);
        Debug.Log("Board Saved to: " + filePath);
    }
}
