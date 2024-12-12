using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class SDKFirebase : GlobalMonoSingleton<SDKFirebase>
{
    public bool IsInitialized { private set; get; }
    private FirebaseApp _app;
    
    public void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("### Firebase Initialized. ###");
                IsInitialized = true;
                _app = FirebaseApp.DefaultInstance;
                
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertySignUpMethod,
#if UNITY_ANDROID
                    "Google"
#else
                            "Apple"
#endif
                );
                
                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
                FirebaseApp.LogLevel = LogLevel.Debug;
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }
    
    public async UniTask<bool> SignInAnonymously()
    {
        try
        {
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            var signInTask = await auth.SignInAnonymouslyAsync();
            if (signInTask != null)
            {
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    signInTask.User.DisplayName, signInTask.User.UserId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError("SignInAnonymouslyAsync error: " + ex.Message);
            return false;
        }
    }

    public async UniTask<bool> SignInEmail(string email, string password)
    {
        try
        {
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            var signInTask = await auth.SignInWithEmailAndPasswordAsync(email, password);
            if (signInTask != null)
            {
                Debug.Log($"User signed in successfully ---> DisplayName: {signInTask.User.DisplayName} / UserId: ({signInTask.User.UserId})");

                return await UserDataManager.Instance.CreateUserDataForFirestore();
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError("SignInEmail error: " + ex.Message);
            return false;
        }
    }
    
    public async UniTask<UserData> LoadUserDataWithFirestore(string userId)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(userId);
        UniTaskCompletionSource<UserData> completionSource = new UniTaskCompletionSource<UserData>();

        await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                var dataList = snapshot.ToDictionary();
                
                if (dataList.TryGetValue("UserDataKey", out var key) &&
                    dataList.TryGetValue("UserData", out var data))
                {
	                var cryptoKey = JsonManager.Instance.SetCryptoKey(key);
	                if (cryptoKey == null)
	                {
		                completionSource.TrySetResult(null);
	                }

	                var userData = JsonManager.Instance.LoadUserDataWithFirestore(data);
	                completionSource.TrySetResult(userData);
                }
                else
                {
	                completionSource.TrySetResult(null);
                }
            }
            else
            {
                Debug.LogError(": " + task.Exception);
                completionSource.TrySetResult(null);
            }
        });

        return await completionSource.Task;
    }

    public async UniTask<bool> SaveUserDataWithFirestore(string userId, byte[] key, byte[] data)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(userId);
        UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();

        var saveData = new Dictionary<string, object>
        {
	        { "UserDataKey", key },
	        { "UserData", data }
        };
        
        try
        {
	        await docRef.SetAsync(saveData).ContinueWithOnMainThread(task =>
	        {
		        if (task.IsCompleted)
		        {
			        if (task.Exception == null)
			        {
				        Debug.Log("UserData saved.");
				        completionSource.TrySetResult(true);
			        }
			        else
			        {
				        Debug.LogError("Firestore save error. : " + task.Exception.Message);
				        completionSource.TrySetResult(false);
			        }
		        }
		        else
		        {
			        Debug.LogError("Firestore save error.");
			        completionSource.TrySetResult(false);
		        }
	        });
        }
        catch (Exception ex)
        {
	        Debug.LogError("Exception in SaveUserDataWithFirestore: " + ex.Message);
	        completionSource.TrySetResult(false);
        }

        return await completionSource.Task;
    }
}
