using System.Collections;
using System.Collections.Generic;
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
            loginResult = await SDKFirebase.Instance.SignInAnonymously();

            completionSource.TrySetResult(loginResult);
        }).AddTo(this);
        
        //_emailLogin.OnClickAsObservable().Subscribe(async _ =>
        //{
            // TODO : email 로그인 팝업 
            //var loginPopup = await CommonManager.Popup.CreateAsync<PopupLoginSelect>();
            //var result = await loginPopup.ShowAsync();
            
            //completionSource.TrySetResult();
        //}).AddTo(this);
        
        Show();
        var result = await completionSource.Task;
        await HideAsync();
        return result;
    }
}
