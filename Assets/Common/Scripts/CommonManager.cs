using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : GlobalMonoSingleton<CommonManager>
{

    //TODO : DELETE (Test용...  나중엔 저장된 json 으로 읽어올 함수 작성할 것)
    public int CurStageIndex { get; private set; }

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        // TODO : Delete (유저 정보 로드 구현 후 삭제)
        CurStageIndex = 0;

    }

    public void OnDisable()
    {
        DOTween.KillAll(true);
    }
}
