using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{


    private bool isGameStart = false;
    public bool IsGameStart
    {
        get => this.isGameStart;
        set => this.isGameStart = value;
    }

    private bool isFade = false;
    public bool IsFade
    {
        get => this.isFade;
        set => this.isFade = value;
    }
}
