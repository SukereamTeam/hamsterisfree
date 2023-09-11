using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;



public class JsonManagerOld : Singleton<JsonManagerOld>
{
    private readonly string StageData_Path = "StageData.json";

    private Dictionary<int, StageData> stageDatas = new Dictionary<int, StageData>();

    
    
    
    public void SaveStageData(int _StageIndex, List<TileData> _Seed, List<TileData> _Monster, int _Exit)
    {
        string path = Path.Combine(Application.persistentDataPath, "StageData.json");

        StageData stageData = new StageData(_Seed, _Monster, _Exit);
        
        stageDatas.Add(_StageIndex, stageData);
        
        string json = JsonConvert.SerializeObject(stageDatas, Formatting.Indented);
        File.WriteAllText(path, json);
    }
    
    
    public StageData LoadStageData(int _StageIndex)
    {
        string path = Path.Combine(Application.persistentDataPath, "StageData.json");
        
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            
            this.stageDatas = JsonConvert.DeserializeObject<Dictionary<int, StageData>>(jsonData);

            if (this.stageDatas.TryGetValue(_StageIndex, out StageData data))
            {
                return data;
            }
        }

        return null;
    }
}
