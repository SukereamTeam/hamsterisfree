using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class GameManager : MonoSingleton<GameManager>
{

    [SerializeField]
    private MapManager mapManager;
    public MapManager MapManager => this.mapManager;


    [SerializeField]
    private float fadeDuration = 0f;



    private IReactiveProperty<bool> isGame = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsGame
    {
        get => this.isGame;
        set => this.isGame = value;
    }

    [SerializeField]
    private int seedCount;
    public int SeedCount
    {
        get => this.seedCount;
        set => this.seedCount = value;
    }





    private async void Start()
    {
        Debug.Log("GameManagere에서 Start 진입");

        if (Instance == null)
        {
            _instance = this;
        }

        this.isGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                GameEndFlow();
            }).AddTo(this);



        MapManager.SetStage(CommonManager.Instance.CurStageIndex, DataContainer.Instance.StageSprites);

        await SceneController.Instance.Fade(true, this.fadeDuration, false, new CancellationTokenSource());

        await UniTask.Delay(TimeSpan.FromMilliseconds(5000));

        this.isGame.Value = true;
    }




    private void GameEndFlow()
    {
        Debug.Log("### Game End ###");

        // TODO
        // 지금은 이걸로 페이드 없애버리지만 나중엔 애니 효과든 뭐든 넣어야 함
        MapManager.IsFade.Value = false;
    }

    public async void OnClick_Back()
    {
        await SceneController.Instance.Fade(false, this.fadeDuration, false, new CancellationTokenSource());

        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();

        Clear();
    }
}
