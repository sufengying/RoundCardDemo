using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//继承该类实现单例模式，注意：只有需要挂载在unity场景上享受Unity生命周期才需要继承该类
public class SingletionMono<T> : MonoBehaviour where T : SingletionMono<T>
{
    private static T _instance;

    private static readonly Object _lock = new Object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != null)
        {
            Destroy(gameObject);
        }

    }
}
