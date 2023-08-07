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
    public ObjectData SeedList;
    public ObjectData MonsterList;
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
                    data.SeedList = new ObjectData();

                    if (_Value.Equals("NULL"))
                    {
                        return;
                    }
                    data.SeedList = GetListData(_Value);
                }
                break;
            case "MonsterData":
                {
                    data.MonsterList = new ObjectData();

                    if (_Value.Equals("NULL"))
                    {
                        return;
                    }
                    data.MonsterList = GetListData(_Value);
                }
                break;

            default: break;
        }

        mDicData[_Key] = data;
    }




    public ObjectData GetListData(string _Value)
    {
        ObjectData data = new ObjectData();

        var splitData = _Value.Split('_');

        for (int i = 0; i < splitData.Length; i++)
        {
            if (i == 0)
            {
                data.Type = splitData[i];
            }
            else
            {
                data.Pos = new List<Tuple<int, int>>(ParsingPosition(_Value));
            }
        }

        return data;
    }

    private List<Tuple<int, int>> ParsingPosition(string _Value)
    {
        List<Tuple<int, int>> tuplePos = new List<Tuple<int, int>>();

        // "((0, 0), (3, 0))"

        string[] pairs = _Value.Replace("(", "").Replace(")", "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
