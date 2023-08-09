using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MonsterTable : TableBase<MonsterTable.MonsterData>
{
    public record MonsterData
    {
        public int Index { get; }

        public MonsterData()
        {
            Index = -1;
        }
    }

    public override void SetTable(string key, string name, string value)
    {
        throw new NotImplementedException();
    }
}
