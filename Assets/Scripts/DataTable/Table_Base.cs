using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataTable
{
    public class Table_Base : ScriptableObject
    {
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
}