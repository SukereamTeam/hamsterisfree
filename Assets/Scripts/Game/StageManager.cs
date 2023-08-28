using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Serialization;

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

    

    public void SetStage(Define.StageType _Type, int _LimitValue)
    {
        this.stageInfo = new StageInfoData(_Type, _LimitValue);
    }

    public void ChangeStageValue(int _Value)
    {
        this.stageInfo.LimitValue += _Value;
    }
}
