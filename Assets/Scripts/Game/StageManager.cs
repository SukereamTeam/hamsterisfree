using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class StageManager : MonoBehaviour
{
    [Serializable]
    public struct StageInfoData
    {
        public Define.StageType Type;
        public int LimitValue;
        public int StageSeedCount;

        public StageInfoData(Define.StageType _Type, int _LimitValue, int _StageSeedCount)
        {
            Type = _Type;
            if (_Type == Define.StageType.LimitTime)
            {
                // 주어진 초 단위 시간(60)을 밀리초로 변환
                _LimitValue *= 1000;
            }
            LimitValue = _LimitValue;
            StageSeedCount = _StageSeedCount;
        }
    }
    
    [SerializeField]
    private StageInfoData stageInfo = new StageInfoData();
    public StageInfoData StageInfo => this.stageInfo;

    [SerializeField]
    private TextMeshProUGUI stageLimitText;
    
    [SerializeField]
    private TextMeshProUGUI stageNumberText;
    
    [SerializeField]
    private TextMeshProUGUI seedInfoText;
    
    // LimitTime타입의 스테이지 일 때의 time값 혹은 LimitTry타입의 스테이지 일 때의 try값
    // default 일 땐 표시하지 않는다.
    private IReactiveProperty<float> curValue = new ReactiveProperty<float>(0f);
    public IReactiveProperty<float> CurValue
    {
        get => this.curValue;
    }
    
    private float timer = 0f;


    private void Update()
    {
        if (GameManager.Instance.IsGame.Value == false)
            return;

        if (this.stageInfo.Type == Define.StageType.LimitTime)
        {
            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                // 1초에 1씩 깎는 것
                this.curValue.Value -= 100f;
                timer -= 0.1f;
            }
        }
    }

    public void SetStage(int _StageNumber, Define.StageType _Type, int _LimitValue, int _StageSeedCount)
    {
        this.stageInfo = new StageInfoData(_Type, _LimitValue, _StageSeedCount);
        
        // Stage 번호 출력
        this.stageNumberText.text = _StageNumber.ToString();
        
        if (this.stageInfo.Type == Define.StageType.LimitTime ||
            this.stageInfo.Type == Define.StageType.LimitTry)
        {
            this.curValue.Value = this.stageInfo.LimitValue;
            
            this.curValue
                .Skip(TimeSpan.Zero)  // 첫 프레임 호출 스킵
                .Where(x => x <= 0f)
                .Subscribe(_ =>
                {
                    // 시간이 다 되었거나, 기회를 다 잃었다면 게임 종료
                    GameManager.Instance.IsGame.Value = false;
                }).AddTo(this);
        }
        
        this.curValue.Subscribe(newValue =>
        {
            var value = curValue.Value;
            var maxValue = this.stageInfo.LimitValue;
            if (this.stageInfo.Type == Define.StageType.LimitTime)
            {
                // 밀리초 -> 초 변환해서 출력
                value = Mathf.Round(value /= 1000);
                maxValue /= 1000;
            }
            
            stageLimitText.text = $"{value} / {maxValue}";
            
        }).AddTo(this);

        GameManager.Instance.SeedScore.Subscribe(_ =>
        {
            this.seedInfoText.text = $"{GameManager.Instance.SeedScore} / {this.stageInfo.StageSeedCount}";
        });
    }
    

    public void ChangeStageValue(int _Value)
    {
        // TODO : 감소하거나 증가할 때 Ani 효과? 파티클 효과? 넣어주기
        Debug.Log($"Current Value : {this.curValue.Value}");
        Debug.Log($"{_Value} 만큼 더해짐");
        Debug.Log($"Result Value : {this.curValue.Value}");

        if (this.stageInfo.Type == Define.StageType.LimitTime)
        {
            // SeedValue * 1000을 곱해서 초 단위로 깎기
            _Value *= 1000;
        }
        else if (this.stageInfo.Type == Define.StageType.LimitTry)
        {
            // 도전 기회는 SeedValue가 어떻든 1씩 깎이게 하고 싶어서
            _Value = (_Value > 0 ? _Value / _Value : _Value / (_Value * -1));
        }
        
        this.curValue.Value += _Value;
    }
}
