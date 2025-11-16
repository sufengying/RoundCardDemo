
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    public class BundleItem
    {
        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string path;
        /// <summary>
        /// 文件加载路径crc
        /// </summary>
        public uint crc;
        /// <summary>
        /// AssetBundle名称
        /// </summary>
        public string bundleName;
        /// <summary>
        /// 资源名称
        /// </summary>
        public string assetName;
        /// <summary>
        /// AssetBundle所属的模块
        /// </summary>
        public BundleModuleEnum bundleModuleType;
        /// <summary>
        /// AssetBundle依赖项
        /// </summary>
        public List<string> bundleDependce;
        /// <summary>
        /// AssetBundle
        /// </summary>
        public AssetBundle assetBundle;
        /// <summary>
        /// 通过AssetBundle加载出的对象
        /// </summary>
        public UnityEngine.Object obj;
    }

    /// <summary>
    /// AssetBundle缓存
    /// </summary>
    public class AssetBundleCache
    {
        /// <summary>
        /// AssetBundle对象
        /// </summary>
        public AssetBundle assetBundle;
        /// <summary>
        /// AssetBundle引用计数
        /// </summary>
        public int referenceCount;

        public void Release()
        {
            assetBundle = null;
            referenceCount = 0;
        }
    }

    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        /// <summary>
        /// 已经加载的资源模块
        /// </summary>
        private List<BundleModuleEnum> mAlreadyLoadBundleModuleList = new List<BundleModuleEnum>();
        /// <summary>
        /// 所有模块的AssetBundle的资源对象字典
        /// </summary>
        private Dictionary<uint, BundleItem> mAllBundleAssetDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// 所有模块的已经加载过的AssetBundle的资源对象字典
        /// </summary>
        private Dictionary<string, AssetBundleCache> mAllAlreadyLoadBundleDic = new Dictionary<string, AssetBundleCache>();

        /// <summary>
        /// AssetBundle类对象池
        /// </summary>
        public ClassObjectPool<AssetBundleCache> mBundleCachePool = new ClassObjectPool<AssetBundleCache>(200);
        /// <summary>
        /// AssetBundle配置文件加载路径
        /// </summary>
        private string mBundleConfigPath;
        /// <summary>
        /// AssetBundle配置文件名称
        /// </summary>
        private string mBundleConfigName;
        /// <summary>
        /// AssetBundle配置文件名称
        /// </summary>
        private string mAssetsBundleConfigName;


        /// <summary>
        /// 生成AssetBundleConfig配置文件路径
        /// </summary>
        /// <param name="bundleModule">资源模块类型</param>
        /// <returns></returns>
        public bool GeneratorBundleConfigPath(BundleModuleEnum bundleModule)
        {
            //设置AssetBundle配置文件名称
            mAssetsBundleConfigName = bundleModule.ToString().ToLower() + "assetbundleconfig";
            //设置AssetBundle配置文件名称
            mBundleConfigName = bundleModule.ToString().ToLower() + "bundleconfig.unity";
            //设置AssetBundle配置文件路径，
            //配置路径在 持久化目录下：unity的持久化目录+ "/HotAssets/" + 资源模块类型 + 斜杠
            mBundleConfigPath = BundleSettings.Instance.GetHotAssetsPath(bundleModule) + mBundleConfigName;
            //如果配置文件 存在，return true，如果不存，我们就直接从内嵌解压的资源中去加载。
            //如果内嵌解压的文件中不存在，说明用户网络有问题
            if (!File.Exists(mBundleConfigPath))
            {
                //从内嵌解压的资源中加载配置文件——》从持久化目录中的加压文件目录下加载
                mBundleConfigPath = BundleSettings.Instance.GetAssetsDecompressPath(bundleModule) + mBundleConfigName;

                //如果不存在这个文件，说明该资源模块的资源（AB包）没有解压，或者没有内嵌也没有解压
                if (!File.Exists(mBundleConfigPath))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 加载AssetBundle配置文件
        /// </summary>
        /// <param name="bundleModule"></param>
        public void LoadAssetBundleConfig(BundleModuleEnum bundleModule)
        {
            try
            {
                //已经加载的资源模块字典里已经存在该资源模块，则直接返回
                if (mAlreadyLoadBundleModuleList.Contains(bundleModule))
                {
                    Debug.LogError("该模块配置文件已经加载："+bundleModule);
                    return;
                }
               
                //获取当前模块配置文件所在的路径
                if (GeneratorBundleConfigPath(bundleModule))
                {
                    AssetBundle bundleConfig = null;
                    //如果该AssetBundle已经加密，则需要解密
                    if (BundleSettings.Instance.bundleEncrypt.isEncrypt)
                    {
                        bundleConfig= AssetBundle.LoadFromMemory(AES.AESFileByteDecrypt(mBundleConfigPath,BundleSettings.Instance.bundleEncrypt.encryptKey));
                    }
                    else
                    {
                        //从本地文件系统加载AssetBundle
                        //参数是AssetBundle文件的完整路径
                        //返回加载的AssetBundle对象
                        //mBundleConfigPath 是AssetBundle配置文件的路径
                        //加载配置文件的AB包
                        //从持久化目录中加载
                        bundleConfig = AssetBundle.LoadFromFile(mBundleConfigPath);
                    }


                    //反序列化配置文件
                    //通过名称：mAssetsBundleConfigName
                    //在Ab包加载出这个资源
                    //返回加载的资源对象
                    //TextAsset 是UnityEngine.TextAsset类，用于存储文本数据
                    string bundleConfigJson = bundleConfig.LoadAsset<TextAsset>(mAssetsBundleConfigName).text;
                    //把bundleConfigJson反序列化成BundleConfig对象
                    BundleConfig bundleManife = JsonConvert.DeserializeObject<BundleConfig>(bundleConfigJson);
                    //把所有的AssetBundle信息存放至字典中，管理起来
                    foreach (var info in bundleManife.bundleInfoList)
                    {
                        if (!mAllBundleAssetDic.ContainsKey(info.crc))
                        {   
                            //创建BundleItem对象
                            BundleItem item = new BundleItem();
                            //设置文件路径
                            item.path = info.path;
                            //设置文件crc
                            item.crc = info.crc;
                            //设置资源模块类型
                            item.bundleModuleType = bundleModule;
                            //设置资源名称
                            item.assetName = info.assetName;
                            //设置依赖项
                            item.bundleDependce = info.bundleDependce;
                            //设置AssetBundle名称
                            item.bundleName = info.bundleName;
                            //把BundleItem对象添加到字典中
                            mAllBundleAssetDic.Add(item.crc, item);
                        }
                        else
                        {
                            Debug.LogError("AssetBundle Already Exists! BundleName:" + info.bundleName);
                        }
                    }
                    //释放AssetBunle配置
                    bundleConfig.Unload(false);
                    mAlreadyLoadBundleModuleList.Add(bundleModule);
                }
                else
                {
                    Debug.LogError("AssetBundleConfig Not find.  Load AssetBundle failed!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Load AssetBundleConfig Failed, Exception:"+e);
            }
          
        }
        /// <summary>
        /// 根据AssetBundle名称查询该AssetBUndle中都有那些资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<BundleItem> GetBundleItemByABName(string bundleName)
        {
            List<BundleItem> itemList = new List<BundleItem>();
            //遍历所有所有模块的AssetBundle的资源对象字典的值
            foreach (var item in mAllBundleAssetDic.Values)
            {
                //如果item的bundleName与传入的bundleName一致
                if (string.Equals(item.bundleName,bundleName))
                {
                    //将item添加到itemList中
                    itemList.Add(item);
                }
            }
            //返回itemList
            return itemList;
        }
        /// <summary>
        /// 通过资源路径的Crc加载该资源所在AssetBundle
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public BundleItem LoadAssetBundle(uint crc)
        {
            BundleItem item = null;

            //先到所有的AssetBunel资源字典中查询一下这个资源存不存在，如果存在说明该资源已经打成了AssetBundle包，这种情况下就可以直接加载了
            //如果不存在，则说明该资源 不属于AssetBUnle 给与错误提示。
            mAllBundleAssetDic.TryGetValue(crc, out item);

            if (item!=null)
            {
                //如果AssetBundle为空，说明该资源所在的AssetBundle没有加载进内存，这种情况我们就需要加载该AssetBundle
                if (item.assetBundle==null)
                {
                    item.assetBundle = LoadAssetBundle(item.bundleName,item.bundleModuleType);
                    //需要加载这个AssetBundle依赖的其他的AssetBundle
                    foreach (var bundleName in item.bundleDependce)
                    {
                        if (item.bundleName!=bundleName)
                        {
                            LoadAssetBundle(bundleName, item.bundleModuleType);
                        }
                    }
                    return item;
                }
                else
                {
                    return item;
                }
            }
            else
            {
                Debug.LogError("assets not exists AssetbundleConfig , LoadAssetBundle failed! Crc:"+crc);
                return null;
            }
        }
        /// <summary>
        /// 通过AssetBundle Name加载AssetBundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="bundleModuleType"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string bundleName, BundleModuleEnum bundleModuleType)
        {
            //声明一个AssetBundleCache类对象
            AssetBundleCache bundle = null;
            //从已经加载过的AssetBundle字典中获取该AssetBundle
            mAllAlreadyLoadBundleDic.TryGetValue(bundleName,out bundle);

            //如果bundle为空，或者bundle不为空，但是bundle的assetBundle为空
            if (bundle==null||(bundle!=null&&bundle.assetBundle==null))
            {
                //从类对象池中取出一个AssetBundleCache
                bundle= mBundleCachePool.Spawn();
                //计算出AssetBundle加载路径  ——————unity的持久化目录+ "/HotAssets/" + 资源模块类型 + 斜杠 + AssetBundle名称
                string hotFilePath = BundleSettings.Instance.GetHotAssetsPath(bundleModuleType)+bundleName;
                //获取热更模块
                //这个通过框架调用了 hotassetsmanager 的 gethotassetsmodule 方法,
                //如果所有热更资源模块字典中包含资源模块类型，则返回热更资源模块，否则返回null
                HotAssetsModule module= ZMAssetsFrame.GetHotAssetsModule(bundleModuleType);
                //如果model为空，检查持久化目录下的hotassets路径下是否存在该AssetBundle，存在则返回true，否则返回false
                //如果model不为空，检查该资源模块的所有热更的资源列表是否等于零，
                //如果等于零，检查持久化目录下的hotassets路径下是否存在该AssetBundle，存在则返回true，否则返回false
                //如果不等于零，将包名传入到热更模块的HotAssetsIsExists方法中，遍历所有热更的资源列表
                //检查该资源是否存在，存在则返回true，否则返回false
                bool isHotPath = module==null?(File.Exists(hotFilePath)?true:false):
                                              (module.HotAssetCount==0?(File.Exists(hotFilePath) ? true : false):
                                                                        module.HotAssetsIsExists(bundleName));
                //通过是否是热更路径 计算出AssetBundle加载的路径
                //如果isHotPath为true，则说明该资源是热更资源，则加载路径为热更路径，否则为解压路径
                string bundlePath = isHotPath ? hotFilePath : BundleSettings.Instance.GetAssetsDecompressPath(bundleModuleType) + bundleName;

                //判断AssetBUndle是否加密，如果加密了，则需要解密
                if (BundleSettings.Instance.bundleEncrypt.isEncrypt)
                {
                    byte[] bytes= AES.AESFileByteDecrypt(bundlePath, BundleSettings.Instance.bundleEncrypt.encryptKey);
                    bundle.assetBundle= AssetBundle.LoadFromMemory(bytes);
                }
                else
                {
                    //通过LoadFromFile 加载AssetBundle 是最快的
                    bundle.assetBundle = AssetBundle.LoadFromFile(bundlePath);
                }
                if (bundle.assetBundle==null)
                {
                    Debug.LogError("AssetBundle load failed bundlePath:"+ bundlePath);
                    return null;
                }
                //AssetBundle引用计数增加
                bundle.referenceCount++;
                mAllAlreadyLoadBundleDic.Add(bundleName,bundle);
            }
            else
            {
                //AssetBunle已经加载过了
                bundle.referenceCount++;
            }
            return bundle.assetBundle;
        }


        /// <summary>
        /// 释放AssetBundle 并且释放AssetBundle占用的内存资源
        /// </summary>
        /// <param name="assetitem"></param>
        /// <param name="unLoad"></param>
        public void ReleaseAssets(BundleItem assetitem,bool unLoad)
        {
            //AssetBUndle释放策略一般有两种
            //1.第一种：
            // 以AssetBundle.UnLoad(false) 为主
            // 对于非对象资源，比如 text texture audio等 ,资源加载完成后，就可以直接通过AssetBundle.UnLoad(false)释放AssetBundle的镜像文件
            // 对于对象资源 比如Gameobject 我们需要在上层做一个引用计数的对象池，obj在加载出来之后就可以使用AssetBundle.UnLoad(false)释放AssetBundle的镜像文件
            // 因为后续我们访问的对象都是对象池中的物体了

            //2.第二种
            //以AssetBundle.UnLoad(true) 为主
            // 在加载AssetBundle 时做一个缓存，后续加载的所有的资源对象全部通过缓存的AssetBUndle进行加载
            // 在跳转场景的时候 通过 AssetBundle.UnLoad(true) 彻底释放所有的资源与内存占用

            //AssetBundle assetBundle = null;
            if (assetitem!=null)
            {
                if (assetitem.obj!=null)
                {
                    assetitem.obj = null;
                }

                ReleaseAssetBundle(assetitem,unLoad);

                if (assetitem.bundleDependce!=null)
                {
                    foreach (var bundleName in assetitem.bundleDependce)
                    {
                        //根据内存引用计数释放AssetBundle
                        ReleaseAssetBundle(null,unLoad, bundleName);
                    }
                }
            }
            else
            {
                Debug.LogError(" assetitem is null, release Assets failed!");
            }
        }
        /// <summary>
        /// 释放AssetBundle所占用的资源
        /// </summary>
        /// <param name="assetitem"></param>
        /// <param name="unLoad"></param>
        /// <param name="bundleName"></param>
        public void ReleaseAssetBundle(BundleItem assetitem, bool unLoad,string bundleName="")
        {
            string assetBudnleName = "";
            if (assetitem == null)
            {
                assetBudnleName = bundleName;
            }
            else
            {
                assetBudnleName = assetitem.bundleName;
            }
            AssetBundleCache bundleCacheItem = null;
            //如果该AssetBUndle的名字不为空，与我们的这个AssetBundle已经加载过了
            if (!string.IsNullOrEmpty(assetBudnleName) && mAllAlreadyLoadBundleDic.TryGetValue(assetBudnleName, out bundleCacheItem))
            {
                if (bundleCacheItem.assetBundle != null)
                {
                    bundleCacheItem.referenceCount--;
                    //如果该AssetBundle内存引用小于等于0 就说明没有人引用了，就可以直接释放了
                    if (bundleCacheItem.referenceCount <= 0)
                    {
                        bundleCacheItem.assetBundle.Unload(unLoad);
                        mAllAlreadyLoadBundleDic.Remove(assetBudnleName);
                        //回收BundleCacheitem类对象
                        bundleCacheItem.Release();
                        mBundleCachePool.Recycl(bundleCacheItem);
                    }
                }
            }
        }
    }

}
