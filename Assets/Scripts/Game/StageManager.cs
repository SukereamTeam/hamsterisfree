using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

public class StageManager : MonoBehaviour
{
    [Serializable]
    public struct StageInfoData
    {
        public Define.StageType StageType;
        public int StageLimitValue;

        public StageInfoData(Define.StageType _Type, int _LimitValue)
        {
            StageType = _Type;
            StageLimitValue = _LimitValue;
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
}
