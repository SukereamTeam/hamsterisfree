using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoginSelect : PopupBase
{
    [SerializeField] private Button _guestLogin;
    [SerializeField] private Button _emailLogin;
    
    public async UniTask<bool> ShowAsync()
    {
        UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();

        bool loginResult = false;
        
        _guestLogin.OnClickAsObservable().Subscribe(async _ =>
        {
            loginResult = await SignInGuest();  //await SDKFirebase.Instance.SignInAnonymously();

            completionSource.TrySetResult(loginResult);
        }).AddTo(this);
        
        _emailLogin.OnClickAsObservable().Subscribe(async _ =>
        {
            var popupEmail = await CommonManager.Popup.CreateAsync<PopupLoginEmail>();
            loginResult = await popupEmail.ShowAsync();
            
            completionSource.TrySetResult(loginResult);
        }).AddTo(this);
        
        Show();
        var result = await completionSource.Task;
        await HideAsync();
        return result;
    }

    private async UniTask<bool> SignInGuest()
    {
        return await UserDataManager.Instance.CreateUserData();
    }
}
