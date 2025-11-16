
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    public class ZMFrameBase : MonoBehaviour
    {
        protected static ZMAssetsFrame _Instance = null;

        public static ZMAssetsFrame Instance
        {
            get
            {
                if (_Instance==null)
                {
                   _Instance = Object.FindFirstObjectByType<ZMAssetsFrame>();
                    if (_Instance==null)
                    {
                        GameObject obj = new GameObject(typeof(ZMAssetsFrame).Name);
                        //禁止销毁这个物体
                        DontDestroyOnLoad(obj);
                        _Instance=obj.AddComponent<ZMAssetsFrame>();
                        _Instance.OnInitlizate();
                    }
                }
                return _Instance;
            }
        }


        protected virtual void OnInitlizate()
        {

        }
    }
}
