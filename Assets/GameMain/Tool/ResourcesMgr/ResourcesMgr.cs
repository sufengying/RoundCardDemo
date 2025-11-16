using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class ResourcesMgr : Singletion<ResourcesMgr>
{
    /// <summary>
    /// 加载预制体（克隆）
    /// </summary>
    /// <param name="path">预制体存储路径</param>
    /// <param name="parent">将预制体复制到哪个父节点</param>
    /// <param name="restPos">是否重置位置</param>
    /// <param name="restScale">是否重置缩放大小</param>
    /// <param name="restRotation">是否重置旋转</param>
    /// <returns></returns>
    public GameObject LoadGameObject(string path,Transform parent=null,bool restPos=false,bool restScale=false,bool restRotation=false)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"资源加载失败: {path}");
            return null;
        }

        GameObject obj = Object.Instantiate(prefab, parent);
        obj.name=prefab.name;
        if (restPos)
        {
            obj.transform.localPosition = Vector3.zero;
        }
        if (restScale)
        {
            obj.transform.localScale = Vector3.one;
        }
        if (restRotation)
        {
            obj.transform.localRotation = Quaternion.identity;
        }
        return obj;

    }

    public GameObject InstantiateGameObject(GameObject prefab,Transform parent=null,bool restPos=false,bool restScale=false,bool restRotation=false)
    {
        if(prefab==null)
        {
            Debug.LogError("预制体为空");
            return null;
        }
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.name=prefab.name;
        if (restPos)
        {
            obj.transform.localPosition = Vector3.zero;
        }
        if (restScale)
        {
            obj.transform.localScale = Vector3.one;
        }
        if (restRotation)
        {
            obj.transform.localRotation = Quaternion.identity;
        }
        return obj;
    }

    /// <summary>
    /// 加载预制体（克隆）,没有返回值
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    public void LoadGameObjectVoid(string path, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"资源加载失败: {path}");
            return;
        }

       GameObject obj= Object.Instantiate(prefab, parent);
       obj.name = prefab.name;

    }

    /// <summary>
    /// 加载对象，同时获取对象身上的某一个组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public T LoadObject<T>(string path,Transform parent=null) where T:Component
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"资源加载失败: {path}");
            return null;
        }

        GameObject obj = Object.Instantiate(prefab, parent);
        T component = obj.GetComponent<T>();

        if (component == null)
        {
            Debug.LogError($"GameObject {path} 没有组件 {typeof(T)}");
            return null;
        }
        return component;

    }

    public T LoadAsset<T>(string path) where T:Object
    {
        T asset = Resources.Load<T>(path);
        if (asset == null)
        {
            Debug.LogError($"资源加载失败: {path}");
            return null;
        }
        return asset;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public async Task LoadObjectAsync(string path,Transform parent)
    {
        GameObject prefab = await LoadAssetAsync(path);
        if(prefab == null)
        {
            Debug.LogError($"资源加载失败: {path}");
            return ;
        }
        GameObject obj = Object.Instantiate(prefab,parent);
        obj.name = prefab.name;
  
    }

    private async Task<GameObject> LoadAssetAsync(string path)
    {
        
        ResourceRequest request = Resources.LoadAsync<GameObject>(path);
        await Task.Yield();

        
        while (!request.isDone)
        {
           
            await Task.Yield();
        }
        
        return request.asset as GameObject;
    }
}
