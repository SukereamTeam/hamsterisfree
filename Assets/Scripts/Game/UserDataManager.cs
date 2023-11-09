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
