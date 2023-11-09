using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataTable;
using System.Linq;



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

    public bool EqualsWithStageParam(Table_Stage.Param stageTable)
    {
        if (EqualsWithTableItemDataList(this.seedDatas, stageTable.SeedData) == false)
        {
            return false;
        }

        return EqualsWithTableItemDataList(this.monsterDatas, stageTable.MonsterData);
    }

    private bool EqualsWithTableItemDataList(IReadOnlyList<TileData> tileDataList, IReadOnlyList<Table_Base.SerializableTuple<string, int, int>> tableItemDataList)
    {
        var tableSeedCount = tableItemDataList.Select(x => x.Item3).Sum();
        if (tableSeedCount != tileDataList.Count)
            return false;

        int idx = 0;
        for (int i = 0; i < tableItemDataList.Count; i++)
        {
            for (int j = 0; j < tableItemDataList[i].Item3; j++)
            {
                if (tableItemDataList[i].Item1 != tileDataList[idx].SubType)
                {
                    return false;
                }
                else if (tableItemDataList[i].Item2 != tileDataList[idx].SubTypeIndex)
                {
                    return false;
                }

                idx++;
            }
        }
        return true;
    }
}


// User Data 저장 --------------------
[Serializable]
public class UserData
{
    public int curStage;
    public int rewardCount;
}
