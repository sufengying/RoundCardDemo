
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ZM.AssetFrameWork
{
    public class AssetsDecompressManager : IDecompressAssets
    {
        /// <summary>
        /// 资源解压路径
        /// </summary>
        private string mDecompressPath;
        /// <summary>
        /// 资源内嵌路径
        /// </summary>
        private string mStreamingAssetsBundlePath;
        /// <summary>
        /// 需要解压的资源列表
        /// </summary>
        private List<string> mNeedDecompressFileList = new List<string>();

        /// <summary>
        /// 开始解压内嵌文件（从unity的streamingAssetsPath路径下读取文件，写入到persistentDataPath路径下）
        /// </summary>
        /// <param name="bundleModule">解压资源模块</param>
        /// <param name="callBack">解压完成回调</param>
        /// <returns></returns>
        public override IDecompressAssets StartDeCompressBuiltinFile(BundleModuleEnum bundleModule, Action callBack)
        {
            //计算需要解压的文件，并返回是否需要解压
            //如果不是安卓或者IOS，则不需要解压
            if (ComputeDecompressFile(bundleModule))
            {
                //设置开始解压
                IsStartDecompress = true;
                //开始解压
                ZMAssetsFrame.Instance.StartCoroutine(UnPackToPersistentDataPath(bundleModule, callBack));
            }
            else                    
            {
                //如果不需要解压文件，则直接回调
                Debug.Log("不需要解压文件");
                callBack?.Invoke();
            }
            return this;
        }
        /// <summary>
        /// 计算需要解压的文件，并返回是否需要解压
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <returns></returns>
        private bool ComputeDecompressFile(BundleModuleEnum bundleModule)
        {
            //获取资源内嵌路径（在streamingAssetsPath路径下，可读不可写）
            mStreamingAssetsBundlePath = BundleSettings.Instance.GetAssetsBuiltinBundlePath(bundleModule);
            //获取资源解压路径（在persistentDataPath路径下，可读可写）
            mDecompressPath=BundleSettings.Instance.GetAssetsDecompressPath(bundleModule);
            //清空需要解压的文件列表
            mNeedDecompressFileList.Clear();

            //如果平台是安卓或者IOS
#if UNITY_ANDROID||UNITY_ISO
            //如果文件夹不存在，就进行创建一个解压资源路径
            if (!Directory.Exists(mDecompressPath))
            {
                Directory.CreateDirectory(mDecompressPath);
            }

            //计算需要解压的文件，以及大小
            //从Resources资源中加载一个TextAsset，文件名是资源模块类型+info
            TextAsset textAsset= Resources.Load<TextAsset>(bundleModule+"info");

            //如果textAsset为空，说明该资源模块的资源还没有内嵌，所以内嵌资源清单还没有生成
            if (textAsset!=null)
            {
                //声明一个存储内嵌的AssetBundle的信息的列表
                //将textAsset.text反序列化成列表
                List<BuiltinBundleInfo> builtinBundleInfoList= JsonConvert.DeserializeObject<List<BuiltinBundleInfo>>(textAsset.text);
                //遍历列表中的所有内嵌的AssetBundle的信息
                foreach (var info in builtinBundleInfoList)
                {
                    //设置本地文件储存路径（持久化目录）
                    string localFilePath = mDecompressPath + info.fileName;
                    //如果文件是meta文件，则跳过
                    //为什么有.meta文件？
                    //因为unity在打包的时候，会生成一个.meta文件，这个文件是用来存储文件的元数据的，比如文件的MD5值，文件的大小，文件的类型等
                    //.meta文件是unity在打包的时候生成的，所以需要跳过

                    //因为localFilePath = mDecompressPath + info.fileName；
                    //localFilePath 有.meta后缀是因为info.fileName
                    if (localFilePath.EndsWith(".meta"))
                    {
                        continue;
                    }
                    //计算出需要解压的文件
                    if (!File.Exists(localFilePath)||MD5.GetMd5FromFile(localFilePath)!=info.md5)
                    {
                        //将需要解压的文件添加到列表中
                        mNeedDecompressFileList.Add(info.fileName);
                        //更新需要解压的大小
                        TotalSizem += info.size / 1024f;
                    }
                }
            }
            else
            {
                Debug.LogError(bundleModule + "info"+" 不存在，请检查内嵌资源 是否内嵌！");
            }
            //如果需要解压的文件列表不为空，则返回true
            //如果需要解压的文件列表为空，说明不需要解压，则返回false
            return mNeedDecompressFileList.Count > 0;
#else
            return false;
#endif
        }
        public override float GetDecompressProgress()
        {
            //返回一个解压进度，解压进度=已经解压的大小/需要解压的大小
            return AlreadyDecompressSizem/TotalSizem;
        }
        /// <summary>
        /// 解压文件到持久化目录（从unity的streamingAssetsPath路径下读取文件，写入到persistentDataPath路径下）
        /// </summary>
        /// <param name="bundleModule">资源模块类型</param>
        /// <param name="callBack">解压完成回调</param>
        /// <returns></returns>
        IEnumerator UnPackToPersistentDataPath(BundleModuleEnum bundleModule,Action callBack)
        {
            //遍历需要解压的文件列表
            foreach (var fileName in mNeedDecompressFileList)
            {
                //声明一个字符串，用于存储文件路径
                string filePath = "";
                //如果平台是macOS或者IOS
#if UNITY_EDITOR_OSX || UNITY_IOS
                //文件路径为：file:// + 资源内嵌路径 + 文件名
                filePath = "file://" + mStreamingAssetsBundlePath + fileName;
#else
                //文件路径为：资源内嵌路径 （在streamingAssetsPath路径下，可读不可写）+ 文件名
                //mStreamingAssetsBundlePath: 资源内嵌路径（在streamingAssetsPath+“/AssetBundle/”+资源模块类型+“/”）
                //实际上就是资源文件的路径
                filePath =  mStreamingAssetsBundlePath + fileName;
#endif
                Debug.Log("Start UnPack AssetBundle filePath:"+filePath+"\r\n UnPackPath:"+mDecompressPath);
                //通过 UnityWebRequest(Http) 访问本地文件 ，这个过程是不消耗流量的，相当于直接读取，所以速度是非常快的
                //读取文件->在streamingAssetsPath路径下的文件
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(filePath);
                //设置超时时间
                unityWebRequest.timeout = 30;
                //发送请求
                yield return unityWebRequest.SendWebRequest();

                if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("UnPack Error:"+unityWebRequest.error);
                }
                else
                {
                    //到了这一步，文件就已经读取完成了
                    byte[] bytes= unityWebRequest.downloadHandler.data;
                    //将文件写入到持久化目录
                    FileHelper.WriteFile(mDecompressPath+fileName,bytes);
                    //更新已经解压的大小
                    AlreadyDecompressSizem += (bytes.Length / 1024f) / 1024f;
                    Debug.Log("AlreadyDecompressSizem:"+AlreadyDecompressSizem +" totalSize:"+TotalSizem);
                    Debug.Log("UnPack Finish "+mDecompressPath+fileName);
                }
                //释放资源
                unityWebRequest.Dispose();
            }
            //解压完成 回调
            callBack?.Invoke();
            //设置开始解压为false
            IsStartDecompress = false;
        }
    }
}
