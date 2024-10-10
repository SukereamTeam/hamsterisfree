using System;
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
}
