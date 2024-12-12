using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class JsonManager : Singleton<JsonManager>
{
    private const string KEY_PATH = "aes.key";

    private byte[] _key = null;

    public async UniTask SaveStageData(StageData data, int stageIndex)
    {
	    var curUserData = UserDataManager.Instance.CurUserData;

	    if (curUserData.StageData.TryAdd(stageIndex, data) == false)
	    {
		    curUserData.StageData[stageIndex] = data;
	    }
	    
	    var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	    if (auth.CurrentUser == null)
	    {
		    SaveLocalData(curUserData);
	    }
	    else
	    {
		    var encryptData = EncryptUserDataForFirestore(curUserData);
		    if (encryptData.key == null || encryptData.data == null)
		    {
			    Debug.Log($"CreateUserData Error / data is null.");
			    return;
		    }
		    
		    var isSuccess = await SDKFirebase.Instance.SaveUserDataWithFirestore(auth.CurrentUser.UserId, encryptData.key, encryptData.data);
		    if (isSuccess == false)
			    Debug.Log($"SaveStageData -> SaveUserDataWithFirestore Error.");
	    }
    }

    public bool SaveLocalData(UserData data)
    {
	    string path = Path.Combine(Application.persistentDataPath, $"{(typeof(UserData))}");
	    try
	    {
		    if (File.Exists(path) == true)
		    {
			    Debug.Log($"Load {(typeof(UserData))} File!");
			    var savedData = ReadEncryptedData(path);

			    if (savedData != null)
			    {
				    savedData = data;
                    
				    using FileStream stream = File.Open(path, FileMode.Open);
				    WriteEncryptedData(savedData, stream);
				    stream.Close();
				    return true;
			    }

			    Debug.Log($"SaveData Error ---> dataList is null. ");
			    return false;
		    }
		    else
		    {
			    Debug.Log("Create UserData");
			    using FileStream stream = File.Create(path);
			    WriteEncryptedData(data, stream);
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
    
    public byte[] SetCryptoKey(object keyValue = null)
    {
	    if (_key != null)
		    return _key;

	    if (keyValue == null)
	    {
		    // 로컬 데이터에서 Crypto 세팅
		    string keyPath = Path.Combine(Application.persistentDataPath, KEY_PATH);
		    if (File.Exists(keyPath))
		    {
			    var key = File.ReadAllBytes(keyPath);
			    return key;
		    }
		    else
		    {
			    using Aes aesProvider = Aes.Create();
			    aesProvider.GenerateKey();
                
			    var key = aesProvider.Key;
			    File.WriteAllBytes(keyPath, key);

			    return key;
		    }
	    }
	    
	    if (keyValue is string keyToBytes)
	    {
		    if (string.IsNullOrEmpty(keyToBytes) == false)
		    {
			    _key = Convert.FromBase64String(keyToBytes);
			    return _key;
		    }
	    }

	    return null;
    }
    
    public UserData LoadUserDataWithLocal()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{(typeof(UserData))}");

        if (File.Exists(path) == false)
        {
            Debug.Log($"### Not exist ---> {path}");
            return null;
        }
        
        var savedData = ReadEncryptedData(path);
        return savedData;
    }

    public UserData LoadUserDataWithFirestore(object data)
    {
	    if (_key == null)
		    return null;
	    
	    if (data is string base64String)
	    {
		    var convertData = Convert.FromBase64String(base64String);
		    
		    using Aes aesProvider = Aes.Create();
		    aesProvider.Key = _key;
        
		    // IV를 파일에서 읽어옴
		    byte[] iv = new byte[aesProvider.BlockSize / 8];
		    Array.Copy(convertData, 0, iv, 0, iv.Length);
		    aesProvider.IV = iv;

		    using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(
			    aesProvider.Key,
			    aesProvider.IV);

		    // iv값이 먼저 있고, 그 다음 값이 데이터 부분임. 그래서 iv의 length만큼 뛰어넘고 그 다음 값부터 읽어들이기
		    using MemoryStream decryptionStream = new MemoryStream(convertData, iv.Length, convertData.Length - iv.Length);
        
		    using CryptoStream cryptoStream = new CryptoStream(
			    decryptionStream,
			    cryptoTransform,
			    CryptoStreamMode.Read);

		    using StreamReader reader = new StreamReader(cryptoStream);

		    string result = reader.ReadToEnd();
		    return JsonConvert.DeserializeObject<UserData>(result);
	    }

	    return null;
    }

    public (byte[] key, byte[] data) EncryptUserDataForFirestore(UserData data)
    {
	    var encryptData = EncryptDataToBytes(data);
	    
	    return (_key, encryptData);
    }
    
    private byte[] EncryptDataToBytes(UserData data)
    {
	    _key ??= SetCryptoKey();

	    if (_key == null)
		    return null;

	    using Aes aesProvider = Aes.Create();
	    aesProvider.Key = _key;
	    aesProvider.GenerateIV();
	    byte[] iv = aesProvider.IV;

	    using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
	    using MemoryStream memoryStream = new MemoryStream();
	    using CryptoStream cryptoStream = new CryptoStream(
		    memoryStream,
		    cryptoTransform,
		    CryptoStreamMode.Write);
	    
	    memoryStream.Write(iv, 0, iv.Length);
	    
	    var jsonData = JsonConvert.SerializeObject(data);
	    cryptoStream.Write(Encoding.ASCII.GetBytes(jsonData));
	    cryptoStream.FlushFinalBlock();
	    
	    return memoryStream.ToArray();
    }
    
    private void WriteEncryptedData(UserData data, FileStream stream)
    {
	    _key ??= SetCryptoKey();

	    if (_key == null)
		    return;
	    
	    using Aes aesProvider = Aes.Create();
	    aesProvider.Key = _key;
        
	    // 새로 암호화 하기 위해 IV 새로 생성
	    aesProvider.GenerateIV();
        
	    byte[] iv = aesProvider.IV;

	    using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
	    using CryptoStream cryptoStream = new CryptoStream(
		    stream,
		    cryptoTransform,
		    CryptoStreamMode.Write);
        
	    // IV를 먼저 파일에 저장 (데이터를 새로 저장할 때 마다 iv값 덮어 씌움)
	    stream.Write(iv, 0, iv.Length);

	    var jsonData = JsonConvert.SerializeObject(data);
	    cryptoStream.Write(Encoding.ASCII.GetBytes(jsonData));
	    cryptoStream.FlushFinalBlock();
    }
    
    private UserData ReadEncryptedData(string path)
    {
	    _key ??= SetCryptoKey();

	    if (_key == null)
		    return null;
	    
	    byte[] fileBytes = File.ReadAllBytes(path);

	    using Aes aesProvider = Aes.Create();

	    // key 파일 로드해서 읽어오기
	    aesProvider.Key = _key;
        
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
	    return JsonConvert.DeserializeObject<UserData>(result);
    }

    public void RemoveData<T>()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{(typeof(T))}");

        if (File.Exists(path) == true)
        {
            File.Delete(path);
        }
    }
}
