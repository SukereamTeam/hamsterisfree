using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;






public class JsonManager : Singleton<JsonManager>
{
    private const string KEY_PATH = "aes.key";
    private const string FILE_FORMAT = ".json";
    
    public bool SaveData<T>(T _Data, int _Index = 0)
    {
        var fileName = GetJsonFileName<T>();
        
        string path = Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            if (File.Exists(path) == true)
            {
                Debug.Log($"Load {fileName} File!");
                
                var dataList = ReadEncryptedData<Dictionary<int, T>>(path);

                if (dataList != null)
                {
                    dataList[_Index] = _Data;
                    
                    using FileStream stream = File.Open(path, FileMode.Open);
                    WriteEncryptedData(dataList, stream);
                    stream.Close();
                    return true;
                }
                else
                {
                    Debug.Log($"SaveData Error ---> dataList is null. ");
                    return false;
                }
            }
            else
            {
                Debug.Log("json 파일이 없었음. 새로 만들기 시작!");

                var dataList = new Dictionary<int, T>
                {
                    { _Index, _Data }
                };
                
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

        var keyValue = GetKey();

        aesProvider.Key = keyValue;
        
        // 새로 암호화 하기 위해 IV 새로 생성
        aesProvider.GenerateIV();
        
        byte[] iv = aesProvider.IV;

        using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
        using CryptoStream cryptoStream = new CryptoStream(
            _Stream,
            cryptoTransform,
            CryptoStreamMode.Write);
        
        // IV를 먼저 파일에 저장 (데이터를 새로 저장할 때 마다 iv값 덮어 씌움)
        _Stream.Write(iv, 0, iv.Length);

        var jsonData = JsonConvert.SerializeObject(_Data);
        cryptoStream.Write(Encoding.ASCII.GetBytes(jsonData));
    }

    
    public T LoadData<T>(int _Index = 0)
    {
        var fileName = GetJsonFileName<T>();
        
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path) == false)
        {
            Debug.Log($"파일 없음 ---> {path}.");
            
            return default(T);
        }

        try
        {
            var dataList = ReadEncryptedData<Dictionary<int, T>>(path);

            if (dataList.ContainsKey(_Index) == false)
                return default(T);

            return dataList[_Index];
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

        // key 파일 로드해서 읽어오기
        var keyValue = GetKey();

        aesProvider.Key = keyValue;
        
        // IV를 파일에서 읽어옴
        byte[] iv = new byte[aesProvider.BlockSize / 8];
        Array.Copy(fileBytes, 0, iv, 0, iv.Length);
        aesProvider.IV = iv;

        using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(
            aesProvider.Key,
            aesProvider.IV);

        // iv값이 먼저 있고, 그 다음 값이 데이터 부분임. 그래서 iv의 length만큼 뛰어넘고 그 다음 값부터 읽어들이기
        using MemoryStream decryptionStream = new MemoryStream(fileBytes, iv.Length, fileBytes.Length - iv.Length);
        
        using CryptoStream cryptoStream = new CryptoStream(
            decryptionStream,
            cryptoTransform,
            CryptoStreamMode.Read);

        using StreamReader reader = new StreamReader(cryptoStream);

        string result = reader.ReadToEnd();
        
        Debug.Log($"Decrypted result : {result}");
        
        return JsonConvert.DeserializeObject<T>(result);
    }

    private byte[] GetKey()
    {
        string path = Path.Combine(Application.persistentDataPath, KEY_PATH);

        byte[] key = null;
        
        if (File.Exists(path) == true)
        {
            key = File.ReadAllBytes(path);
        }
        else
        {
            using Aes aesProvider = Aes.Create();
            aesProvider.GenerateKey();
                
            key = aesProvider.Key;
                
            File.WriteAllBytes(path, key);
        }

        return key;
    }

    private string GetJsonFileName<T>()
    {
        string name = $"{(typeof(T).ToString())}";

        return name;
    }
}
