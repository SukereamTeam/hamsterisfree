using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using DataTable;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MonsterTile : TileBase
{
    [SerializeField]
    private Define.TileType_Sub subType = Define.TileType_Sub.Default;
    
    private ITileActor _tileActor;
    private bool _isMovingDone = false;

    private Vector2 _originPos = new Vector2();
    private Vector2 _startPos = new Vector2();
    private Vector2 _endPos = new Vector2();
    
    private Table_Monster.Param _monsterData;
    private Define.TileType_Sub _bossFunc = Define.TileType_Sub.Default;
    
    private CancellationTokenSource _moveCts;
    private CancellationTokenSource _actCts;
    private IDisposable _actDisposable = null;

    private const float TweenTime = 0.25f;

    public bool IsFuncStart { get; private set; }


    private new void Start()
    {
        base.Start();

        IsFuncStart = false;

        _actCts = new CancellationTokenSource();
        _moveCts = new CancellationTokenSource();
        
        GameManager.Instance.IsGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                ActClear();
            }).AddTo(this);
    }

    public override void Initialize(TileInfo info, Vector2 pos)
    {
        var posTuple = GetStartEndPosition(pos);
        _startPos = posTuple._Start;
        _endPos = posTuple._End;

        _originPos = pos;
        
        base.Initialize(info, _startPos);
        
        subType = Enum.Parse<Define.TileType_Sub>(info.SubType);

        var sprite = DataContainer.Instance.MonsterSprites[base._tileInfo.SubType];
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        
        _monsterData = DataContainer.Instance.MonsterTable.GetParamFromType(
            base._tileInfo.SubType, base._tileInfo.SubTypeIndex);

        if (_monsterData.Func != string.Empty)
        {
            _bossFunc = Enum.Parse<Define.TileType_Sub>(_monsterData.Func);
        }
    }

    public override void Reset()
    {
        transform.localScale = Vector3.one;

        // Moving Type 경우 주기적으로 처음 위치와 다른 곳으로 이동하기 때문에
        // Initialize에서 저장했던 originPos 값으로 다시 세팅해준다.
        var posTuple = GetStartEndPosition(_originPos);
        _startPos = posTuple._Start;
        _endPos = posTuple._End;

        TileCollider.enabled = true;

        ActClear();

        base.Reset();
    }

    public void TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행

        if (IsFuncStart == true)
        {
            return;
        }
        else if (IsFuncStart == false)
        {
            //Debug.Log("Monster Func Start !!!");
            IsFuncStart = true;
        }

        if (_actCts == null || _actCts.IsCancellationRequested == true)
        {
            _actCts = new CancellationTokenSource();
        }

        if (_moveCts == null || _moveCts.IsCancellationRequested == true)
        {
            _moveCts = new CancellationTokenSource();
        }

        Move(_tileInfo.ActiveTime, _monsterData.MoveSpeed, _moveCts).Forget();
        
        _tileActor = null;
        
        switch (subType)
        {
            case Define.TileType_Sub.Disappear:
            {
                _tileActor = new TileActor_Disappear();
            }
                break;
            case Define.TileType_Sub.Moving:
            {
                _tileActor = new TileActor_Moving();
            }
                break;
            case Define.TileType_Sub.Fade:
            {
                _tileActor = new TileActor_Fade();
            }
                break;
            case Define.TileType_Sub.Boss:
            {
                if (_bossFunc == Define.TileType_Sub.Moving)
                {
                    _tileActor = new TileActor_Moving();
                }
            }
                break;
        }
        
        if (_tileActor != null)
        {
            if (subType == Define.TileType_Sub.Moving || _bossFunc == Define.TileType_Sub.Moving)
            {
                return;
            }
            
            var task = _tileActor.Act(this, _actCts, _tileInfo.ActiveTime);
            _actDisposable = task.ToObservable().Subscribe(x =>
            {
                IsFuncStart = x;
            });
        }
        
    }

    private async UniTaskVoid Move(float _Time, float _Speed, CancellationTokenSource _Cts)
    {
        float timer = 0f;
        float progress = 0f;

        try
        {
            while (_Cts.IsCancellationRequested == false)
            {
                timer += Time.deltaTime;
            
                progress += Time.deltaTime * _Speed; //elapsedTime / _Time;
                progress = Mathf.Clamp01(progress);       // 0~1 사이 값 유지
            
                var newPosition = Vector3.Lerp(_startPos, _endPos, progress);
            
                transform.localPosition = new Vector3(newPosition.x, newPosition.y, -0.5f);
            
                if (progress >= 1f)
                {
                    progress = 0f;
                
                    if (timer >= _Time)
                    {
                        timer = 0f;
                        
                        if (subType == Define.TileType_Sub.Moving ||
                            _bossFunc == Define.TileType_Sub.Moving)
                        {
                            // 타일이 끝으로 완전히 간 다음 Position을 바꿔주고 싶어서 이렇게 구현
                            if (_tileActor != null)
                            {
                                _isMovingDone = false;
                                var task = _tileActor.Act(this, _actCts);
                                _actDisposable = task.ToObservable().Subscribe(x =>
                                {
                                    _isMovingDone = true;
                                    IsFuncStart = x;
                                });

                                await UniTask.WaitUntil(() => _isMovingDone == true, cancellationToken: _Cts.Token);

                                _isMovingDone = false;
                                Vector2 changePos = new Vector2(transform.localPosition.x,
                                    transform.localPosition.y);
                                var posTuple = GetStartEndPosition(changePos);
                                _startPos = posTuple._Start;
                                _endPos = posTuple._End;
                        
                                //originPos = changePos;
                                
                                continue;
                            }
                        }
                    }
                    
                    (_startPos, _endPos) = (_endPos, _startPos);
                }
            
                await UniTask.Yield();
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.LogError($"### Monster Move exception : {ex.Message} / {ex.StackTrace}");
        }

        IsFuncStart = false;
    }


    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"MonsterType : {_tileInfo.SubType}_{_tileInfo.SubTypeIndex} 닿음");

        TileCollider.enabled = false;
        GameManager.Instance.IsMonsterTrigger = true;
        
        if (_tileActor == null)
        {
            if (_moveCts != null || _moveCts?.IsCancellationRequested == false)
            {
                _moveCts.Cancel();
            }

            // Default 등 Func가 따로 없는 타일들을 위한
            IsFuncStart = false;
        }
        else
        {
            ActClear();
            _tileActor = null;
        }
        
        await UniTask.WaitUntil(() => IsFuncStart == false);
        
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_MONSTER.ToString()).Forget();

        // LimitTry Stage인 경우 Try 횟수 1 감소
        if (GameManager.Instance.StageManager.StageInfo.Type == Define.StageType.LimitTry)
        {
            GameManager.Instance.StageManager.ChangeStageValue(-1);
        }

        // 몬스터에 닿으면 다시 처음부터 시작해야 함 -> fade 걷히는 처리
        GameManager.Instance.MapManager.IsFade.Value = false;

        // Trigger 연출
        await DOTween.Sequence()
            .Append(transform.DOScale(1.5f, TweenTime))
            .Append(transform.DOScale(Vector3.one, TweenTime))
            .SetLoops(2, LoopType.Restart)
            .ToUniTask(cancellationToken: destroyCancellationToken);

        GameManager.Instance.RewindStage();
    }

    private (Vector2 _Start, Vector2 _End) GetStartEndPosition(Vector2 _Pos)
    {
        Vector2 startPosition = Vector2.zero;
        Vector2 endPosition = Vector2.zero;

        
        if (Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XStart) || Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XEnd))
        {
            if (Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XStart))
            {
                // 원래 In 안에서만 움직이게 하려고 했는데 (맵 내부), 너무 어려워서 내부를 넘어서 맵 outline까지 움직이도록 +1 더해줌
                startPosition = new Vector2((float)Define.MapSize.In_XEnd + 1f, _Pos.y);    
                endPosition = new Vector2(0f, _Pos.y);
            }
            else
            {
                startPosition = new Vector2(0f, _Pos.y);
                endPosition = new Vector2((float)Define.MapSize.In_XEnd + 1f, _Pos.y);
            }
        }
        else if (Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YStart) || Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YEnd))
        {
            if (Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YStart))
            {
                startPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YEnd);
                endPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YStart);
            }
            else
            {
                startPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YStart);
                endPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YEnd);
            }
        }

        return (startPosition, endPosition);
    }

    private void ActClear()
    {
        if (IsFuncStart == true)
        {
            IsFuncStart = false;
        }

        if (_actCts != null || _actCts?.IsCancellationRequested == false)
        {
            _actCts.Cancel();
        }

        if (_moveCts != null || _moveCts?.IsCancellationRequested == false)
        {
            _moveCts.Cancel();
        }

        _actDisposable?.Dispose();

        transform?.DOKill(true);
    }
    
    private void OnDestroy()
    {
        ActClear();

        _actCts?.Dispose();
        _moveCts?.Dispose();
    }
}
