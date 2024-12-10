using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;



[RequireComponent(typeof(UnityEngine.UI.LoopScrollRect))]
[DisallowMultipleComponent]
public class InitScrollLobby : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    public GameObject item;
    public int totalCount = -1;

    // Implement your own Cache Pool here. The following is just for example.
    Stack<Transform> pool = new Stack<Transform>();
    public GameObject GetObject(int index)
    {
        if (pool.Count == 0)
        {
            return Instantiate(item);
        }
        Transform candidate = pool.Pop();
        candidate.gameObject.SetActive(true);
        return candidate.gameObject;
    }

    public void ReturnObject(Transform trans)
    {
        // Use `DestroyImmediate` here if you don't need Pool
        trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
        trans.gameObject.SetActive(false);
        trans.SetParent(transform, false);
        pool.Push(trans);
    }

    public void ProvideData(Transform transform, int idx)
    {
        transform.SendMessage("ScrollCellIndex", idx);
    }

    public void Initialize(int _StageCount)
    {
        Debug.Log("# InitOnStart Start #");

        var loopScrollRect = this.GetComponent<LoopScrollRect>();

        var curIdx = UserDataManager.Instance.CurUserData.CurrentStage - 2;
        if (curIdx < 0)
        {
            curIdx = 0;
        }

        if (loopScrollRect != null)
        {
            loopScrollRect.prefabSource = this;
            loopScrollRect.dataSource = this;
            loopScrollRect.totalCount = _StageCount;
            loopScrollRect.RefillCells(curIdx);
        }
        else
        {
            Debug.Log("# LoopScrollRect is Null. #");
        }
    }
}
