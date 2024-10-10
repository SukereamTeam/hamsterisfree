using UnityEngine;
using Cysharp.Threading.Tasks;

public class SDKFirebaseAuth : MonoBehaviour
{
    public async UniTask SignInAnonymously()
    {
        var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        await auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
    
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
        
        await UniTask.CompletedTask;
    }
    
    // public async UniTask<bool> SignInAnonymously()
    // {
    //     try
    //     {
    //         var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    //         var signInTask = await auth.SignInAnonymouslyAsync();
    //         if (signInTask != null)
    //         {
    //             Debug.LogFormat("User signed in successfully: {0} ({1})",
    //                 signInTask.User.DisplayName, signInTask.User.UserId);
    //             return true;
    //         }
    //         return false;
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError("SignInAnonymouslyAsync encountered an error: " + ex.Message);
    //         return false;
    //     }
    // }
}
