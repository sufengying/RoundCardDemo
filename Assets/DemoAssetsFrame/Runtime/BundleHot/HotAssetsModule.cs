
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 热更资源模块
    /// </summary>
    public class HotAssetsModule
    {
        /// <summary>
        /// 热更资源下载储存路径
        /// </summary>
        public string HotAssetsSavePath { get { return Application.persistentDataPath + "/HotAssets/" + CurBundleModuleEnum + "/"; } }
        /// <summary>
        /// 所有热更的资源列表
        /// </summary>
        public List<HotFileInfo> mAllHotAssetsList = new List<HotFileInfo>();
        /// <summary>
        /// 需要下载的资源列表
        /// </summary>
        public List<HotFileInfo> mNeedDownLoadAssetsList = new List<HotFileInfo>();
        /// <summary>
        /// 服务端资源清单
        /// </summary>
        private HotAssetsManifest mServerHotAssetsManifest;
        /// <summary>
        /// 本地资源清单
        /// </summary>
        private HotAssetsManifest mLocalHotAssetsManifest;
        /// <summary>
        /// 服务端资源热更清单储存路径
        /// </summary>
        private string mServerHotAssetsManifestPath;
        /// <summary>
        /// 本地资源热更清单文件储存路径
        /// </summary>
        private string mLocalHotAssetManifestPath;
        /// <summary>
        /// 热更公告
        /// </summary>
        public string UpdateNoticeContent { get { return mServerHotAssetsManifest.updateNotice; } }
        /// <summary>
        /// 当前下载的资源模块类型
        /// </summary>
        public BundleModuleEnum CurBundleModuleEnum { get; set; }
        /// <summary>
        /// 最大下载资源大小
        /// </summary>
        public float AssetsMaxSizeM { get; set; }
        /// <summary>
        /// 资源已经下载的大小
        /// </summary>
        public float AssetsDownLoadSizeM;
        /// <summary>
        /// 资源下载器
        /// </summary>
        private AssetsDownLoader mAssetsDownLoader;
        /// <summary>
        /// AssetBundle配置文件下载完成监听
        /// </summary>
        public Action<string> OnDownLoadABConfigListener;
        /// <summary>
        /// 下载AssetBundle完成的回调
        /// </summary>
        public Action<string> OnDownLoadAssetBundleListener;
        /// <summary>
        /// 所有热更资源的一个长度
        /// </summary>
        public int HotAssetCount { get { return mAllHotAssetsList.Count; } }

        private MonoBehaviour mMono;
        /// <summary>
        /// 下载所有资源完成的回调
        /// </summary>
        public Action<BundleModuleEnum> OnDownLoadAllAssetsFinish;

        public bool IsNeedRemove { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundleModule">资源模块类型</param>
        /// <param name="mono">MonoBehaviour对象</param>
        public HotAssetsModule(BundleModuleEnum bundleModule,MonoBehaviour mono)
        {
            mMono = mono;
            CurBundleModuleEnum = bundleModule;
        }
        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="startDownLoadCallback">开始下载的回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="isCheckAssetsVersion">是否检测资源版本</param>
        public void StartHotAssets(Action startDownLoadCallback,Action<BundleModuleEnum> hotFinish=null,bool isCheckAssetsVersion=true)
        {
            this.OnDownLoadAllAssetsFinish += hotFinish;
            //如果需要检测资源版本
            if (isCheckAssetsVersion)
            {
                //检测资源版本是否需要热更
                //用一个lambda表达式，接收两个参数，一个是bool类型，一个是float类型
                //isHot表示是否需要热更，size表示需要下载的资源大小
                CheckAssetsVersion((isHot,size)=> {
                    if (isHot)
                    {
                        //如果需要热更，开始下载热更资源，并且把开始下载的回调传递给StartDownLoadHotAssets方法
                        StartDownLoadHotAssets(startDownLoadCallback);
                    }
                    else
                    {
                        //如果不需要热更，直接触发所有文件下载完成的回调，并且把资源模块类型传递给回调
                        OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
                    }
                });
            }
        }
        /// <summary>
        /// 开始下载热更资源
        /// </summary>
        /// <param name="startDonwLoadCallBack"></param>
        public void StartDownLoadHotAssets(Action startDonwLoadCallBack)
        {
            //优先下载AssetBUndle配置文件，下载完成后呢，调用回调，让开发者及时加载配置文件
            //热更资源下载完成之后同样给与回调，供开发者动态加载刚下载完成的资源
            List<HotFileInfo> downLoadList = new List<HotFileInfo>();
            //遍历需要下载的资源列表
            for (int i = 0; i < mNeedDownLoadAssetsList.Count; i++)
            {
                //获取需要下载的资源列表的第i个资源
                HotFileInfo hotFile = mNeedDownLoadAssetsList[i];
                //如果这个ab包的包名包含Config 说明是配置文件，需要优先下载
                //一般来说，配置文件总是需要下载的
                //因为如果需要热更新，那么服务器的配置文件AB包的MD5值一定与本地的不一致
                //所以需要从服务器下载配置文件
                //又或者没有本地持久化目录没有这个文件，那么也需要从服务器下载配置文件
                if (hotFile.abName.Contains("config"))
                {
                    //把配置文件插入到需要下载的资源列表的最前面
                    downLoadList.Insert(0, hotFile);
                }
                else
                {
                    //否则就是正常添加
                    downLoadList.Add(hotFile);
                }
            }

            //创建一个资源队列
            Queue<HotFileInfo> downLoadQueue = new Queue<HotFileInfo>();
            //遍历需要下载的资源列表
            foreach (var item in downLoadList)
            {
                //把需要下载的资源添加到资源队列
                downLoadQueue.Enqueue(item);
            }
            //通过资源下载器，开始下载资源
            //1.new一个资源下载器——AssetsDownLoader
            //2.进行AssetsDownLoader的构造函数，传入参数
            //3.参数：
            //this：表示当前热更资源模块
            //downLoadQueue：表示需要下载的资源队列
            //mServerHotAssetsManifest.downLoadURL：表示下载地址
            //HotAssetsSavePath：表示保存路径，保存在持久化目录上
            //DownLoadAssetBundleSuccess：表示下载成功回调
            //DownLoadAssetBundleFailed：表示下载失败回调
            //DownLoadAllAssetBundleFinish：表示下载完成回调
            mAssetsDownLoader = new AssetsDownLoader(this,downLoadQueue,mServerHotAssetsManifest.downLoadURL,HotAssetsSavePath
                ,DownLoadAssetBundleSuccess,DownLoadAssetBundleFailed,DownLoadAllAssetBundleFinish);

            //开始下载，并且调用开始下载的回调
            startDonwLoadCallBack?.Invoke();

            //调用下载器的StartThreadDownLoadQueue方法，开始下载队列中的资源
            mAssetsDownLoader.StartThreadDownLoadQueue();

        }
        /// <summary>
        /// 检测资源版本
        /// </summary>
        /// <param name="checkCallBack">检查资源版本回调</param>
        public void CheckAssetsVersion(Action<bool,float> checkCallBack)
        {
            //生成本地和服务端资源热更清单路径（在持久化目录下）
            GeneratorHotAssetsManifest();
            //先清空需要下载的资源列表，防止数据残留
            mNeedDownLoadAssetsList.Clear();

            //用monobehavior的协程，开始下载资源清单，
            //从资源服务器的对应资源模块的AssetsHotManifest.json文件下载到持久化目录里
            //下载完成后执行lambda表达式
            mMono.StartCoroutine(DownLoadHotAssetsManifest(()=> {
                //资源清单下载完成
                //1.检测当前版本是否需要热更
                //通过对比本地资源清单和资源服务器的资源清单，例如对比补丁列表数量，判断是否需要热更
                //如果是首次安装，那么可能没有本地资源清单，那么需要热更
                if (CheckModuleAssetsIsHot())
                {
                    //需要热更  
                    //获取服务端资源清单的最后一个补丁
                    HotAssetsPatch serverHotPath = mServerHotAssetsManifest.hotAssetsPatchList[mServerHotAssetsManifest.hotAssetsPatchList.Count - 1];
                    //计算需要热更的文件列表（ab包）
                    //isNeedHot表示是否需要更新资源
                    bool isNeedHot= ComputeNeedHotAssetsList(serverHotPath);
                    if (isNeedHot)
                    {
                        //如果需要更新资源，则调用checkCallBack回调，并传递true和最大下载资源大小
                        checkCallBack?.Invoke(true,AssetsMaxSizeM);
                    }
                    else
                    {
                        //如果不需要更新资源，则调用checkCallBack回调，并传递false和0
                        checkCallBack?.Invoke(false, 0);
                    }
                }
                else
                {
                    //如果不需要热更，则调用checkCallBack回调，并传递false和0
                    checkCallBack?.Invoke(false, 0);
                }
                //2.如果需要热更，开始计算需要下载的文件 开始下载文件
                //3.如果不需要热更，说明文件是最新的，直接热更完成

            }));
        }
        /// <summary>
        /// 计算需要热更的文件列表
        /// </summary>
        /// <param name="serverAssetsPath">服务端资源清单的补丁版本</param>
        /// <returns></returns>
        public bool ComputeNeedHotAssetsList(HotAssetsPatch serverAssetsPath)
        {
            //判断持久化目录是否存在，不存在则创建
            if (!Directory.Exists(HotAssetsSavePath))
            {
                Directory.CreateDirectory(HotAssetsSavePath);
            }
            //如果本地资源清单文件存在，则反序列化成HotAssetsManifest对象
            if(File.Exists(mLocalHotAssetManifestPath))
                mLocalHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(mLocalHotAssetManifestPath));
            //最大下载资源大小设为零，防止数据误差
            AssetsMaxSizeM = 0;
            //遍历服务端资源清单的补丁版本中的资源列表（ab包）
            foreach (var item in serverAssetsPath.hotAssetsList)
            {
                //生成持久化目录的AssetBundle文件路径
                string localFilePath = HotAssetsSavePath + item.abName;

                //把该资源文件（ab包）添加到所有热更的资源列表
                mAllHotAssetsList.Add(item);
                //通过服务端的ab包的包名，获取本地持久化目录的md5值，这两个包的包名一致，返回持久化目录端的资源清单的md5值
                //如果没有本地资源清单，那么返回空
                string localMd5= GetLocalFileMd5ByBundleName(item.abName);
                //如果本地文件不存在，或者本地文件相同ab包包名的md5与服务端相同ab包包名的md5不一致，就需要热更
                //如果本地文件存在，并且本地文件相同ab包包名的md5与服务端相同ab包包名的md5一致，就不需要热更
                //如果是首次安装，那么可能没有本地文件，那么需要热更

                //如果指定路径没有文件，说明需要从服务器下载热更包，
                //如果指定路径有文件，需要对比这两个A包的md5值
                //如果md5值不一致，说明需要从服务器下载热更包，把这个ab包添加到需要下载队列
                //如果md5值一致，说明不需要从服务器下载热更包
                if (!File.Exists(localFilePath)||item.md5!= localMd5)
                {
                    //把这个资源文件（ab包）添加到需要下载的资源列表
                    mNeedDownLoadAssetsList.Add(item);
                    //把该资源文件（ab包）的大小添加到最大下载资源大小
                    AssetsMaxSizeM += item.size / 1024f;
                }
            }
            
            //这个返回值表示：
            //true: 有需要下载的资源（mNeedDownLoadAssetsList不为空）
            //false: 没有需要下载的资源（mNeedDownLoadAssetsList为空）
            return mNeedDownLoadAssetsList.Count > 0;
        }

        /// <summary>
        /// 通过ab包的包名，获取本地持久化目录的md5值
        /// </summary>
        /// <param name="bundleName">服务端的ab包的包名</param>
        /// <returns></returns>
        public string GetLocalFileMd5ByBundleName(string bundleName)
        {
            //如果持久化目录资源清单不为空，并且持久化目录资源清单的补丁列表不为空
            if (mLocalHotAssetsManifest!=null&& mLocalHotAssetsManifest.hotAssetsPatchList.Count>0)
            {
                //获取持久化目录资源清单的最后一个补丁
                HotAssetsPatch localPatch = mLocalHotAssetsManifest.hotAssetsPatchList[mLocalHotAssetsManifest.hotAssetsPatchList.Count-1];
                //遍历持久化目录资源清单的最后一个补丁的资源列表（ab包）
                foreach (var item in localPatch.hotAssetsList)
                {
                    //如果传入的ab包的包名与持久化目录资源清单的最后一个补丁的资源列表（ab包）的某个包名一致
                    if (string.Equals(bundleName,item.abName))
                    {
                        //返回该ab包的md5值
                        return item.md5;
                    }
                }
            }
            //否则返回空
            return "";
        }
        /// <summary>
        /// 检测模块资源是否需要热更
        /// </summary>
        /// <returns></returns>
        public bool CheckModuleAssetsIsHot()
        {
            //如果服务端资源清单不存，不需要热更（说明在热更文件夹中没有没有找到对应的热更资源清单）
            //说明热更文件夹没有这个资源模块的热更资源清单，说明该模块没有生成
            //说明该模块不需要热更
            //所以返回false表示不需要热更
            if (mServerHotAssetsManifest==null)
            {
                return false;   
            }
            //如果本地资源清单文件路径不存在，而服务端资源清单存在
            //（在持久化目录，文件不存在·），说明我们需要热更
            //本地没有清单文件，说明：
            //1.可能是首次安装
            //2.或者本地清单被删除
            //3.或者需要重新下载资源
            //所以返回true表示需要热更      
            if (!File.Exists(mLocalHotAssetManifestPath))
            {
                return true;
            }
            //判断本地资源清单补丁版本号是否与服务端资源清单补丁版本号一致，如果一致，不需要热更， 如果不一致，则需要热更
            //获得持久化目录的本地资源清单，并反序列化成HotAssetsManifest对象
            HotAssetsManifest localHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(mLocalHotAssetManifestPath));
            //如果本地资源清单的补丁列表为空，而服务端资源清单的补丁列表不为空
            //说明本地资源清单没有热更补丁，而服务端资源清单有热更补丁
            //说明需要热更
            if (localHotAssetsManifest.hotAssetsPatchList.Count==0&&mServerHotAssetsManifest.hotAssetsPatchList.Count!=0)
            {
                return true;
            }
         
            //获取本地热更补丁的最后一个补丁
            HotAssetsPatch localHotPatch = localHotAssetsManifest.hotAssetsPatchList[localHotAssetsManifest.hotAssetsPatchList.Count - 1];
            //获取服务端热更补丁的最后一个补丁
            HotAssetsPatch serverHotPatch = mServerHotAssetsManifest.hotAssetsPatchList[mServerHotAssetsManifest.hotAssetsPatchList.Count - 1];

            //如果本地热更补丁和服务端热更补丁都不为空
            if (localHotPatch!=null&& serverHotPatch!=null)
            {
                //如果本地热更补丁的版本号不等于服务端热更补丁的版本号
                //说明需要热更
                if (localHotPatch.patchVersion!=serverHotPatch.patchVersion)
                {
                    return true;
                }
                //else
                //{
                //    return false;
                //}
            }

            //如果服务器有补丁（serverHotPatch!=null），说明需要热更
            //如果服务器没有补丁（serverHotPatch==null），说明不需要热更
            //这是一个兜底的判断，确保：
            //服务器有补丁时进行热更
            //服务器没有补丁时不进行热更
            //处理前面条件都没有匹配到的情况
       
            if (serverHotPatch!=null)
            {
                
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 下载资源热更清单，协程
        /// </summary>
        /// <param name="downLoadFinish">下载完成回调</param>
        /// <returns></returns>
        private IEnumerator DownLoadHotAssetsManifest(Action downLoadFinish)
        {
            //获取热更文件清单的输出路径
            string url = BundleSettings.Instance.AssetBundleDownLoadUrl + "/HotAssets/" + CurBundleModuleEnum + "AssetsHotManifest.json";
            //创建一个UnityWebRequest请求，用于下载目标路径的资源
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            //设置超时时间
            webRequest.timeout = 30;

            Debug.Log("*** Requset HotAssetsMainfest Url:"+ url);

            //发送请求，并等待请求完成
            //SendWebRequest 是 UnityWebRequest 类的一个方法，用于发送 HTTP 请求并等待响应。
            //这里暂停协程，等待请求完成
            yield return webRequest.SendWebRequest();

            //如果请求失败，输出错误信息
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("DownLoad Error:"+webRequest.error);
            }
            else
            {
                //这里是把热更文件清单下载保存到持久化目录
                try
                {
                    Debug.Log("*** Request AssetBundle HotAssetsMainfest Url Finish Module:"+CurBundleModuleEnum 
                        +" txt:"+webRequest.downloadHandler.text);
                    //写入服务端资源热更清单到本地
                    //webRequest.downloadHandler.data 是 UnityWebRequest 类的一个属性，用于获取下载的数据。
                    //webRequest.downloadHandler.text 是 UnityWebRequest 类的一个属性，用于获取下载的数据。
                    //webRequest.downloadHandler.data 是 byte[] 类型，用于存储下载的数据。
                    //webRequest.downloadHandler.text 是 string 类型，用于存储下载的数据。
                    //这个方法把下载的数据写入到本地文件中
                    //如果路径不存在，会自动创建文件
                    //如果文件已存在，会覆盖原有内容
                    //如果目录不存在，会先创建目录
                    FileHelper.WriteFile(mServerHotAssetsManifestPath, webRequest.downloadHandler.data);
                    //把下载的数据转换为HotAssetsManifest对象，并赋值给mServerHotAssetsManifest,反序列化
                    mServerHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                        Debug.LogError("服务端资源清单配置下载异常，文件不存在或者配置有问题，更新出错，请检查：\n" 
                        + "URL: " + url + "\n"
                        + "Error: " + e.ToString() + "\n"
                        + "Response: " + webRequest.downloadHandler.text);
                 }
            }
            //下载完成回调，无论是否下载成功，都会调用这个回调
            downLoadFinish?.Invoke();
        }
        /// <summary>
        /// 生成本地和服务端资源热更清单路径
        /// </summary>
        public void GeneratorHotAssetsManifest()
        {
            mServerHotAssetsManifestPath = Application.persistentDataPath + "/Server" + CurBundleModuleEnum + "AssetsHotManifest.json";
            mLocalHotAssetManifestPath = Application.persistentDataPath + "/Local" + CurBundleModuleEnum + "AssetsHotManifest.json";
        }

        #region 资源下载回调
        /// <summary>
        /// 下载成功回调，下载成功这个AB包，如果是配置文件AB包，把这个AB包传到AssetBundleManager，解析出这个模块的所有资源信息并存储
        /// </summary>
        /// <param name="hotFile">下载的资源文件</param>
        public void DownLoadAssetBundleSuccess(HotFileInfo hotFile)
        {
            //获取ab包的包名，去掉.unity后缀
            string abName = hotFile.abName.Replace(".unity", "");
            //如果ab包的包名包含bundleconfig，说明是配置文件
            if (hotFile.abName.Contains("bundleconfig"))
            {
                //调用回调，并传递ab包的包名
                OnDownLoadABConfigListener?.Invoke(abName);
                //如果下载成功需要及时去加载配置文件
                AssetBundleManager.Instance.LoadAssetBundleConfig(CurBundleModuleEnum);
            }
            else
            {
                OnDownLoadAssetBundleListener?.Invoke(abName);
            }
            HotAssetsManager.DownLoadBundleFinish?.Invoke(hotFile);
        }
        /// <summary>
        /// 下载资源包失败回调，输出错误信息
        /// </summary>
        /// <param name="hotFile"></param>
        public void DownLoadAssetBundleFailed(HotFileInfo hotFile)
        {
            //如果下载失败，输出错误信息
            Debug.LogError("DownLoad Error:"+hotFile.abName);

        }
        /// <summary>
        /// 下载所有资源包完成回调后，把原来的本地资源清单文件删除，然后复制服务端资源清单文件作为本地资源清单文件
        /// </summary>
        /// <param name="hotFile"></param>
        public void DownLoadAllAssetBundleFinish (HotFileInfo hotFile)
        {
            if (File.Exists(mLocalHotAssetManifestPath))
            {
                File.Delete(mLocalHotAssetManifestPath);
            }
            //把服务端热更清单文件考本到本地
            //服务器文件：/ServerXXXAssetsHotManifest.json
            //本地文件：/LocalXXXAssetsHotManifest.json
            //复制后：保持各自的文件名不变
            //实际上复制了文件里的内容，
            //mLocalHotAssetManifestPath路径不存在，File.Copy会尝试创建这个文件
            //mLocalHotAssetManifestPath路径存在，File.Copy会覆盖原有内容
            File.Copy(mServerHotAssetsManifestPath, mLocalHotAssetManifestPath);
            OnDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
        }
        #endregion

        /// <summary>
        /// 在主线程中更新，这个方法作用是调用AssetsDownLoader类中的OnMainThreadUpdate方法，确保传入的回调被执行
        /// </summary>
        public void OnMainThreadUpdate()
        {
            mAssetsDownLoader?.OnMainThreadUpdate();
        }
        /// <summary>
        /// 设置下载线程个数，这个方法在HotAssetsManager类中调用
        /// 由这个类从正在下载热更资源模块的字典中获取热更资源模块，然后调用这个方法，设置最大下载线程个数
        /// </summary>
        /// <param name="threadCount">下载线程个数</param>
        public void SetDownLoadThreadCount(int threadCount)
        {
            Debug.Log("多线程负载均衡:"+threadCount+" ModuleType:"+CurBundleModuleEnum);
            if (mAssetsDownLoader!=null)
            {
                //设置下载器 AssetsDownLoader 最大下载线程个数
                mAssetsDownLoader.MAX_THREAD_COUNT = threadCount;
            }
        }
        /// <summary>
        /// 判断热更文件是否存在
        /// </summary>
        public bool HotAssetsIsExists(string bundleName)
        {
            //遍历所有热更资源列表
            foreach (var item in mAllHotAssetsList)
            {
                //如果传入的ab包的包名与所有热更资源列表的某个包名一致
                if (string.Equals(bundleName,item.abName))
                {
                    //返回true表示存在
                    return true;
                }
            }
            //返回false表示不存在
            return false;
        }

    }
}
