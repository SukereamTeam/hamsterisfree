// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// #if UNITY_EDITOR
// public class ReadOnlyCustom : PropertyAttribute
// {
//     
// }
//
//
// [CustomPropertyDrawer(typeof(ReadOnlyCustom))]
// public class ReadOnlyDrawer : PropertyDrawer
// {
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         return EditorGUI.GetPropertyHeight(property, label, true);
//     }
//
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);
//         EditorGUI.BeginDisabledGroup(true); // 인스펙터에서 수정 비활성화
//
//         EditorGUI.PropertyField(position, property, label, true);
//
//         EditorGUI.EndDisabledGroup();
//         EditorGUI.EndProperty();
//     }
// }
//
// #endif