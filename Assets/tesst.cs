using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZM.AssetFrameWork;
public class tesst 
{
    // Start is called before the first frame update
    //Assets/Hall/Prefab/LoginWindow.prefab
    private void Awake()
    {
        
        //³õÊ¼»¯¿ò¼Ü
        ZMAssetsFrame.Instance.InitFrameWork();
        Debug.Log(Application.persistentDataPath);

    }
    void Start()
    {
       // HotUpdateManager.Instance.HotAndUnPackAssets(BundleModuleEnum.loginWindow, this);
    }

    public void StartGame()
    {
        ZMAssetsFrame.Instantiate("Assets/BundleDemo/LoginWindow/LoginWindow", null, Vector3.zero, Vector3.one, Quaternion.identity);
    }

}