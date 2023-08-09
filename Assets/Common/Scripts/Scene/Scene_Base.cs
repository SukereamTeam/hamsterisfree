using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Scene_Base : MonoBehaviour
{
    public virtual async UniTask Test()
    {
        Debug.Log("BaseScene's FunctionToCallInNextScene");

        await UniTask.CompletedTask;
    }
}
