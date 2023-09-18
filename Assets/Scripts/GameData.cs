using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



// Stage 데이터 저장 --------------------
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

    public StageData(List<TileData> _Seed, List<TileData> _Monster, int _Exit)
    {
        seedDatas = _Seed;
        monsterDatas = _Monster;
        exitDataRootIdx = _Exit;
    }
}


// User Data 저장 --------------------
[Serializable]
public class UserData
{
    public int curStage;
    public int rewardCount;
}
