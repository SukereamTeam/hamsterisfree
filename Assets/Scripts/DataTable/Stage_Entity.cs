using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_Entity : ScriptableObject
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

    [Serializable]
    public class SerializableTuple<T1, T2>
    {
        public T1 Type;
        public T2 Count;

        public SerializableTuple(T1 item1, T2 item2)
        {
            Type = item1;
            Count = item2;
        }
    }
}
