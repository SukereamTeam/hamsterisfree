using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DataTable
{
    [Serializable]
    public class Table_Monster : Table_Base
    {
        [SerializeField]
        public List<Param> list = new List<Param>();

        [Serializable]
        public class Param
        {
            public int UID;
            public string Type;
            public int TypeIndex;
            public float ActiveTime;
            public SerializableTuple<int, int> Size;
            public string Func;
            public string SpritePath;
        }

        public Param GetParamFromType(string _Type, int _TypeIndex)
        {
            return list.FirstOrDefault(x =>
            {
                return (x.Type.Equals(_Type) && x.TypeIndex.Equals(_TypeIndex));
            });
        }
    }
}