using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;


public struct StageData
{
    public int Index;
    public string StageType;
    public string MapName;
    public List<TableBase.ObjectData> SeedList;
    public List<TableBase.ObjectData> MonsterList;
}

public class StageTable : TableBase
{
    public override void SetTable(string _Key, string _Name, string _Value)
    {
        StageData data;

        if (mDicData.ContainsKey(_Key) == false)
        {
            data = new StageData();

            mDicData.Add(_Key, data);
        }

        data = (StageData)mDicData[_Key];


        switch (_Name)
        {
            case "Index":
                data.Index = Int32.Parse(_Value);
                break;
            case "StageType":
                data.StageType = _Value;
                break;
            case "MapName":
                data.MapName = _Value;
                break;
            case "SeedData":
                {
                    data.SeedList = new List<ObjectData>();

                    if (_Value.Equals("NULL") == false)
                    {
                        data.SeedList = GetListData(_Value);
                    }
                }
                break;
            case "MonsterData":
                {
                    data.MonsterList = new List<ObjectData>();

                    if (_Value.Equals("NULL") == false)
                    {
                        data.MonsterList = GetListData(_Value);
                    }
                }
                break;

            default: break;
        }

        mDicData[_Key] = data;
    }




    public List<ObjectData> GetListData(string _Value)
    {
        List<ObjectData> list = new List<ObjectData>();

        var splitList = _Value.Split('+');

        (string, int, List<Tuple<int, int>>) value = ("", -1, new List<Tuple<int, int>>());

        for (int i = 0; i < splitList.Length; i++)
        {
            var splitData = splitList[i].Replace("(", "").Replace(")", "").Split('_');

            
            for (int j = 0; j < splitData.Length; j++)
            {
                if (j == 0)
                {
                    value.Item1 = splitData[j];
                }
                else if (j == 1)
                {
                    value.Item2 = Int32.Parse(splitData[j]);
                }
                else
                {
                    value.Item3 = new List<Tuple<int, int>>(ParsingPosition(splitData[j]));
                }
            }

            ObjectData data = new ObjectData(value.Item1, value.Item2, value.Item3);

            list.Add(data);
        }

        

        return list;
    }

    private List<Tuple<int, int>> ParsingPosition(string _Value)
    {
        List<Tuple<int, int>> tuplePos = new List<Tuple<int, int>>();

        // "((0, 0), (3, 0))"

        string[] pairs = _Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        // "0030"

        for (int i = 0; i < pairs.Length; i += 2)
        {
            int x = int.Parse(pairs[i]);
            int y = int.Parse(pairs[i + 1]);

            tuplePos.Add(new Tuple<int, int>(x, y));
        }

        return tuplePos;
    }
}
