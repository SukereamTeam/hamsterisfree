using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestScriptable))]
public class TestScriptableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (TestScriptable)target;

        if (GUILayout.Button("Add to Counter", GUILayout.Height(40)))
        {
            script.UpdateCounter();
        }

    }
}