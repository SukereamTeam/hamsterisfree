using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Intro,
        Lobby,
        Game,
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

    public enum TileSpriteName
    {
        Center = 0,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Mask
    }

    public enum ObjectDataField
    {
        Type = 0,
        Size,
        Pos
    }
}
