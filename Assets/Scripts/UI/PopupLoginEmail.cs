using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoginEmail : PopupBase
{
    [SerializeField] private TextMeshProUGUI _emailText;
    [SerializeField] private TextMeshProUGUI _passwordText;
    [SerializeField] private Button _comfirmButton;
    
    public async UniTask<bool> ShowAsync()
    {
        UniTaskCompletionSource<bool> completionSource = new();
        
        bool loginResult = false;
        
        _comfirmButton.OnClickAsObservable().Subscribe(async _ =>
        {
            if (CheckValidEmail(_emailText.text) == false)
                return;

            if (string.IsNullOrEmpty(_passwordText.text))
                return;
            
            loginResult = await SDKFirebase.Instance.SignInEmail(_emailText.text, _passwordText.text);

            completionSource.TrySetResult(loginResult);
        }).AddTo(this);
        
        Show();
        var result = await completionSource.Task;
        await HideAsync();
        return result;
    }
    
    private bool CheckValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}
