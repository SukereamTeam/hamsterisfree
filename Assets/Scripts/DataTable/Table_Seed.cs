using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }
    }
}