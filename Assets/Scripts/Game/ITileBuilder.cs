using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ITileBuilder
{
    TileBase GetTile();

}

public enum TileType
{
    Seed,
    Monster
}