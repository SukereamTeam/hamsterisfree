using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Stage_Importer : AssetPostprocessor
{
    private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static readonly char[] TRIM_CHARS = { '\"' };

    private static readonly string filePath = "Assets/Resources/Data/csv/StageTable.csv";
    private static readonly string exportPath = "Assets/Resources/Data/so/StageTable.asset";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (var asset in importedAssets)
        {
            if (filePath.Equals(asset) == false)
                continue;

            Stage_Entity data = (Stage_Entity)AssetDatabase.LoadAssetAtPath(exportPath, typeof(Stage_Entity));

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<Stage_Entity>();
                AssetDatabase.CreateAsset((ScriptableObject)data, exportPath);
            }

            data.hideFlags = HideFlags.NotEditable;
            data.list.Clear();

            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var headers = Regex.Split(reader.ReadLine(), SPLIT_RE);

                    while (!reader.EndOfStream)
                    {
                        string dataLine = reader.ReadLine();
                        var values = Regex.Split(dataLine, SPLIT_RE);
                        if (values.Length == 0 || values[0] == "")
                        {
                            continue;
                        }

                        Stage_Entity.Param csvData = new Stage_Entity.Param();

                        for (int j = 0; j < headers.Length; j++)
                        {
                            var value = values[j];
                            value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                            // 헤더와 일치하는 속성에 값을 할당
                            switch (headers[j])
                            {
                                case "Index":
                                    csvData.Index = Int32.Parse(value);
                                    break;
                                case "StageType":
                                    csvData.StageType = ParseStageType(value);
                                    break;
                                case "MapName":
                                    csvData.MapName = value;
                                    break;
                                case "SeedData":
                                    csvData.SeedData = ParseObjectData(value);
                                    break;
                                case "MonsterData":
                                    csvData.MonsterData = ParseObjectData(value);
                                    break;
                            }
                        }

                        data.list.Add(csvData);
                    }
                }
            }
        }
    }

    private static Stage_Entity.SerializableTuple<string, int> ParseStageType(string input)
    {
        Stage_Entity.SerializableTuple<string, int> result = new("", 0);

        string[] parts = input.Trim('(', ')').Split(',');

        if (parts.Length == 2)
        {
            string type = parts[0].Trim();

            if (Int32.TryParse(parts[1].Trim(), out int value))
            {
                result = new Stage_Entity.SerializableTuple<string, int>(type, value);
            }
            else
            {
                Debug.Log($"### Error ---> {parts[0]}, {parts[1]} <--- ParseStageType ");
            }
        }

        return result;
    }

    private static List<Stage_Entity.SerializableTuple<string, int>> ParseObjectData(string input)
    {
        // "((0, 0), (3, 0))"
        string[] pairs = input.Replace("(", "").Replace(")", "").Split(new[] { "), (" }, StringSplitOptions.RemoveEmptyEntries);

        // "0030"

        List<Stage_Entity.SerializableTuple<string, int>> resultList = new List<Stage_Entity.SerializableTuple<string, int>>(pairs.Length);


        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (keyValue.Length == 2)
            {
                if (int.TryParse(keyValue[1].Trim(), out int value))
                {
                    resultList.Add(new Stage_Entity.SerializableTuple<string, int>(keyValue[0], value));
                }
                else
                {
                    Debug.Log($"### Error ---> {keyValue[0]}, {keyValue[1]} <--- ParseSeedData ");
                }
            }
        }

        return resultList;
    }
}
