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
        var userData = JsonManager.Instance.LoadUserDataWithLocal();
        if (userData == null)
        {
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser == null) return false;
            
            userData = await SDKFirebase.Instance.LoadUserDataWithFirestore(auth.CurrentUser.UserId);
        }

        CurUserData = userData;
        return userData != null;
    }

    public async UniTask<bool> CreateLocalUserData()
    {
        UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
        
        try
        {
            var newData = new UserData { CurrentStage = 0, RewardCount = 0, StageData = new Dictionary<int, StageData>()};
            JsonManager.Instance.SaveLocalData(newData);
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

    public async UniTask<bool> CreateUserData()
    {
	    UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
	    var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	    if (auth.CurrentUser == null)
	    {
		    Debug.Log($"auth.CurrentUser is null");
		    return false;
	    }
        
	    try
	    {
		    var newData = new UserData { CurrentStage = 0, RewardCount = 0, StageData = new Dictionary<int, StageData>()};
		    var data = JsonManager.Instance.EncryptUserDataForFirestore(newData);
		    
		    await SDKFirebase.Instance.SaveUserDataWithFirestore(auth.CurrentUser.UserId, data.key, data.data);
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
    
    public void ClearStage(int currentStage, int reward)
    {
        if (currentStage == CurUserData.CurrentStage)
        {
            CurUserData.CurrentStage++;
            CurUserData.RewardCount += reward;
        }

        // TODO : 수정 필요... 이거 파베 로그인 계정도 쓸거임
        JsonManager.Instance.SaveLocalData(CurUserData);
    }
}
