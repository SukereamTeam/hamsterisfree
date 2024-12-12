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

        public StageInfoData(Define.StageType type, int limitValue, int stageSeedCount)
        {
            Type = type;
            if (type == Define.StageType.LimitTime)
            {
                // 주어진 초 단위 시간(60)을 밀리초로 변환
                limitValue *= 1000;
            }
            LimitValue = limitValue;
            StageSeedCount = stageSeedCount;
        }
    }
    
    [SerializeField]
    private StageInfoData stageInfo = new StageInfoData();
    public StageInfoData StageInfo => stageInfo;

    [SerializeField]
    private TextMeshProUGUI stageLimitText;
    
    [SerializeField]
    private TextMeshProUGUI stageNumberText;
    
    [SerializeField]
    private TextMeshProUGUI seedInfoText;
    
    // LimitTime타입의 스테이지 일 때의 time값 혹은 LimitTry타입의 스테이지 일 때의 try값
    // default 일 땐 표시하지 않는다.
    private IReactiveProperty<float> _curValue = new ReactiveProperty<float>(0f);
    
    private float _timer = 0f;


    private void Update()
    {
        if (GameManager.Instance.IsGame.Value == false)
            return;

        if (stageInfo.Type == Define.StageType.LimitTime)
        {
            _timer += Time.deltaTime;
            if (_timer >= 0.1f)
            {
                // 1초에 1씩 깎는 것
                _curValue.Value -= 100f;
                _timer -= 0.1f;
            }
        }
    }

    public void SetStage(int stageNumber, Define.StageType type, int limitValue, int stageSeedCount)
    {
        stageInfo = new StageInfoData(type, limitValue, stageSeedCount);
        
        // Stage 번호 출력
        stageNumberText.text = stageNumber.ToString();
        
        if (stageInfo.Type == Define.StageType.LimitTime ||
            stageInfo.Type == Define.StageType.LimitTry)
        {
            _curValue.Value = stageInfo.LimitValue;
            
            _curValue
                .Skip(TimeSpan.Zero)  // 첫 프레임 호출 스킵
                .Where(x => x <= 0f)
                .Subscribe(_ =>
                {
                    // 시간이 다 되었거나, 기회를 다 잃었다면 게임 종료
                    GameManager.Instance.IsGame.Value = false;
                }).AddTo(this);
        }
        
        _curValue.Subscribe(newValue =>
        {
            var value = _curValue.Value;
            var maxValue = stageInfo.LimitValue;
            if (stageInfo.Type == Define.StageType.LimitTime)
            {
                // 밀리초 -> 초 변환해서 출력
                value = Mathf.Round(value /= 1000);
                maxValue /= 1000;
            }
            
            stageLimitText.text = $"{value} / {maxValue}";
            
        }).AddTo(this);

        GameManager.Instance.SeedScore.Subscribe(_ =>
        {
            seedInfoText.text = $"{GameManager.Instance.SeedScore} / {stageInfo.StageSeedCount}";
        });
    }
    

    public void ChangeStageValue(int value)
    {
        // TODO : 감소하거나 증가할 때 Ani 효과? 파티클 효과? 넣어주기
        Debug.Log($"Current Value : {_curValue.Value}");
        Debug.Log($"{value} 만큼 더해짐");
        Debug.Log($"Result Value : {_curValue.Value}");

        if (stageInfo.Type == Define.StageType.LimitTime)
        {
            // SeedValue * 1000을 곱해서 초 단위로 깎기
            value *= 1000;
        }
        else if (stageInfo.Type == Define.StageType.LimitTry)
        {
            // 도전 기회는 SeedValue가 어떻든 1씩 깎이게 하고 싶어서
            value = (value > 0 ? value / value : value / (value * -1));
        }
        
        _curValue.Value += value;
    }
}
