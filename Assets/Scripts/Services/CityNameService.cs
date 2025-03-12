using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services
{
    public class CityNameService : MonoBehaviour
    {
        public TextAsset cityNameFile;
        [SerializeField]private static List<string> cityNames = new List<string>();
        
        public static string GetRandomCityName()
        {
            int index = Random.Range(0, cityNames.Count);
            string result = cityNames[index];
            cityNames.RemoveAt(index);
            return result;
        }

        private void Awake()
        {
            //Debug.Log(cityNameFile.text);
            
            foreach (string str in cityNameFile.text.Split("\n"))
            {
                cityNames.Add(str);
                //Debug.Log(str);
            }
            
        }
    }
}
