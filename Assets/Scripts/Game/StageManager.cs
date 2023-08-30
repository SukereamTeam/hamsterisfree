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
            LimitValue = _LimitValue;
            StageSeedCount = _StageSeedCount;
        }
    }
    
    #if UNITY_EDITOR
    [ReadOnlyCustom]
    #endif
    [SerializeField]
    private StageInfoData stageInfo = new StageInfoData();
    public StageInfoData StageInfo => this.stageInfo;

    [SerializeField] private TextMeshProUGUI stageLimitText;
    [SerializeField] private TextMeshProUGUI stageNumberText;
    [SerializeField] private TextMeshProUGUI seedInfoText;
    
    // LimitTime타입의 스테이지 일 때의 time값 혹은 LimitTry타입의 스테이지 일 때의 try값
    // default 일 땐 표시하지 않는다.
    private IReactiveProperty<float> curValue = new ReactiveProperty<float>(0f);
    public IReactiveProperty<float> CurValue
    {
        get => this.curValue;
    }
    
    private float timer = 0f;
    
    
    public void SetStage(int _StageNumber, Define.StageType _Type, int _LimitValue, int _StageSeedCount)
    {
        this.stageInfo = new StageInfoData(_Type, _LimitValue, _StageSeedCount);
        
        this.seedInfoText.text = $"{GameManager.Instance.SeedScore}/{this.stageInfo.StageSeedCount}";
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
                    // 시간이 다 되었거나, 기회를 다 잃었다면
                    GameManager.Instance.IsGame.Value = false;
                }).AddTo(this);
        }
        
        this.curValue.Subscribe(newValue =>
        {
            switch (this.stageInfo.Type)
            {
                // TODO : 모든 스테이지는 SeedScore를 표시해줘야 함.
                case Define.StageType.Default:
                    stageLimitText.text = $"{GameManager.Instance.SeedScore}";
                    break;
                case Define.StageType.LimitTime:
                case Define.StageType.LimitTry:
                    stageLimitText.text = $"{curValue.Value} / {this.stageInfo.LimitValue}";
                    break;
            }
        }).AddTo(this);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGame.Value == false)
            return;

        if (this.stageInfo.Type == Define.StageType.LimitTime)
        {
            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                //Debug.Log($"### Update curValue : {this.curValue.Value}");
                this.curValue.Value -= 0.1f;
                timer = 0f;
            }
        }

        this.seedInfoText.text = $"{GameManager.Instance.SeedScore}/{this.stageInfo.StageSeedCount}";
    }

    public void ChangeStageValue(int _Value)
    {
        // TODO : 감소하거나 증가할 때 Ani 효과? 파티클 효과? 넣어주기
        this.curValue.Value += _Value;
    }
}
