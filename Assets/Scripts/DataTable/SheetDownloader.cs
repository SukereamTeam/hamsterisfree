using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;

public class SheetDownloader : MonoBehaviour
{
    private const string _SHEET_ID = "1fvU3xywFyNZkfHUn1L4PFeJu_uEJMswXxMU7Dl_PcmM";
    private const string _SHEET_NAME = "StageTable";
    private const string _SHEET_PATH = "Assets/Resources/Data/csv";


    public void DownloadCSV()
    {
        StartCoroutine(Download(_SHEET_ID, "csv"));
    }

    private IEnumerator Download(string sheetID, string format)
    {
        var url = $"https://docs.google.com/spreadsheets/d/{sheetID}/export?format={format}&sheet={_SHEET_NAME}";
        
        using (var www = UnityWebRequest.Get(url))
        {
            Debug.Log("Start Downloading...");

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) yield break;

            var fileUrl = $"{_SHEET_PATH}/{_SHEET_NAME}.{format}";
            File.WriteAllText(fileUrl, www.downloadHandler.text + "\n");

            Debug.Log("Download Complete.");
        }
    }
}
