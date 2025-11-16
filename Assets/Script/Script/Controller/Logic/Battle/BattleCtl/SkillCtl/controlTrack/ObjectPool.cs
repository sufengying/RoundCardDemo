using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectPool : Singletion<ObjectPool>
{
    private Dictionary<GameObject,Queue<GameObject>> poolDict = new Dictionary<GameObject,Queue<GameObject>>();
    private Dictionary<GameObject,int> maxPoolSize=new Dictionary<GameObject,int>();
    private Dictionary<GameObject,int> initialPoolSize=new Dictionary<GameObject,int>();

    public Transform FatherTrans_Effect
    {
        get { return GameNode.Instance.fatherTrans_Effect; }
    }

    
    public void InitializePool(GameObject prefab,int initialSize=2,int maxSize=10,Transform parent=null)
    {
        if(prefab ==null ) return;

        if(!poolDict.ContainsKey(prefab))
        {
            poolDict[prefab]=new Queue<GameObject>();
            maxPoolSize[prefab]=maxSize;
            initialPoolSize[prefab]=initialSize;
        }

        for(int i =0;i<initialSize;i++)
        {
            var obj=CreatNewInstance(prefab,parent);
            poolDict[prefab].Enqueue(obj);
        }

    }



    public GameObject Get(GameObject prefab,int initialSize=2,int maxSize=10,Transform parent=null)
    {
        if(prefab==null) return null;

        if(!poolDict.ContainsKey(prefab))
        {
            InitializePool(prefab,initialSize,maxSize,parent);
        }
        //如果队列中由可用对象
        if(poolDict[prefab].Count>0)
        {
            var obj=poolDict[prefab].Dequeue();
            obj.transform.localPosition = Vector3.zero;

            if (obj.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 1f;
            }

            obj.SetActive(true);
            return obj;
        }
                // 如果池中没有可用对象，且未达到最大大小，创建新实例
        if (poolDict[prefab].Count < maxPoolSize[prefab])
        {
            var obj=CreatNewInstance(prefab,parent);
            return obj;
        }

        // 如果达到最大大小，返回null
        Debug.LogWarning($"Object pool for {prefab.name} is full!");
        return null;
    }
    public async void Return(GameObject instance,float delay=0)
    {
        if(delay>0)
        {
            await Task.Delay((int)(delay*1000));
            ReturnToPool(instance);
        }
        else
        {
            ReturnToPool(instance);
        }
    }

    private void ReturnToPool(GameObject instance)
    {
                // 1. 空值检查
        if (instance == null) return;

        
        // 重置 CanvasGroup 的 alpha 值
        if (instance.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.alpha = 1f;
        }
    

        // 2. 查找对应的预制体
        GameObject prefab = null;
        foreach (var kvp in poolDict)
        {
            // 通过名称前缀匹配预制体
            // 例如：如果实例名称是 "Bullet (Pooled)"，会匹配到 "Bullet" 预制体
            if (instance.name.StartsWith(kvp.Key.name))
            {
                prefab = kvp.Key;
                break;
            }
        }

        // 3. 检查是否找到对应的预制体
        if (prefab == null)
        {
            // 如果没找到，说明这个对象不是由对象池创建的
            Debug.LogWarning($"Trying to return an object to pool that wasn't created by the pool: {instance.name}");
            return;
        }

        // 4. 检查池的队列是否已满
        if (poolDict[prefab].Count < maxPoolSize[prefab])
        {
            // 4.1 如果队列未满：
            // 禁用对象
            instance.SetActive(false);
            // 设置父物体为对象池
            instance.transform.SetParent(GameNode.Instance.mainCanvas.transform);
            // 将对象加入队列
            poolDict[prefab].Enqueue(instance);
        }
        else
        {
            // 4.2 如果池已满：
            // 直接销毁对象
            Object.Destroy(instance);
        }

    }
    private GameObject CreatNewInstance(GameObject prefab,Transform parent=null)
    {
        
        var obj =Object.Instantiate(prefab,parent);
        obj.SetActive(false);
        obj.name=prefab.name;
        return obj;
    }
    
        /// <summary>
    /// 清空指定预制体的对象池
    /// </summary>
    public void ClearPool(GameObject prefab)
    {
        if (prefab == null || !poolDict.ContainsKey(prefab)) return;

        while (poolDict[prefab].Count > 0)
        {
            var obj = poolDict[prefab].Dequeue();
            Object.Destroy(obj);
        }

        poolDict.Remove(prefab);
        maxPoolSize.Remove(prefab);
        initialPoolSize.Remove(prefab);
    }

    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kvp in poolDict)
        {
            while (kvp.Value.Count > 0)
            {
                var obj = kvp.Value.Dequeue();
                Object.Destroy(obj);
            }
        }

        poolDict.Clear();
        maxPoolSize.Clear();
        initialPoolSize.Clear();
    }

    private void OnDestroy()
    {
        ClearAllPools();
    }

}
