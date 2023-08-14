using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stage_Entity))]
public class Stage_ScriptableEditor : Editor
{
    [SerializeField]
    private string assetPath = "Assets/Resources/Data/so/StageTable.asset";

    [SerializeField]
    private Stage_Entity asset;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        
        if (GUILayout.Button("Update"))
        {
            

            Stage_Entity data = (Stage_Entity)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Stage_Entity));

            if (data != null)
            {
                this.asset = data;
            }
        }
    }
}
