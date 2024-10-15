using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    public UserData CurUserData { get; private set; }
    
    public void LoadUserData()
    {
        var userData = JsonManager.Instance.LoadData<UserData>();
        if (userData == null)
        {
            // TODO : 로컬에 저장된 데이터가 없다면 ,,, 파이어베이스에서 로드해오기 그리고 return
            
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
    
    public void ClearStage(int _CurStage, int _Reward)
    {
        if (_CurStage == CurUserData.curStage)
        {
            CurUserData.curStage++;

            CurUserData.rewardCount += _Reward;
        }

        JsonManager.Instance.SaveData(CurUserData);
    }
}
