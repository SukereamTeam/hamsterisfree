using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;





public class SeedTable : TableBase<SeedTable.SeedData>
{
    public record SeedData
    {
        public int Index { get; }

        public SeedData()
        {
            Index = -1;
        }
    }

    public override void SetTable(string key, string name, string value)
    {

    }

}
