using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 기본 Generic Singleton
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
    protected static T _instance = null;

    public static bool IsInstance => _instance != null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = System.Activator.CreateInstance(typeof(T)) as T;
            }

            return _instance;
        }
    }

}

/// <summary>
/// MonoBehaviour를 상속받은 Generic Singleton
/// 오브젝트 생성 타입
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, new()
{
    private static object _syncObj = new object();
    protected static T _instance = null;

    public static bool IsInstance => _instance;

    public static T Instance
    {
        get
        {
            lock (_syncObj)
            {
                if (!_instance)
                {
                    // awake 호출시 에러 메세지 발생
                    _instance = FindObjectOfType(typeof(T)) as T;
                    if (!_instance)
                    {
                        string name = String.Concat("Singleton.", typeof(T).ToString());
                        _instance = new GameObject(name, typeof(T)).GetComponent<T>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        _instance = null;
    }

    public static void Clear()
    {
        if (_instance)
        {
            DestroyImmediate(_instance.gameObject);
            _instance = null;
        }
    }
}