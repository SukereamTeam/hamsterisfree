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

        public StageInfoData(Define.StageType _Type, int _LimitValue)
        {
            Type = _Type;
            LimitValue = _LimitValue;
        }
    }
    
    #if UNITY_EDITOR
    [ReadOnlyCustom]
    #endif
    [SerializeField]
    private StageInfoData stageInfo = new StageInfoData();
    public StageInfoData StageInfo => this.stageInfo;

    [SerializeField] private TextMeshProUGUI stageValueText;
    
    private IReactiveProperty<float> curValue = new ReactiveProperty<float>(0f);
    public IReactiveProperty<float> CurValue
    {
        get => this.curValue;
    }
    
    private float timer = 0f;
    
    
    public void SetStage(Define.StageType _Type, int _LimitValue)
    {
        this.stageInfo = new StageInfoData(_Type, _LimitValue);
        
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
                    stageValueText.text = $"{GameManager.Instance.SeedScore}";
                    break;
                case Define.StageType.LimitTime:
                case Define.StageType.LimitTry:
                    stageValueText.text = $"{curValue.Value} / {this.stageInfo.LimitValue}";
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
                Debug.Log($"### Update curValue : {this.curValue.Value}");
                this.curValue.Value -= 0.1f;
                timer = 0f;
            }
        }
        
        
    }

    public void ChangeStageValue(int _Value)
    {
        // TODO : 감소하거나 증가할 때 Ani 효과? 파티클 효과? 넣어주기
        this.curValue.Value += _Value;
    }
}
