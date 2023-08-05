using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;


public enum Direction
{
    Left = 0,
    Right,
    Top,
    Bottom
}

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
        this.isGame.Value = true;

        this.isGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                GameEndFlow();
            }).AddTo(this);
    }

    private void GameEndFlow()
    {
        Debug.Log("### Game End ###");

        // TODO
        // 지금은 이걸로 페이드 없애버리지만 나중엔 애니 효과든 뭐든 넣어야 함
        GameManager.Instance.MapManager.IsFade.Value = false;
    }
}
