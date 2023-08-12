using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : MonoSingleton<CommonManager>
{

    //TODO : DELETE (Test용...  나중엔 저장된 json 으로 읽어올 함수 작성할 것)
    public int CurStageIndex { get; private set; }

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        CurStageIndex = 0;

        // Table Load
        DataContainer.Initialize();
    }

    public void OnDisable()
    {
        DOTween.KillAll(true);
    }
}
