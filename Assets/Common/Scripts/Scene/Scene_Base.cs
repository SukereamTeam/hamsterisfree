using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Base : MonoBehaviour
{
    public virtual void Test()
    {
        Debug.Log("BaseScene's FunctionToCallInNextScene");
    }
}
