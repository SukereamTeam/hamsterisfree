using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DataTable
{
    [Serializable]
    public class Table_Lobby : Table_Base
    {
        [SerializeField]
        public List<Param> list = new List<Param>();
        
        [Serializable]
        public class Param
        {
            public int Horizontal;
            public int Vertical;
            public float Spacing;
        }
        
        
    }
}

