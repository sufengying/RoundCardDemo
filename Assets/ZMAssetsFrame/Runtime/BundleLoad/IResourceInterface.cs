
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IResourceInterface  
{
    void Initlizate();

    void PreLoadObj(string path,int count=1);

    void PreLoadResource<T>(string path) where T : UnityEngine.Object;

    GameObject Instantiate(string path, Transform parent, Vector3 localPoition, Vector3 localScale, Quaternion quaternion);

    void InstantiateAsync(string path, Action<GameObject, object, object> loadAsync, object param1, object param2);

    long InstantiateAndLoad(string path, Action<GameObject, object, object> loadAsync, Action loading, object param1, object param2);

    void RemoveObjectLoadCallBack(long loadid);

    void Release(GameObject obj, bool destroy = false);

    void Release(Texture texture);

    Sprite LoadSprite(string path);

    Texture LoadTexture(string path);

    AudioClip LoadAudio(string path);

    TextAsset LoadTextAsset(string path);

    Sprite LoadAtlasSprite(string atlasPath, string spriteName);

    long LoadTextureAsync(string path, Action<Texture, object> loadAsync, object param1 = null);

    long LoadSpriteAsync(string path, Image image, bool setNativeSize = false, Action<Sprite> loadAsync = null);

    void ClearAllAsyncLoadTask();

    void ClearResourcesAssets(bool absoluteCleaning);//是否深度清理


}
