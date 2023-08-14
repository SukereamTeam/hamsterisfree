using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.Threading;
using System.IO;
using System;

public class SheetDownloader : MonoBehaviour
{
    private const string _SHEET_ID = "1fvU3xywFyNZkfHUn1L4PFeJu_uEJMswXxMU7Dl_PcmM";
    private const string _SHEET_NAME = "StageTable";
    private const string _SHEET_PATH = "Assets/Resources/Data/csv";


    public async UniTaskVoid DownloadCSV(Action _OnComplete = null)
    {
        await Download(_SHEET_ID, "csv");

        if (_OnComplete != null)
            _OnComplete();
    }

    private async UniTask Download(string sheetID, string format)
    {
        var url = $"https://docs.google.com/spreadsheets/d/{sheetID}/export?format={format}&sheet={_SHEET_NAME}";
        

        using (var www = UnityWebRequest.Get(url))
        {
            Debug.Log("### Start DataTable CSV Downloading ###");

            try
            {
                await www.SendWebRequest();
            }
            catch(Exception ex)
            {
                Debug.LogError($"### exception occurred: {ex}");
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                return;
            }

            var fileUrl = $"{_SHEET_PATH}/{_SHEET_NAME}.{format}";

            await UniTask.SwitchToMainThread();
            // 비동기 작업을 메인 스레드에서 실행되도록 전환해주는 함수
            // UnityWebRequest 는 백그라운드 스레드에서 실행될 수 있으나
            // UI 업뎃이나 파일 작업은 메인 스레드에서 수행할 수 있음
            // 그래서 백그라운드 스레드에서 작업을 마치고 결과를 메인 스레드로 전환하여 이후 작업을 수행하도록 함


            await File.WriteAllTextAsync(fileUrl, www.downloadHandler.text + "\n", new CancellationToken());

            Debug.Log("Download Complete.");
        }
    }
}
