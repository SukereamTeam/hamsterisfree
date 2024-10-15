using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
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
}
