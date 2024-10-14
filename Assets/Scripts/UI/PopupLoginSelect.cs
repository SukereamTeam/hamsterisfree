using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoginSelect : PopupBase
{
    [SerializeField] private Button _confirmButton;
    
    public async UniTask<bool> ShowAsync()
    {
        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
        
        _confirmButton.OnClickAsObservable().Subscribe(async _ =>
        {
            
            
            completionSource.TrySetResult();
        }).AddTo(this);
        
        AddCloseTask(completionSource.Task, 0);
        
        var result = await WaitInputAsync();
        
        await HideAsync();
        
        return result == 0;
    }
}
