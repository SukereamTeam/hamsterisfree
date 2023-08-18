using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DataTable
{
    [Serializable]
    public class Table_Seed : Table_Base
    {
        [SerializeField]
        public List<Param> list = new List<Param>();

        [Serializable]
        public class Param
        {
            public int Index;
            public string Type;
            public int ActiveTime;
            public string SpritePath;
        }

        public Param GetParamFromType(string _Type)
        {
            return list.FirstOrDefault(x => x.Type.Equals(_Type));
        }
    }
}