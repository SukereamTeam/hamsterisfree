using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;



[Serializable]
public struct TileData
{
    public string SubType;
    public int SubTypeIndex;
    public int RootIdx;

    public TileData(string _SubType, int _SubTypeIndex, int _RootIdx)
    {
        SubType = _SubType;
        SubTypeIndex = _SubTypeIndex;
        RootIdx = _RootIdx;
    }
}

[Serializable]
public class StageData
{
    public List<TileData> seedDatas;
    public List<TileData> monsterDatas;
    public int exitDataRootIdx;

    public StageData(List<TileData> _Seed, List<TileData> _Monster, int _Exit)
    {
        seedDatas = _Seed;
        monsterDatas = _Monster;
        exitDataRootIdx = _Exit;
    }
}

[Serializable]
public class UserData
{
    public int curStage;
    public int rewardCount;
}

public class JsonManager : Singleton<JsonManager>
{
    private const string KEY = "fwZjmNoOBDfDoqwL6uCa1YTtdMrJ022oVG7hB0gEp/I=";
    private const string IV = "k+BF9dKWC8U24dji9lcpKA==";
    
    
    public bool SaveData<T>(T _Data)
    {
        var relativePath = GetJsonFileName<T>();

        if (relativePath == string.Empty)
        {
            Debug.Log($"Not Found {nameof(T)} match name");
            return false;
        }
        
        string path = Path.Combine(Application.persistentDataPath, relativePath);
        
        try
        {
            if (File.Exists(path) == true)
            {
                Debug.Log("이미 파일이 있음! 여기에 저장 ㄱㄱ");
                
                // TODO : 기존 데이터 로드(암호화된 JSON 파일을 읽어서 파일 내용을 복호화하여 기존 데이터를 로드)
                // 필요한 데이터를 기존 데이터에다가 새로 추가합니다.
                // 다시 암호화: 모든 데이터를 다시 암호화하고, 암호화된 JSON 파일을 업데이트
                
                var data = ReadEncryptedData<List<T>>(path);

                if (data != null)
                {
                    data.Add(_Data);
                    
                    using (FileStream stream = File.Open(path, FileMode.Open))
                    {
                        WriteEncryptedData(data, stream);
                    }
                
                    return true;
                }
                else
                {
                    Debug.Log($"SaveData Error ---> data is null. ");
                    return false;
                }
            }
            else
            {
                Debug.Log("파일이 없었음. 새로 만들기 시작!");

                var dataList = new List<T> { _Data };
                
                // 암호화하여 저장
                using FileStream stream = File.Create(path);
                WriteEncryptedData(dataList, stream);
                stream.Close();

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"SaveData Error ---> {e.Message} / {e.StackTrace} ");
            return false;
        }
    }


    private void WriteEncryptedData<T>(T _Data, FileStream _Stream)
    {
        using Aes aesProvider = Aes.Create();
        aesProvider.Key = Convert.FromBase64String(KEY);
        aesProvider.IV = Convert.FromBase64String(IV);

        using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
        using CryptoStream cryptoStream = new CryptoStream(
            _Stream,
            cryptoTransform,
            CryptoStreamMode.Write);

        var jsonData = JsonConvert.SerializeObject(_Data);
        cryptoStream.Write(Encoding.ASCII.GetBytes(jsonData));
    }

    
    public T LoadData<T>(int _Index = 0)
    {
        var relativePath = GetJsonFileName<T>();

        if (relativePath == string.Empty)
        {
            throw new NullReferenceException();
        }
        
        string path = Path.Combine(Application.persistentDataPath, relativePath);

        if (File.Exists(path) == false)
        {
            Debug.Log($"파일 없음 ---> {path}.");
            
            return default(T);
        }

        try
        {
            var data = ReadEncryptedData<List<T>>(path);
            
            return data[_Index];
        }
        catch (Exception e)
        {
            Debug.LogError($" LoadData Error ---> {e.Message} / {e.StackTrace}");
            throw e;
        }
    }

    private T ReadEncryptedData<T>(string _Path)
    {
        byte[] fileBytes = File.ReadAllBytes(_Path);

        using Aes aesProvider = Aes.Create();
        aesProvider.Key = Convert.FromBase64String(KEY);
        aesProvider.IV = Convert.FromBase64String(IV);

        using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(
            aesProvider.Key,
            aesProvider.IV);

        using MemoryStream decryptionStream = new MemoryStream(fileBytes);
        using CryptoStream cryptoStream = new CryptoStream(
            decryptionStream,
            cryptoTransform,
            CryptoStreamMode.Read);

        using StreamReader reader = new StreamReader(cryptoStream);

        string result = reader.ReadToEnd();
        
        Debug.Log($"Decrypted result : {result}");
        
        return JsonConvert.DeserializeObject<T>(result);
    }

    private string GetJsonFileName<T>()
    {
        string name = String.Empty;
        if (typeof(T) == typeof(StageData))
        {
            name = "StageData.json";
        }

        return name;
    }
}
