
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 热更新管理器，采用单例模式
    /// </summary>
    public class HotUpdateManager : Singleton<HotUpdateManager>
    {
        private Main main;
        private HotAssetsWindow mHotAssetsWindow;
        /// <summary>
        /// 热更并且解压热更模块
        /// </summary>
        /// <param name="bundleModule">资源模块类型</param>
        /// <param name="main">Main对象</param>
        public void HotAndUnPackAssets(BundleModuleEnum bundleModule,Main main)
        {
            if (this.main == null)
            {
                this.main = main;
            }
            //实例化了一个资源加载窗口（有进度条）
            mHotAssetsWindow = InstantiateResourcesObj<HotAssetsWindow>("HotAssetsWindow");
            //开始解压游戏内嵌资源
            //从unity工程的streamingAssets目录中，读取文件并写入到持久化目录下
            //当读取完毕就会触发回调
            //主要调用了AssetsDecompressManager.cs中的StartDeCompressBuiltinFile方法
           IDecompressAssets decompress= ZMAssetsFrame.StartDeCompressBuiltinFile(bundleModule,()=> {
                //说明资源开启解压了
                //这是读取本地文件，所以不需要网络

                //当资源解压完毕
                //检查设备是否有网络连接
                //NotReachable：无网络连接
                //ReachableViaCarrierDataNetwork：通过移动数据网络连接
                //ReachableViaLocalAreaNetwork：通过WiFi或局域网连接
                if (Application.internetReachability== NetworkReachability.NotReachable)
                {
                    //在场景上实例化一个叫UpdateTipsWindow的预制体，并返回UpdateTipsWindow组件
                    //这里实际上加载了一个资源提示更新弹窗，并且绑定了两个按钮事件
                    InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow").
                                                                InitView("当前无网络，请检测网络重试？", 
                                                                            ()=> { NotNetButtonClick(bundleModule); }, 
                                                                            () => { NotNetButtonClick(bundleModule); });
                    return;
                }
                //如果设备有网络，则检查资源版本
                else
                {
                    CheckAssetsVersion(bundleModule);
                }
            });
            //更新解压进度
            mHotAssetsWindow.ShowDecompressProgress(decompress);
        }

        public void NotNetButtonClick(BundleModuleEnum bundleModule)
        {
            //如果么有网络，弹出弹窗提示，提示用户没有网络请重试
            if (Application.internetReachability!= NetworkReachability.NotReachable)
            {
                CheckAssetsVersion(bundleModule);
            }
            else
            {

            }

        }
        public void CheckAssetsVersion(BundleModuleEnum bundleModule)
        {
            //主要调用了HotAssetsManager.cs中的CheckAssetsVersion方法
            //HotAssetsManager.CheckAssetsVersion会跟据传入的资源模块，从字典或者创建一个AssetModule对象
            //然后调用AssetModule的CheckAssetsVersion方法
            //AssetModule的CheckAssetsVersion方法，会先下载服务器的对应资源模块的热更文件清单
            //然后对比本地资源清单和资源服务器的资源清单，例如对比补丁列表数量，判断是否需要热更
            //如果是首次安装，那么可能没有本地资源清单，那么需要热更
            //计算需要热更的Ab包的大小
            //最后执行这里的lambda表达式，把是否需要热更和需要热更的Ab包的大小传入

            //这里的lambda表达式，主要作用是
            //当确认需要热更时，会根据用户的网络类型，如果是wifi，则直接热更，否则弹出提示弹窗，让用户决定是否更新资源
            ZMAssetsFrame.CheckAssetsVersion(bundleModule,(isHot,sizem)=> {
                if (isHot)
                {
                    //当用户使用是流量的时候呢，需要询问用户是否需要更新资源
                    //网络连接类型：
                    //NetworkReachability.ReachableViaCarrierDataNetwork：使用移动数据网络（如4G/5G）
                    //运行平台：
                    //RuntimePlatform.WindowsEditor：在Unity Windows编辑器下运行
                    //RuntimePlatform.OSXEditor：在Unity Mac编辑器下运行
                    if (Application.internetReachability== NetworkReachability.ReachableViaCarrierDataNetwork||Application.platform==
                    RuntimePlatform.WindowsEditor||Application.platform==RuntimePlatform.OSXEditor)
                    {
                        //弹出选择弹窗，让用户决定是否更新
                        //再次复制出提示弹窗，并且绑定两个按钮事件
                        //确认更新
                        //退出游戏
                        InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow").
                        InitView("当前有"+sizem.ToString("F2")+"m,资源需要更新，是否更新",()=> {
                            //确认更新回调
                            StartHotAssets(bundleModule);
                        },
                        ()=> {
                            //退出游戏回调
                            Application.Quit();
                        });
                    }
                    else
                    {
                        //假如是wifi，则直接热更，不必弹出提示
                        StartHotAssets(bundleModule);
                    }
                }
                else
                {
                    //如果不需要热更，说明用户已经热更过了，资源是最新的，直接进入游戏 TODO
                    OnHotFinishCallBack(bundleModule);
                }
            });
        }
        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="bundleModule"></param>
        public void StartHotAssets(BundleModuleEnum bundleModule)
        {
            //主要调用了HotAssetsManager.cs中的HotAssets方法
            //传入资源模块，开始热更回调，热更完成回调，等待热更回调，是否检查资源版本
            //OnStartHotAssetsCallBack：什么都不执行
            //OnHotFinishCallBack：初始化游戏环境，然后执行main.StartGame()：克隆某个物体到场景上
            //因为已经检查过资源版本，所以这里不需要检查

            //HotAssetsManager.cs中的HotAssets方法会创建一个HotAssetsModule对象
            //然后把这个对象添加到正在下载热更资源的列表和字典中
            //然后调用HotAssetsModule的StartHotAssets方法，并把回调传入
            ZMAssetsFrame.HotAssets(bundleModule, OnStartHotAssetsCallBack, OnHotFinishCallBack,null,false);
            //更新热更进度
            //主要调用了HotAssetsWindow.cs中的GetHotAssetsModule方法，返回一个HotAssetsModule对象
            mHotAssetsWindow.ShowHotAssetsProgress(ZMAssetsFrame.GetHotAssetsModule(bundleModule));
        }
        /// <summary>
        /// 热更完成回调
        /// </summary>
        public void OnHotFinishCallBack(BundleModuleEnum bundleModule)
        {
            Debug.Log("OnHotFinishCallBack.....");
           
            main.StartCoroutine(InitGameEnv());
        }

        public void OnStartHotAssetsCallBack(BundleModuleEnum bundleModule)
        {

        }
        /// <summary>
        /// 初始化游戏环境
        /// </summary>
        /// <returns></returns>
         private IEnumerator InitGameEnv()
        {
            for (int i = 0; i < 100; i++)
            {
                mHotAssetsWindow.progressSlider.value = i / 100.0f;
                if (i==1)
                {
                    mHotAssetsWindow.progressText.text = "加载本地资源...";
                }
                else if (i==20)
                {
                    mHotAssetsWindow.progressText.text = "加载配置文件...";
                }
                else if (i == 70)
                {
                    mHotAssetsWindow.progressText.text = "加载AssetBUndle配置文件...";
                    //AssetBundleManager.Instance.LoadAssetBundleConfig(BundleModuleEnum.Game);
                }
                else if (i == 90)
                {
                    mHotAssetsWindow.progressText.text = "加载游戏配置文件...";
                    LoadGameConfig();
                }
                else if (i == 99)
                {
                    mHotAssetsWindow.progressText.text = "加载地图场景...";
                }
                yield return null;

            }
            main.StartGame();
            GameObject.Destroy(mHotAssetsWindow.gameObject);
        }
        public void LoadGameConfig()
        {

        }
        /// <summary>
        /// 实例化资源对象，从Resources资源中加载一个GameObject，并实例化，并返回T组件
        /// </summary>
        /// <typeparam name="T">返回的组件</typeparam>
        /// <param name="prefabName">资源名字，该资源直接放到Resources目录下</param>
        /// <returns></returns>     
        public T InstantiateResourcesObj<T>(string prefabName)
        {
            //从Resources资源中加载一个GameObject，并实例化，并返回T组件
           return  GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(prefabName)).GetComponent<T>();
        }
    }
}
