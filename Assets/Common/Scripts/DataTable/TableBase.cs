using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public struct ObjectData
{
    // "Normal_((0, 0), (3, 0))"
    public string Type;
    public List<Tuple<int, int>> Pos;
}


public abstract class TableBase : MonoBehaviour
{
    public abstract void SetTable(string key, string name, string value);

    protected Dictionary<string, object> mDicData = new Dictionary<string, object>();
    public Dictionary<string, object> DicData => this.mDicData;
}
