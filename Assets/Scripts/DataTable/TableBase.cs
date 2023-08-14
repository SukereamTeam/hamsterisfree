using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class TableBase<T>
{
    public abstract void SetTable(string key, string name, string value);

    protected Dictionary<string, T> dicData = new Dictionary<string, T>();
    public Dictionary<string, T> DicData => this.dicData;
}
