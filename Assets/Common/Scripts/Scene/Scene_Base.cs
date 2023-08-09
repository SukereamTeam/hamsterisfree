using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Scene_Base : MonoBehaviour
{
    public virtual async UniTask LoadDatas()
    {
        Debug.Log("### Scene_Base LoadDatas ###");

        await UniTask.CompletedTask;
    }
}
