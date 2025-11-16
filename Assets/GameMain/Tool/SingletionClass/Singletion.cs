using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//继承该类即可实现单例模式，注意：使用了 new() 约束，确保 T 有一个无参数的构造函数,不能用于 MonoBehaviour
//懒汉式，第一次访问时才会创建实例
public class Singletion<T> where T:new()
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
}
