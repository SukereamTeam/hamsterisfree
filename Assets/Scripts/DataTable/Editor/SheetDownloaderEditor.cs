using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataTable;

[CustomEditor(typeof(SheetDownloader))]
public class SheetDownloaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var sheetDownloader = (SheetDownloader)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Download All CSV"))
        {
            sheetDownloader.DownloadAll(() =>
            {
                AssetDatabase.Refresh();
            }).Forget();

            
        }

        if (GUILayout.Button("Download CSV"))
        {
            //sheetDownloader.DownloadCSV().Forget();
        }

        
    }
}
