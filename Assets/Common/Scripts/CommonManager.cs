using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : GlobalMonoSingleton<CommonManager>
{
    // Lobby 씬에서 선택한 스테이지 Index 저장
    private int curStageIndex = -1;
    public int CurStageIndex
    {
        get => this.curStageIndex;
        set => this.curStageIndex = value;
    }

    public UserData CurUserData { get; private set; }

    private bool isInit = false;
    public void Initialize()
    {
        if (isInit)
            return;

        this.isInit = true;

        // TODO : Delete (유저 정보 로드 구현 후 삭제)
        CurStageIndex = 0;
        
        // TODO : UserData 를 CommonManager 가 아니라 DataContainer 에 넣어두고 쓰기?
        var userData = JsonManager.Instance.LoadData<UserData>();
        if (userData == null)
        {
            UserData newData = new UserData()
            {
                curStage = 0,
                rewardCount = 0
            };

            JsonManager.Instance.SaveData(newData);

            CurUserData = newData;
        }
        else
        {
            CurUserData = userData;
        }
    }

    public void OnDisable()
    {
        DOTween.KillAll(true);
    }
}
