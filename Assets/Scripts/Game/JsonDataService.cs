using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class JsonDataService : Singleton<JsonDataService>
{
    private const string KEY = "fwZjmNoOBDfDoqwL6uCa1YTtdMrJ022oVG7hB0gEp/I=";
    private const string IV = "k+BF9dKWC8U24dji9lcpKA==";
    
    public bool SaveData<T>(string _RelativePath, T _Data, bool _Encrypted)
    {
        string path = Path.Combine(Application.persistentDataPath, _RelativePath);
        
        try
        {
            if (File.Exists(path))
            {
                Debug.Log("이미 파일이 있음! 여기에 저장 ㄱㄱ");
                //File.Delete(path);
                
                // TODO : 기존 데이터 로드(암호화된 JSON 파일을 읽어서 파일 내용을 복호화하여 기존 데이터를 로드)
                // 필요한 데이터를 기존 데이터에다가 새로 추가합니다.
                // 다시 암호화: 모든 데이터를 다시 암호화하고, 암호화된 JSON 파일을 업데이트
                
                // ...
                if (_Encrypted)
                {
                    // Load 후 복호화하여 새로 추가 뒤 다시 암호화
                }
                else
                {
                    // Data Create, -> Save
                }
            }
            else
            {
                Debug.Log("파일이 없었음. 새로 만들기 시작! 밑에서부터 ㄱㄱ");
            }
            
            
            if (_Encrypted)
            {
                // 암호화하여 저장
                using FileStream stream = File.Create(path);
                WriteEncryptedData(_Data, stream);
                stream.Close();
            }
            else
            {
                var jsonData = JsonConvert.SerializeObject(_Data);
                File.WriteAllText(path, jsonData);
            }

            return true;
        }
        catch (Exception e)
        {
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

    
    public T LoadData<T>(string _RelativePath, bool _Encrypted)
    {
        string path = Path.Combine(Application.persistentDataPath, _RelativePath);

        if (File.Exists(path) == false)
        {
            Debug.LogError($"파일 못찾음 ---> {path}.");
            throw new FileNotFoundException($"{path} does not exist!");
        }

        try
        {
            T data;
            if (_Encrypted)
            {
                data = ReadEncryptedData<T>(path);
            }
            else
            {
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }
            
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError(" ??? ");
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
}
