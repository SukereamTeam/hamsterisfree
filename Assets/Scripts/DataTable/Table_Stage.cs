using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataTable
{
    [Serializable]
    public class Table_Stage : Table_Base
    {
        [SerializeField]
        public List<Param> list = new List<Param>();

        [Serializable]
        public class Param
        {
            public int Index;
            public SerializableTuple<string, int> StageType;
            public string MapName;
            public List<SerializableTuple<string, int, int>> SeedData;
            public List<SerializableTuple<string, int, int>> MonsterData;
        }
    }
}