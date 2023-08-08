using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Intro,
        Game,
        Lobby
    }

    public enum Direction
    {
        Left = 0,
        Right,
        Top,
        Bottom
    }

    public enum TileType
    {
        Normal = 0,
        Exit,
        Seed,
        Monster
    }


}
