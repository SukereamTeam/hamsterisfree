using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SheetDownloader))]
public class SheetDownloaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Download CSV"))
        {
            (target as SheetDownloader).DownloadCSV().Forget();
        }
    }
}
