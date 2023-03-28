using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _mInstance;

    public static T instance
    {
        get
        {
            if (_mInstance == null)
            {
                _mInstance = FindObjectOfType<T>();
                if (_mInstance == null)
                {
                    GameObject obj = new GameObject();
                    _mInstance = obj.AddComponent<T>();
                }
            }
            return _mInstance;
        }
    }

    protected virtual void Awake()
    {
        _mInstance = this as T;
    }
}

public class SingletonSimple<T> where T : class, new()
{
    private static T _mInstance;

    public static T instance
    {
        get
        {
            if (_mInstance == null)
            {
                _mInstance = new T();

            }
            return _mInstance;
        }
    }
}

public class ClassSingleton<T> where T : class, new()
{
    private static T MInstance;

    public static T Instance
    {
        get
        {
            if (MInstance == null)
            {
                MInstance = new T();

            }
            return MInstance;
        }
    }
}
