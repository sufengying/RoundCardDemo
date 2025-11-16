
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 资源下载线程
    /// </summary>
    public class DownLoadThread
    {
        /// <summary>
        /// 下载完成回调，接收下载线程和热更文件信息（一个模块中的AB包）
        /// </summary>
        private Action<DownLoadThread, HotFileInfo> OnDownLoadSuccess;
        /// <summary>
        /// 下载失败回调，接收下载线程和热更文件信息（即一个模块中的AB包）
        /// </summary>
        public Action<DownLoadThread, HotFileInfo> OnDownLoadFailed;

        /// <summary>
        /// 当前热更的资源模块
        /// </summary>
        private HotAssetsModule mCurHotAssetsModule;
        /// <summary>
        /// 当前热更的文件信息（ab包）
        /// </summary>
        private HotFileInfo mHotFileInfo;
        /// <summary>
        /// 文件下载的地址
        /// </summary>
        private string mDownLoadUrl;
        /// <summary>
        /// 下载下的文件储存的地址
        /// </summary>
        private string mFileSavePath;
        /// <summary>
        /// 下载的大小
        /// </summary>
        private float mDownLoadSizeKB;
        /// <summary>
        /// 当前下载的次数
        /// </summary>
        private int curDownLoadCount;
        /// <summary>
        /// 最大尝试下载次数
        /// </summary>
        private const int MAX_TRY_DOWNLOAD_COUNT = 3;
        /// <summary>
        /// 资源下载线程
        /// </summary>
        /// <param name="assetsModule">资源所属模块</param>
        /// <param name="hotFileInfo">需要下载热更的资源</param>
        /// <param name="downLoadUrl">资源下载地址</param>
        /// <param name="fileSavePath">文件储存地址</param>
        public DownLoadThread(HotAssetsModule assetsModule, HotFileInfo hotFileInfo, string downLoadUrl, string fileSavePath)
        {
            //设置当前热更的资源模块
            this.mCurHotAssetsModule = assetsModule;
            //设置当前热更的文件信息
            this.mHotFileInfo = hotFileInfo;
            //设置文件储存地址
            this.mFileSavePath = fileSavePath + "/" + hotFileInfo.abName;
            //设置下载地址
            this.mDownLoadUrl = downLoadUrl + "/" + hotFileInfo.abName;
        }
        /// <summary>
        /// 开始通过子线程下载资源
        /// </summary>
        /// <param name="downLoadSuccess">下载成功回调</param>
        /// <param name="downLoadFailed">下载失败回调</param>
        public void StartDownLoad(Action<DownLoadThread, HotFileInfo> downLoadSuccess, Action<DownLoadThread, HotFileInfo> downLoadFailed)
        {
            //当前下载次数+1
            curDownLoadCount++;
            //设置下载成功回调
            OnDownLoadSuccess = downLoadSuccess;
            //设置下载失败回调
            OnDownLoadFailed = downLoadFailed;
            //在线程池中异步执行代码
            //创建一个新的后台任务
            //不会阻塞主线程
            Task.Run(() =>
            {
                //这里的代码在子线程中执行
                try
                {
                    Debug.Log("StartDownLoad ModuelEnum:" + mCurHotAssetsModule.CurBundleModuleEnum + " AssetBundle URL:" + mDownLoadUrl);
                    // 1. 创建HTTP请求
                    // 这行代码创建了一个HTTP请求对象
                    // WebRequest.Create() 创建一个WebRequest对象
                    // as HttpWebRequest 将WebRequest转换为HttpWebRequest类型
                    // mDownLoadUrl 是下载地址，比如 "http://example.com/file.unity"
                    HttpWebRequest request = WebRequest.Create(mDownLoadUrl) as HttpWebRequest;

                    // 2. 设置请求方法
                    // 设置HTTP请求方法为GET
                    // GET方法用于从服务器获取资源
                    request.Method = "GET";

                    // 3. 发送请求并获取响应
                    // 发送HTTP请求并等待服务器响应
                    // GetResponse() 会阻塞直到收到服务器响应
                    // as HttpWebResponse 将响应转换为HttpWebResponse类型
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                    // 4. 创建本地文件流
                    // 创建一个新的文件用于保存下载的内容
                    // File.Create() 会创建新文件，如果文件已存在则覆盖
                    // mFileSavePath 是保存路径，比如 "D:/Game/Assets/file.unity"
                    FileStream fileStream = File.Create(mFileSavePath);

                    // 5. 获取响应流
                    // 获取HTTP响应的流
                    // GetResponseStream() 返回一个用于读取响应数据的流
                    using (var stream = response.GetResponseStream())
                    {
                        // //文件下载异常
                        // if (stream.Length==0)
                        // {
                        //     Debug.LogError("File DownLoad exception plase check file fileName:"+mHotFileInfo.abName +" fileUrl:"+mDownLoadUrl);
                        // }
                        // 6. 读取响应流
                        //创建一个缓冲区，用于存储从流中读取的数据
                        // 缓冲区大小为512字节
                        byte[] buffer = new byte[512]; //512 

                        //从字节流中读取字节，读取到buff数组中
                        // 从输入流（stream）中读取最多512字节的数据，存到buffer数组里
                        // 0表示从buffer的起始位置开始写入
                        // buffer.Length表示最多读取buffer这么多字节（即512）
                        // 返回值size表示实际读取到的字节数（可能小于512，比如最后一次读取）
                        // 如果读取到流的末尾，size会等于0
                        int size = stream.Read(buffer, 0, buffer.Length); //700

                        while (size > 0)
                        {
                            //将“buffer”数组中从索引“0”开始、“size”个字节的数据，写入到“fileStream”所代表的本地文件流中。
                            fileStream.Write(buffer, 0, size);
                            //从流中读取数据到缓冲区
                            size = stream.Read(buffer, 0, buffer.Length);
                            //1mb=1024kb 1kb=1024字节
                            mDownLoadSizeKB += size;
                            //计算以m为单位的大小
                            //在当前的热更资源模块中，记录已经下载的大小
                            mCurHotAssetsModule.AssetsDownLoadSizeM += ((size / 1024.0f) / 1024.0f);

                        }
                        // 调用“Dispose”方法（或“using”语句）会释放“fileStream”所占用的非托管资源（例如文件句柄、缓冲区等），
                        // 释放后，fileStream将无法再用于读取或写入文件
                        fileStream.Dispose();
                        // “fileStream.Close()”则显式调用“Close”方法，关闭文件流，并释放相关资源。
                        fileStream.Close();
                        //mCurHotAssetsModule.CurBundleModuleEnum 当前热更的资源模块
                        //mDownLoadUrl 资源下载地址
                        //mFileSavePath 文件储存地址
                        Debug.Log("OnDownLoadSuccess ModuleEnum:" + mCurHotAssetsModule.CurBundleModuleEnum + " AssetBundleUrl:" + mDownLoadUrl
                            + " FileSavePath:" + mFileSavePath);
                        //调用下载成功回调
                        OnDownLoadSuccess?.Invoke(this, mHotFileInfo);

                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("DownLoad AssetBundle Error Url:" + mDownLoadUrl + " Exception:" + e);
                    //如果下载次数大于最大尝试下载次数，就调用下载失败回调
                    if (curDownLoadCount > MAX_TRY_DOWNLOAD_COUNT)
                    {
                        OnDownLoadFailed?.Invoke(this, mHotFileInfo);
                    }
                    else
                    {
                        //如果下载失败，就进行重新下载
                        Debug.LogError("文件下载失败，正在进行重新下载，下载次数" + curDownLoadCount);
                        //递归调用StartDownLoad方法，进行重新下载
                        //并且把下载次数+1，如果下载次数大于最大尝试下载次数，就调用下载失败回调
                        StartDownLoad(OnDownLoadSuccess, OnDownLoadFailed);
                    }
                    throw;
                }

            });
        }
    }
}
