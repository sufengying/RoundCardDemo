
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZM.AssetFrameWork
{
    public partial class ZMAssetsFrame :ZMFrameBase
    {
        public static Transform RecyclObjRoot { get; private set; }

        private IHotAssets mHotAssets = null;

        private IResourceInterface mResource = null;

        private IDecompressAssets mDecompressAssets = null;
        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            //创建一个游戏对象
            GameObject recyclObjectRoot = new GameObject("RecyclObjRoot");
            //把该游戏对象的transform赋值给RecyclObjRoot
            RecyclObjRoot = recyclObjectRoot.transform;
            //设置该游戏对象为非激活状态
            recyclObjectRoot.SetActive(false);
            //设置该游戏对象在场景中不销毁
            DontDestroyOnLoad(recyclObjectRoot);
            //创建一个热更管理器
            mHotAssets = new HotAssetsManager();
            //创建一个解压管理器
            mDecompressAssets =new  AssetsDecompressManager();
            //创建一个资源管理器
            mResource = new ResourceManager();
            //初始化资源管理器
            //这里的初始化，主要作用是
            //1.设置热更资源的下载完成回调
            mResource.Initlizate();
        }

        public void Update()
        {
            //在主线程中更新
            mHotAssets?.OnMainThreadUpdate();
        }
    }
}
