using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table_Stage : Table_Base
{
    public List<Param> list = new List<Param>();

    [Serializable]
    public class Param
    {
        public int Index;
        public SerializableTuple<string, int> StageType;
        public string MapName;
        public List<SerializableTuple<string, int>> SeedData;
        public List<SerializableTuple<string, int>> MonsterData;
    }
}
