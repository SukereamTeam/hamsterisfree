using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class TableBase<T, S>
{
    public abstract void SetTable(string key, string name, string value);

    protected Dictionary<string, S> dicData = new Dictionary<string, S>();
    public Dictionary<string, S> DicData => this.dicData;
}
