using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    public Action OnHideComplete;
    private readonly List<(UniTask task, int buttonId, Action onCloseAction)> _closeTaskList = new();
    
    public void AddCloseTask(UniTask task, int id, Action onCloseAction = null) => _closeTaskList.Add((task, id, onCloseAction));

    protected void Show()
    {
        gameObject.SetActive(true);
    }
    
    protected UniTask HideAsync()
    {
        OnHideComplete?.Invoke();
        return default;
    }
}
