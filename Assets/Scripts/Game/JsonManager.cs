using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;

[Serializable]
public struct TileData
{
    public string SubType;
    public int SubTypeIndex;
    public int RootIdx;

    public TileData(string _SubType, int _SubTypeIndex, int _RootIdx)
    {
        SubType = _SubType;
        SubTypeIndex = _SubTypeIndex;
        RootIdx = _RootIdx;
    }
}

[Serializable]
public class StageData
{
    public List<TileData> seedDatas;
    public List<TileData> monsterDatas;
    public int exitDataRootIdx;
}





public class JsonManager : Singleton<JsonManager>
{
    private readonly string StageData_Path = Path.Combine(Application.persistentDataPath, "positionData.json");
    
    
    
    
    public void SaveStageData(List<TileData> _Seed, List<TileData> _Monster, int _Exit)
    {
        StageData stageData = new StageData
        {
            seedDatas = _Seed,
            monsterDatas = _Monster,
            exitDataRootIdx = _Exit
        };
        
        string[] jsonLines = new string[]
        {
            JsonConvert.SerializeObject(stageData.seedDatas),
            JsonConvert.SerializeObject(stageData.monsterDatas),
            JsonConvert.SerializeObject(stageData.exitDataRootIdx)
        };
        
        File.WriteAllLines(StageData_Path, jsonLines);
        Debug.Log("### StageData Saved ###");
    }
    
    
    public StageData LoadStageData()
    {
        if (File.Exists(StageData_Path))
        {
            string jsonData = File.ReadAllText(StageData_Path);
            StageData loadedData = JsonConvert.DeserializeObject<StageData>(jsonData);
            
            Debug.Log("### StageData loaded ###");
            return loadedData;
        }
        else
        {
            Debug.LogError("No saved data found.");
            return null;
        }
    }
}
