using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.Threading;
using System.IO;
using System;
using Unity.Collections;

public class SheetDownloader : MonoBehaviour
{
    public enum SheetName
    {
        StageTable,
        SeedTable
    }

    [Serializable]
    public class SheetData
    {
        public string SheetName;
        public string SheetId;
        
        public SheetData(string _Id, string _Name)
        {
            SheetId = _Id;
            SheetName = _Name;
        }
    }


    private const string CSV_PATH = "Assets/Resources/Data/csv";
    private const string FILE_FORMAT = "csv";

    [ReadOnlyCustom]
    [SerializeField]
    private SheetData[] sheetDatas;




    public async UniTaskVoid DownloadAll(Action _Oncomplete = null)
    {
        foreach(var sheet in sheetDatas)
        {
            await Download(sheet, FILE_FORMAT);
        }

        if (_Oncomplete != null)
        {
            _Oncomplete();
        }

        await UniTask.Yield();

        foreach(var sheet in sheetDatas)
        {
            await CreateScriptableObject(sheet);
        }
    }

    private async UniTask Download(SheetData _SheetData, string _Format)
    {
        var url = $"https://docs.google.com/spreadsheets/d/{_SheetData.SheetId}/export?format={_Format}&sheet={_SheetData.SheetName}";

        using (var www = UnityWebRequest.Get(url))
        {
            Debug.Log("Start DataTable CSV Downloading");

            try
            {
                await www.SendWebRequest();
            }
            catch (Exception ex)
            {
                Debug.LogError($"### exception occurred: {ex}");
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"### Failed Download CSV {_SheetData.SheetName} ###");
                return;
            }

            var fileUrl = $"{CSV_PATH}/{_SheetData.SheetName}.{_Format}";

            await UniTask.SwitchToMainThread();
            // 비동기 작업을 메인 스레드에서 실행되도록 전환해주는 함수
            // UnityWebRequest 는 백그라운드 스레드에서 실행될 수 있으나
            // UI 업뎃이나 파일 작업은 메인 스레드에서 수행할 수 있음
            // 그래서 백그라운드 스레드에서 작업을 마치고 결과를 메인 스레드로 전환하여 이후 작업을 수행하도록 함


            await File.WriteAllTextAsync(fileUrl, www.downloadHandler.text + "\n", new CancellationToken());

            Debug.Log("Download Complete.");
        }
    }

    private async UniTask CreateScriptableObject(SheetData _SheetData)
    {
        
    }
}
