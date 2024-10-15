using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : GlobalMonoSingleton<CommonManager>
{
    public static PopupManager Popup { private set; get; } = new();
    
    // Lobby 씬에서 선택한 스테이지 Index를 저장
    private int curStageIndex = -1;
    public int CurStageIndex
    {
        get => this.curStageIndex;
        set => this.curStageIndex = value;
    }
    
    private bool isInit = false;
    
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        CurStageIndex = 0;
    }

    public void OnDisable()
    {
        DOTween.KillAll(true);
    }
}
