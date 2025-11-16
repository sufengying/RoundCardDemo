
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    public class DownLoadEventHandler
    {
        public DownLoadEvent downLoadEvent;//回调
        public HotFileInfo hotfileInfo;
    }

    /// <summary>
    /// 下载事件
    /// </summary>
    /// <param name="hotFile"></param>
    public delegate void DownLoadEvent(HotFileInfo hotFile);
    /// <summary>
    /// 多线程资源下载器
    /// </summary>
    public class AssetsDownLoader
    {
        /// <summary>
        /// 最大下载线程个数
        /// </summary>
        public int MAX_THREAD_COUNT = 3;
        /// <summary>
        /// 资源文件下载地址
        /// </summary>
        private string mAssetsDownLoadUrl;
        /// <summary>
        /// 热更文件储存路径
        /// </summary>
        private string mHotAssetsSavePath;
        /// <summary>
        /// 当前热更的资源模块
        /// </summary>
        private HotAssetsModule mCurHotAssetsModule;
        /// <summary>
        /// 文件下载队列
        /// </summary>
        private Queue<HotFileInfo> mDownLoadQueue;
        /// <summary>
        /// 文件下载成功回调
        /// </summary>
        private DownLoadEvent OnDownLoadSuccess;
        /// <summary>
        /// 文件下载失败回调
        /// </summary>
        private DownLoadEvent OnDownLoadFailed;
        /// <summary>
        /// 所有文件下载完成的回调
        /// </summary>
        private DownLoadEvent OnDownLoadFinish;
        /// <summary>
        /// 下载回调的列表
        /// </summary>
        private Queue<DownLoadEventHandler> mDownLoadEventQueue = new Queue<DownLoadEventHandler>();
        /// <summary>
        /// 当前所有正在下载的线程列表
        /// </summary>
        private List<DownLoadThread> mAllDownLoadThreadList = new List<DownLoadThread>();

        /// <summary>
        /// 资源下载器
        /// </summary>
        /// <param name="assetModule">资源下载模块</param>
        /// <param name="downLoadQueue">资源下载队列</param>
        /// <param name="downloadUrl">资源下载地址</param>
        /// <param name="hotAssetsSavePath">热更文件储存路径</param>
        /// <param name="downLoadSuccess">文件下载成功回调</param>
        /// <param name="downLoadFailed">文件下载失败或出错的回调</param>
        /// <param name="downLoadFinish">所有文件下载完成的回调</param>
        public AssetsDownLoader(HotAssetsModule assetModule, Queue<HotFileInfo> downLoadQueue, string downloadUrl, string hotAssetsSavePath,
            DownLoadEvent downLoadSuccess, DownLoadEvent downLoadFailed, DownLoadEvent downLoadFinish)
        {
            this.mCurHotAssetsModule = assetModule;
            this.mDownLoadQueue = downLoadQueue;
            this.mAssetsDownLoadUrl = downloadUrl;
            this.mHotAssetsSavePath = hotAssetsSavePath;
            this.OnDownLoadSuccess = downLoadSuccess;
            this.OnDownLoadFailed = downLoadFailed;
            this.OnDownLoadFinish = downLoadFinish;
        }

        public void StartThreadDownLoadQueue()
        {
            //根据最大的线程下载个数，开启基本下载通道
            for (int i = 0; i < MAX_THREAD_COUNT; i++)
            {
                //如果下载队列中还有文件（队列不为空），就开启下载
                if (mDownLoadQueue.Count > 0)
                {
                    Debug.Log("Start DownLoad AssetBundle MAX_THREAD_COUNT:" + MAX_THREAD_COUNT);
                    StartDownLoadNextBundle();
                }
            }
        }
        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        public void StartDownLoadNextBundle()
        {
            HotFileInfo hotFileInfo = mDownLoadQueue.Dequeue();
            //传入参数
            //mCurHotAssetsModule 当前热更的资源模块
            //hotFileInfo 当前热更的文件信息（ab包）从队列取出一个
            //mAssetsDownLoadUrl 资源下载地址
            //mHotAssetsSavePath 热更文件储存路径
            DownLoadThread downLoadItem = new DownLoadThread(mCurHotAssetsModule, hotFileInfo, mAssetsDownLoadUrl, mHotAssetsSavePath);

            //开始下载，异步执行，不会阻塞主线程
            //设置下载成功回调
            //DownLoadSuccess 下载成功回调，接收  DownLoadThread  和  HotFileInfo参数
            //DownLoadFailed 下载失败回调，接收DownLoadThread和HotFileInfo参数
            //即创建出来的DownLoadThread对象 和 传入DownLoadThread对象进行下载的热更新文件 ，这个方法会在DownLoadThread对象中调用
            downLoadItem.StartDownLoad(DownLoadSuccess, DownLoadFailed);
            //把下载线程添加到列表中
            mAllDownLoadThreadList.Add(downLoadItem);
        }
        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        public void DownLoadNextBundle()
        {
            //如果当前下载的线程个数，大于最大的限制个数，我们就关闭当前下载通道（就不执行这个函数）
            //mAllDownLoadThreadList 当前正在下载的线程列表
            if (mAllDownLoadThreadList.Count > MAX_THREAD_COUNT)
            {
                Debug.Log("DownLoadNextBundle Out MaxThreadCount,Close this DownLoad Channel...");
                return;
            }
            //如果下载队列中还有文件（队列不为空），就开启下载
            if (mDownLoadQueue.Count > 0)
            {
                //开启下载
                //这个方法会创建一个DownLoadThread对象，并开始下载一个热更新文件，
                //并把这个DownLoadThread对象添加到mAllDownLoadThreadList列表中
                StartDownLoadNextBundle();
                //如果当前下载的线程个数，小于最大的限制个数，我们就开启更多的下载通道
                if (mAllDownLoadThreadList.Count < MAX_THREAD_COUNT)
                {
                    //计算出正在待机的线程下载通道，把这些下载通道全部打开
                    int idleThreadCount = MAX_THREAD_COUNT - mAllDownLoadThreadList.Count;
                    for (int i = 0; i < idleThreadCount; i++)
                    {
                        if (mDownLoadQueue.Count > 0)
                        {
                            //开启下载
                            //这个方法会创建一个DownLoadThread对象，并开始下载一个热更新文件，
                            //并把这个DownLoadThread对象添加到mAllDownLoadThreadList列表中
                            StartDownLoadNextBundle();
                        }
                    }
                }
            }
            else
            {
                //如果下载中的文件也没有了，就说明我们所有文件都下载成功了
                if (mAllDownLoadThreadList.Count == 0)
                {
                    //触发所有文件下载完成的回调
                    //OnDownLoadFinish 是所有文件下载完成的回调，接收一个HotFileInfo对象
                    //TriggerCallBackInMainThread方法确保回调在主线程中执行
                    //这样可以安全地执行Unity相关的操作
                    TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = OnDownLoadFinish });
                }
            }
        }
        /// <summary>
        /// AssetBundle文件下载成功
        /// </summary>
        /// <param name="downLoadThread"></param>
        /// <param name="hotFileInfo"></param>
        public void DownLoadSuccess(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            //从当前正在下载的线程列表中移除该下载线程
            RemoveDownLoadThread(downLoadThread);
            //因为我们的文件 是在子线程中进行下载，所以说回调也是在子线程中触发。
            //我们要做的事情，就是把回调放到主线程中去调用。

            //Unity的API（包括UI操作、GameObject操作等）必须在主线程中执行
            //下载操作是在后台线程中进行的
            //当下载完成时，回调函数可能包含需要在主线程执行的Unity操作

            //DownLoadEventHandler是一个包含回调信息和文件信息的结构


            //DownLoadEventHandler 是一个拥有 无返回值接收一个HotFileInfo对象的委托，和一个HotFileInfo变量的数据结构类
            //OnDownLoadSuccess 是下载成功回调，接收一个HotFileInfo对象
            //这个函数把DownLoadEventHandler对象添加到mDownLoadEventQueue队列中
            //mDownLoadEventQueue 是文件下载队列，用于存储需要执行的回调函数
           
           
            TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = OnDownLoadSuccess, hotfileInfo = hotFileInfo });


            //下载下一个AssetBundle
            DownLoadNextBundle();
        }
        /// <summary>
        /// AssetBundle文件下载失败
        /// </summary>
        /// <param name="downLoadThread"></param>
        /// <param name="hotFileInfo"></param>
        public void DownLoadFailed(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            //下载失败，也移除下载线程
            RemoveDownLoadThread(downLoadThread);
            //触发下载失败回调
            //OnDownLoadFailed 是下载失败回调，接收一个HotFileInfo对象
            //这样可以安全地执行Unity相关的操作
            TriggerCallBackInMainThread(new DownLoadEventHandler { downLoadEvent = OnDownLoadFailed, hotfileInfo = hotFileInfo });
            //下载下一个AssetBundle
            DownLoadNextBundle();
        }

        /// <summary>
        /// 在主线程中触发回调
        /// </summary>
        /// <param name="downLoadEventHandler"></param>
        public void TriggerCallBackInMainThread(DownLoadEventHandler downLoadEventHandler)
        {
            //锁定mDownLoadEventQueue，确保线程安全
            lock (mDownLoadEventQueue)
            {
                //把downLoadEventHandler添加到mDownLoadEventQueue队列中
                mDownLoadEventQueue.Enqueue(downLoadEventHandler);
            }
        }
        /// <summary>
        /// 主线程更新接口
        /// </summary>
        public void OnMainThreadUpdate()
        {
            //如果mDownLoadEventQueue队列中还有元素（队列不为空），就执行回调
            if (mDownLoadEventQueue.Count > 0)
            {
                //从mDownLoadEventQueue队列中获取DownLoadEventHandler对象
                DownLoadEventHandler downLoadEventHandler = mDownLoadEventQueue.Dequeue();
                //执行回调，每一个DownLoadEventHandler对象都有一个downLoadEvent回调函数
                //每一个热更新文件进行下载，无论下载是否成功，都会把回调函数和热更新文件信息传递给downLoadEventHandler
                //然后在update中执行 downLoadEventHandler的downLoadEvent回调函数（委托）

                //执行的就是通过 HotAssetsModule的StartDownLoadHotAssets方法中，创建的AssetsDownLoader类对象构造函数所传入的
                //downLoadSuccess 下载成功回调，接收  DownLoadThread  和  HotFileInfo参数
                //downLoadFailed 下载失败回调，接收DownLoadThread和HotFileInfo参数

                //也就是说下载器里缓存的回调函数的定义，在AssetsDownLoader类中，
                //在HotAssetsModule类中，调用StartDownLoadHotAssets方法时，会创建AssetsDownLoader类对象，
                //并把回调函数和热更新文件信息传递给downLoadEventHandler
                //然后update中执行 downLoadEventHandler的downLoadEvent回调函数（委托）
                //OnMainThreadUpdate()方法在HotAssetsModule类中调用，
                //在update中执行 downLoadEventHandler的downLoadEvent回调函数（委托）
                downLoadEventHandler.downLoadEvent?.Invoke(downLoadEventHandler.hotfileInfo);
            }
        }
        /// <summary>
        /// 移除下载线程
        /// </summary>
        /// <param name="downLoadThread"></param>
        public void RemoveDownLoadThread(DownLoadThread downLoadThread)
        {
            //如果当前正在下载的线程列表中包含这个下载线程
            if (mAllDownLoadThreadList.Contains(downLoadThread))
            {
                //移除这个下载线程，因为已经下载成功了
                mAllDownLoadThreadList.Remove(downLoadThread);
            }
        }
    }
}
