using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;


public class StageTable : TableBase<StageTable, StageTable.StageData>
{
    private int index;
    private string stageType;
    private string mapName;
    private List<ObjectData> seedList = new List<ObjectData>(0);
    private List<ObjectData> monsterList = new List<ObjectData>(0);


    public record StageData
    {
        public int Index { get; }
        public string StageType { get; }
        public string MapName { get; }
        public List<ObjectData> SeedList { get; }
        public List<ObjectData> MonsterList { get; }

        public StageData()
        {
            Index = -1;
            StageType = "";
            MapName = "";
            SeedList = new List<ObjectData>(0);
            MonsterList = new List<ObjectData>(0);
        }

        public StageData(int _Index, string _StageType, string _MapName, List<ObjectData> _SeedList, List<ObjectData> _MonsterList)
        {
            Index = _Index;
            StageType = _StageType;
            MapName = _MapName;
            SeedList = new List<ObjectData>(_SeedList);
            MonsterList = new List<ObjectData>(_MonsterList);
        }
    }

    public record ObjectData
    {
        // "Normal_1_((0, 0), (3, 0))"
        public string Type { get; }
        public int Size { get; }
        public List<Tuple<int, int>> Pos { get; }

        public ObjectData(string _Type, int _Size, List<Tuple<int, int>> _Pos)
        {
            Type = _Type;
            Size = _Size;
            Pos = new List<Tuple<int, int>>(_Pos);
        }

        public ObjectData()
        {
            Type = "";
            Size = -1;
            Pos = new List<Tuple<int, int>>(0);
        }
    }

    public override void SetTable(string _Key, string _Name, string _Value)
    {
        StageData data;

        if (dicData.ContainsKey(_Key) == false)
        {
            data = new StageData();

            dicData.Add(_Key, data);
        }

        
        switch (_Name)
        {
            case "Index":
                this.index = Int32.Parse(_Value);
                break;
            case "StageType":
                this.stageType = _Value;
                break;
            case "MapName":
                this.mapName = _Value;
                break;
            case "SeedData":
                {
                    this.seedList = new List<ObjectData>(0);

                    if (_Value.Equals("NULL") == false)
                    {
                        this.seedList = new List<ObjectData>(GetListData(_Value));
                    }
                }
                break;
            case "MonsterData":
                {
                    this.monsterList = new List<ObjectData>(0);

                    if (_Value.Equals("NULL") == false)
                    {
                        this.monsterList = new List<ObjectData>(GetListData(_Value));
                    }
                }
                break;

            default: break;
        }

        if (_Name.Equals("MonsterData"))
        {
            // 한 줄의 마지막을 읽어올 때 데이터 넣어주기
            data = new StageData(this.index, this.stageType, this.mapName, this.seedList, this.monsterList);

            dicData[_Key] = data;

            Reset();
        }
    }




    private List<ObjectData> GetListData(string _Value)
    {
        var splitList = _Value.Split('+');

        List<ObjectData> list = new List<ObjectData>(splitList.Length);

        (string type, int size, List<Tuple<int, int>> pos) value = ("", -1, new List<Tuple<int, int>>(0));

        for (int i = 0; i < splitList.Length; i++)
        {
            var splitData = splitList[i].Replace("(", "").Replace(")", "").Split('_');

            
            for (int j = 0; j < splitData.Length; j++)
            {
                if (j == 0)
                {
                    value.type = splitData[j];
                }
                else if (j == 1)
                {
                    value.size = Int32.Parse(splitData[j]);
                }
                else
                {
                    value.pos = new List<Tuple<int, int>>(ParsingPosition(splitData[j]));
                }
            }

            ObjectData data = new ObjectData(value.type, value.size, value.pos);

            list.Add(data);
        }

        

        return list;
    }

    private List<Tuple<int, int>> ParsingPosition(string _Value)
    {
        // "((0, 0), (3, 0))"
        string[] pairs = _Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        // "0030"

        List<Tuple<int, int>> tuplePos = new List<Tuple<int, int>>(pairs.Length);


        for (int i = 0; i < pairs.Length; i += 2)
        {
            int x = int.Parse(pairs[i]);
            int y = int.Parse(pairs[i + 1]);

            tuplePos.Add(new Tuple<int, int>(x, y));
        }

        return tuplePos;
    }

    private void Reset()
    {
        this.index = -1;
        this.stageType = "";
        this.mapName = "";
        this.seedList.Clear();
        this.monsterList.Clear();
    }
}
