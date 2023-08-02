using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private MapManager mapManager = null;
    public MapManager MapManager => this.mapManager;

    private bool isGameStart = false;
    public bool IsGameStart
    {
        get => this.isGameStart;
        set => this.isGameStart = value;
    }




    private void Start()
    {
        
    }

    private void Update()
    {
        if (this.isGameStart)
        {
            Debug.Log("Start Now !!!");
        }
    }
}
