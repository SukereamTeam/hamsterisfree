using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.Threading;
using System.IO;
using System;
using System.Reflection;
using DataTable;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;


#if UNITY_EDITOR
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

    private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static readonly char[] TRIM_CHARS = { '\"' };

    private const string CSV_PATH = "Assets/Resources/Data/csv";
    private const string SO_PATH = "Assets/Resources/Data/so";

    private const string FILE_FORMAT = "csv";

    [SerializeField]
    private DataContainer dataContainer;

    [ReadOnlyCustom]
    [SerializeField]
    private SheetData[] sheetDatas;




    public async UniTaskVoid DownloadAll(Action _Oncomplete = null)
    {
        foreach(var sheet in sheetDatas)
        {
            await Download(sheet, FILE_FORMAT);
        }

        _Oncomplete?.Invoke();

        await UniTask.Yield();

        foreach (var sheet in sheetDatas)
        {
            CreateScriptableObject(sheet);
        }

        EditorUtility.SetDirty(this.dataContainer);
        AssetDatabase.SaveAssets();
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


            await File.WriteAllTextAsync(fileUrl, www.downloadHandler.text + "\n");

            Debug.Log("Download Complete.");
        }
    }

    private void CreateScriptableObject(SheetData _SheetData)
    {
        // eg) 1. StageTable 에서 Table 떼기
        var tableName = _SheetData.SheetName.Split("Table")[0];

        // eg) 2. 'Stage' 문자열이 들어간 클래스 찾기
        Type foundType = FindClassWithPartialString<Table_Base>(tableName);

        if (foundType == null)
        {
            Debug.Log($"Not Found DataTable Type Class ---> {_SheetData.SheetName}");
            return;
        }

        // 데이터테이블 기반의 ScriptableObject 에셋 경로
        string assetPath = $"{SO_PATH}/{_SheetData.SheetName}.asset";

        // 위에서 찾은 클래스의 타입(foundType)을 기반으로 하는 ScriptableObject 로드
        var data = (ScriptableObject)AssetDatabase.LoadAssetAtPath(assetPath, foundType);

        if (data == null)
        {
            // foundType 기반의 ScriptableObject 인스턴스 생성
            data = ScriptableObject.CreateInstance(foundType);

            // 위에서 생성한 인스턴스로 에셋 생성
            AssetDatabase.CreateAsset(data, assetPath);
        }

        // 인스펙터에서 수정 못하게
        data.hideFlags = HideFlags.NotEditable;



        // foundType 이 가지고 있는 필드 긁어오기
        FieldInfo[] fields = foundType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        //PropertyInfo[] properties = foundType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);


        IEnumerable<MemberInfo> allMembers = fields.Cast<MemberInfo>();//fields.Cast<MemberInfo>().Concat(properties.Cast<MemberInfo>());

        // List 타입인 것 가지고 오기 (eg. Table_Stage -> List<Param> list)
        MemberInfo listMember = allMembers.Where(member => IsListType(member)).FirstOrDefault();

        // 정확히 어떤 List 타입인지 타입 가져오기
        Type listType = GetListElementType(listMember);

        // 위에서 찾았던 List 타입인 필드 끌어오기
        var fieldValue = ((listMember as FieldInfo)?.GetValue(data)) as IList;

        // 끌어와서 일단 list 초기화 해주기
        fieldValue.Clear();


        // CSV Path
        var csvPath = $"{CSV_PATH}/{_SheetData.SheetName}.{FILE_FORMAT}";

        using (FileStream stream = File.Open(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                // CSV 헤더 나누기
                var headers = Regex.Split(reader.ReadLine(), SPLIT_RE);

                while (!reader.EndOfStream)
                {
                    string dataLine = reader.ReadLine();
                    var values = Regex.Split(dataLine, SPLIT_RE);
                    if (values.Length == 0 || values[0] == "")
                    {
                        continue;
                    }


                    // 위에서 찾았던 List의 타입으로 인스턴스 생성 (값 넣어서 List 변수에 넣어줄 예정)
                    object csvData = Activator.CreateInstance(listType);


                    for (int j = 0; j < headers.Length; j++)
                    {
                        var value = values[j];
                        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                        // listType의 타입이 사용자 정의 클래스라서... 클래스에 header와 이름이 같은 필드가 있는지 체크
                        FieldInfo csvDataField = listType.GetField(headers[j]);
                        if (csvDataField != null)
                        {
                            if (csvDataField.Name.Equals(headers[j]))
                            {
                                // StageType 필드인 경우 파싱 필요해서
                                // 시트 string 값 : "(LimitTime, 60)" -> Type: LimitTime, Count: 60(60초) 로 파싱이 필요함
                                Type stageTypeField = typeof(Table_Base.SerializableTuple<string, int>);
                                if (csvDataField.FieldType.Equals(stageTypeField))
                                {
                                    csvDataField.SetValue(csvData, ParseStageType(value));
                                    continue;
                                }

                                // List<ObjectData> 필드인 경우 파싱 필요
                                // 시트 string 값 : "((Default, 0, 3), (Boss, 5, 1))" -> 각각 나눠 List로 저장하는 파싱 작업 필요
                                Type objectDataTypeField = typeof(List<Table_Base.SerializableTuple<string, int, int>>);
                                if (csvDataField.FieldType.Equals(objectDataTypeField))
                                {
                                    csvDataField.SetValue(csvData, ParseObjectData(value));
                                    continue;
                                }

                                // Int 타입인 경우 파싱 필요
                                if (csvDataField.FieldType.Equals(typeof(Int32)))
                                {
                                    csvDataField.SetValue(csvData, Int32.Parse(value));
                                    continue;
                                }

                                if (csvDataField.FieldType.Equals(typeof(float)))
                                {
                                    csvDataField.SetValue(csvData, float.Parse(value));
                                    continue;
                                }


                                csvDataField.SetValue(csvData, value);
                            }
                        }
                    }

                    // 위에서 value 넣어준 csvData를 삽입
                    fieldValue.Add(csvData);
                }
            }
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        SetDataContainer(foundType, data);
    }


    private void SetDataContainer(Type _Type, ScriptableObject _Data)
    {
        var script = this.dataContainer.GetComponent<DataContainer>();

        foreach (var field in script.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.FieldType.Equals(_Type))
            {
                field.SetValue(script, _Data);
                break;
            }
        }
    }

    private static bool IsListType(MemberInfo member)
    {
        if (member is FieldInfo fieldInfo && typeof(List<>).IsAssignableFrom(fieldInfo.FieldType.GetGenericTypeDefinition()))
        {
            return true;
        }

        //if (member is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsGenericType && typeof(List<>) == propertyInfo.PropertyType.GetGenericTypeDefinition())
        //{
        //    return true;
        //}

        return false;
    }

    private static Type GetListElementType(MemberInfo member)
    {
        if (member is FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType.GetGenericArguments()[0];
        }

        //if (member is PropertyInfo propertyInfo)
        //{
        //    return propertyInfo.PropertyType.GetGenericArguments()[0];
        //}

        throw new ArgumentException($"### {member.Name} 는 Field가 아니다 ###");
    }

    private static Type FindClassWithPartialString<T>(string partialString)
    {
        var classType = typeof(T);
        var assembly = classType.Assembly;
        Type[] types = assembly.GetTypes(); // 어셈블리 내의 모든 타입 가져오기

        foreach (Type type in types)
        {
            // Namespace 일치 확인, 상속 받은 클래스인지 확인, 이름 포함하는지 확인
            if (type.Namespace == "DataTable" && classType.IsAssignableFrom(type) && type.Name.Contains(partialString))
            {
                return type; // 일치하는 클래스 타입 반환
            }
        }

        Debug.Log($"### {partialString} 와 일치하는 클래스 타입이 없음 ###");
        return null; // 일치하는 클래스 타입이 없을 경우 null 반환
    }

    private static Table_Base.SerializableTuple<string, int> ParseStageType(string input)
    {
        Table_Base.SerializableTuple<string, int> result = new("", 0);

        string[] parts = input.Trim('(', ')').Split(',');

        if (parts.Length == 2)
        {
            string type = parts[0].Trim();

            if (Int32.TryParse(parts[1].Trim(), out int value))
            {
                result = new Table_Base.SerializableTuple<string, int>(type, value);
            }
            else
            {
                Debug.Log($"### Error ---> {parts[0]}, {parts[1]} <--- ParseStageType ");
            }
        }

        return result;
    }

    private static List<Table_Base.SerializableTuple<string, int, int>> ParseObjectData(string input)
    {
        if (input.Equals("NULL"))
            return null;

        string[] tupleStrings = input.Split(new char[] { '(', ')', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        List<Table_Base.SerializableTuple<string, int, int>> resultList = new List<Table_Base.SerializableTuple<string, int, int>>((tupleStrings.Length / 3));

        for (int i = 0; i < tupleStrings.Length; i += 3)
        {
            string strValue = tupleStrings[i];
            int typeIndexValue = int.Parse(tupleStrings[i + 1]);
            int countValue = int.Parse(tupleStrings[i + 2]);

            bool isFind = false;
            for (int j = 0; j < resultList.Count; j++)
            {
                if (resultList[j].Type == strValue && resultList[j].SubType == typeIndexValue)
                {
                    // 원하는 string 값을 찾았을 때 int 값을 수정
                    isFind = true;
                    resultList[j].Value += countValue;
                    break;
                }
            }

            if (isFind == false)
            {
                resultList.Add(new Table_Base.SerializableTuple<string, int, int>(strValue, typeIndexValue, countValue));
            }
        }

        return resultList;
    }
}

#endif