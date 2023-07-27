using UnityEngine;
using System.Reflection;
using System;

public static class UnityEditorExtension
{
    public static void AssertCheckNullWithSerializeFields(this MonoBehaviour behaviour)
    {
        if (Application.isEditor == false)
        {
            return;
        }

        FieldInfo[] fieldInfos = behaviour.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            SerializeField serializeField = null;
            try
            {
                serializeField = fieldInfo.GetCustomAttribute<SerializeField>(true);
            }
            catch (Exception _)
            {
                continue;
            }

            if (serializeField != null)
            {
                var value = fieldInfo.GetValue(behaviour);
                behaviour.Assert(value.IsNotNull(), String.Format("{0} is null.", fieldInfo));
            }
        }
    }

    public static object FindSerializeField(this MonoBehaviour behaviour, string fieldName)
    {
        var fieldInfo = behaviour.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(behaviour);
        }

        return null;
    }
}