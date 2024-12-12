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
	    UserData userData = null;
	    var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	    if (auth.CurrentUser == null)
	    {
		    userData = JsonManager.Instance.LoadUserDataWithLocal();
	    }
        else
        {
            userData = await SDKFirebase.Instance.LoadUserDataWithFirestore(auth.CurrentUser.UserId);
        }

        CurUserData = userData;
        return userData != null;
    }

    public async UniTask<bool> CreateUserDataForLocal()
    {
        UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
        
        var newData = new UserData { CurrentStage = 0, RewardCount = 0, StageData = new Dictionary<int, StageData>()};
        var isSuccess = JsonManager.Instance.SaveLocalData(newData);
        if (isSuccess)
        {
	        CurUserData = newData;
	        completionSource.TrySetResult(true);
        }
        else
	        completionSource.TrySetResult(false);

        return await completionSource.Task;
    }

    public async UniTask<bool> CreateUserDataForFirestore()
    {
	    UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
	    var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	    if (auth.CurrentUser == null)
	    {
		    Debug.Log($"auth.CurrentUser is null");
		    return false;
	    }
	    
	    var newData = new UserData { CurrentStage = 0, RewardCount = 0, StageData = new Dictionary<int, StageData>()};
	    var data = JsonManager.Instance.EncryptUserDataForFirestore(newData);
	    if (data.key == null || data.data == null)
	    {
		    Debug.Log($"CreateUserData Error / data is null.");
		    completionSource.TrySetResult(false);
	    }
	    else
	    {
		    var isSuccess = await SDKFirebase.Instance.SaveUserDataWithFirestore(auth.CurrentUser.UserId, data.key, data.data);
		    if (isSuccess)
		    {
			    CurUserData = newData;
			    completionSource.TrySetResult(true);
		    }
		    else
			    completionSource.TrySetResult(false);
	    }
	    
	    return await completionSource.Task;
    }

    public async UniTask<bool> SaveUserDataForFirestore(UserData userData)
    {
	    UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
	    var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	    if (auth.CurrentUser == null)
	    {
		    Debug.Log($"auth.CurrentUser is null");
		    return false;
	    }
	    
	    var data = JsonManager.Instance.EncryptUserDataForFirestore(userData);
	    if (data.key == null || data.data == null)
	    {
		    Debug.Log($"CreateUserData Error / data is null.");
		    completionSource.TrySetResult(false);
	    }
	    else
	    {
		    var isSuccess = await SDKFirebase.Instance.SaveUserDataWithFirestore(auth.CurrentUser.UserId, data.key, data.data);
		    if (isSuccess)
		    {
			    CurUserData = userData;
			    completionSource.TrySetResult(true);
		    }
		    else
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
