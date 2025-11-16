
using UnityEngine;
namespace ZM.AssetFrameWork
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static T mInstance = null;
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = Object.FindFirstObjectByType<T>();
                    if (mInstance == null)
                    {
                        var obj = new GameObject(typeof(T).Name);
                        mInstance = obj.AddComponent<T>();
                        mInstance.OnAwake();
                    }
                }
                return mInstance;
            }
        }
        protected virtual void OnAwake()
        {

        }
        public virtual void Dispose()
        {
            Destroy(mInstance.gameObject);
        }
    }
}
