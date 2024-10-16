using System;
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
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    signInTask.User.DisplayName, signInTask.User.UserId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError("SignInEmail error: " + ex.Message);
            return false;
        }
    }
    
    public void LoadUserDataWithFirestore(string userId)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                var dataList = snapshot.ToDictionary(); //ReadEncryptedData<Dictionary<int, T>>(path);
                
                string jsonData = snapshot.ToDictionary()["UserData"].ToString();
            }
            else
            {
                Debug.LogError("데이터 로드 실패 또는 데이터가 없음: " + task.Exception);
            }
        });
    }

    public void SaveUserDataWithFirestore()
    {
        // TODO
        // key 값과 암호화된 데이터를 저장해야 함
        // Dictionary<string, object> userData = new()
        //{
        //    { "Key", "~" },
        //    { "Data", "~" }
        //} 이렇게
        // 그래서 load 할 땐 Key 값으로 Data를 복호화해서 가져온다.
    }
}
