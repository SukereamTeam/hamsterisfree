using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private MapManager mapManager = null;
    public MapManager MapManager => this.mapManager;


    private IReactiveProperty<bool> isGame = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsGame
    {
        get => this.isGame;
        set => this.isGame = value;
    }




    private void Start()
    {
        IsGame.Value = true;


    }

    private void Update()
    {

    }
}
