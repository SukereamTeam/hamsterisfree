using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    public UserData CurUserData { get; private set; }
    
    public async UniTask<bool> LoadUserData()
    {
        // 먼저 로컬에 저장된 데이터가 있는지 확인
        
        // TODO : 로컬에 key도 저장해두기? ...
        
        
        var userData = JsonManager.Instance.LoadData<UserData>();
        if (userData == null)
        {
            // TODO : 로컬에 저장된 데이터가 없다면 ,,, 파이어베이스에서 로드해오기 그리고 return
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                return await SDKFirebase.Instance.LoadUserDataWithFirestore(auth.CurrentUser.UserId);
            }
            else
                return false;
        }
        else
        {
            CurUserData = userData;
            return true;
        }

        return false;
    }

    public async UniTask<bool> CreateUserData()
    {
        UniTaskCompletionSource<bool> completionSource = new();
        
        try
        {
            UserData newData = new()
            {
                curStage = 0,
                rewardCount = 0
            };

            JsonManager.Instance.SaveData(newData);

            CurUserData = newData;

            completionSource.TrySetResult(true);
        }
        catch (Exception e)
        {
            Debug.Log($"CreateUserData Error ---> {e.Message} / {e.StackTrace} ");
            completionSource.TrySetResult(false);
        }

        return await completionSource.Task;
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
