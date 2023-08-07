using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class TableBase
{
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
            Pos = new List<Tuple<int, int>>();
        }
    }

    public abstract void SetTable(string key, string name, string value);

    protected Dictionary<string, object> mDicData = new Dictionary<string, object>();
    public Dictionary<string, object> DicData => this.mDicData;
}
