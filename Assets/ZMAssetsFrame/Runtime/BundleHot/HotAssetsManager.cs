
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 等待下载的模块
    /// </summary>
    public class WaitDownLoadModule
    {
        /// <summary>
        /// 资源模块类型
        /// </summary>
        public BundleModuleEnum bundleModule;
        /// <summary>
        /// 开始热更回调
        /// </summary>
        public Action<BundleModuleEnum> startHot;
        /// <summary>
        /// 热更完成回调
        /// </summary>
        public Action<BundleModuleEnum> hotFinish;
        /// <summary>
        /// 热更进度回调
        /// </summary>
        public Action<BundleModuleEnum, float> hotAssetsProgressCallBack;
    }

    public class HotAssetsManager : IHotAssets
    {
        /// <summary>
        /// 最大并发下载线程个数
        /// </summary>
        private int MAX_THREAD_COUNT = 3;
        /// <summary>
        /// 所有热更资源模块
        /// key：资源模块类型
        /// value：热更资源模块
        /// </summary>
        private Dictionary<BundleModuleEnum, HotAssetsModule> mAllAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();

        /// <summary>
        /// 正在下载热更资源模块的字典
        /// key：资源模块类型
        /// value：热更资源模块
        /// </summary>
        private Dictionary<BundleModuleEnum, HotAssetsModule> mDownLoadingAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();
        /// <summary>
        /// 正在下载热更资源的列表
        /// </summary>
        private List<HotAssetsModule> mDownLoadAssetsModuleList = new List<HotAssetsModule>();
        /// <summary>
        /// 等待下载的队列
        /// </summary>
        private Queue<WaitDownLoadModule> mWaitDownLoadQueue = new Queue<WaitDownLoadModule>();
        /// <summary>
        /// 下载AssetBundle完成
        /// </summary>
        public static Action<HotFileInfo> DownLoadBundleFinish;

        /// <summary>
        /// 热更资源
        /// </summary>
        /// <param name="bundleModule">资源模块类型</param>
        /// <param name="startHotCallBack">开始热更回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载回调</param>
        /// <param name="isCheckAssetsVersion">是否检测资源版本</param>
        public void HotAssets(BundleModuleEnum bundleModule, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true)
        {
            //如果热更类型为不热更
            if (BundleSettings.Instance.bundleHotType==  BundleHotEnum.NoHot)
            {
                //调用热更完成回调
                hotFinish?.Invoke(bundleModule);
                return;
            }

            //读取配置中的最大下载线程个数
            MAX_THREAD_COUNT = BundleSettings.Instance.MAX_THREAD_COUNT;

            //从字典获取资源模块，或者创建一个资源模块并添加到字典
            HotAssetsModule assetsModule = GetOrNewAssetModule(bundleModule);
            //判断是否有闲置资源下载线程
            //如果正在下载热更资源模块的字典中的数量小于最大下载线程个数
            if (mDownLoadingAssetsModuleDic.Count<MAX_THREAD_COUNT)
            {
                //如果正在下载热更资源的字典中不包含资源模块类型
                if (!mDownLoadingAssetsModuleDic.ContainsKey(bundleModule))
                {
                    //把热更资源模块添加到正在下载热更资源的字典中
                    //key：资源模块类型
                    //value：热更资源模块
                    mDownLoadingAssetsModuleDic.Add(bundleModule,assetsModule);
                }
                //如果正在下载热更资源的列表中不包含热更资源模块
                if (!mDownLoadAssetsModuleList.Contains(assetsModule))
                {
                    //把热更资源模块添加到正在下载热更资源的列表中会找到或创建这个HotAssetsModule
                    //然后
                    mDownLoadAssetsModuleList.Add(assetsModule);
                }
                //设置热更资源模块的热更完成回调
                assetsModule.OnDownLoadAllAssetsFinish += HotModuleAssetsFinish;
                //开始热更资源，传入参数
                //参数1：开始下载的回调
                // MultipleThreadBalancing()；//分配下载线程
                //startHotCallBack?.Invoke(bundleModule);

                //参数2：热更完成回调
                //hotFinish
                //参数3：没有传入第三个参数，采用默认值true，即检测资源版本
                assetsModule.StartHotAssets(()=> { MultipleThreadBalancing();startHotCallBack?.Invoke(bundleModule); },hotFinish);
            }
            //如果正在下载热更资源模块的字典中的数量大于最大下载线程个数        
            else
            {
                //调用等待下载回调
                waiteDownLoad?.Invoke(bundleModule);
                //把热更模块添加到等待下载队列
                //key：资源模块类型
                //value：热更资源模块
                mWaitDownLoadQueue.Enqueue(new WaitDownLoadModule { bundleModule=bundleModule,startHot=startHotCallBack,hotFinish=hotFinish });
            }
        }
        /// <summary>
        /// 获取或创建热更资源模块
        /// </summary>
        /// <param name="bundleModule">资源模块类型枚举</param>
        /// <returns>热更资源模块</returns>
        public HotAssetsModule GetOrNewAssetModule(BundleModuleEnum bundleModule)
        {
            //声明热更资源模块
            HotAssetsModule assetsModule = null;
            //如果字典中包含资源模块类型
            if (mAllAssetsModuleDic.ContainsKey(bundleModule))
            {
                //获取热更资源模块，所有热更资源模块取出
                assetsModule = mAllAssetsModuleDic[bundleModule];
            }
            else
            {
                //创建热更资源模块，执行构造函数，传入资源模块类型和monobehaviour对象
                assetsModule = new HotAssetsModule(bundleModule,ZMAssetsFrame.Instance);
                //把热更资源模块添加到字典中
                mAllAssetsModuleDic.Add(bundleModule, assetsModule);
            }
            //返回热更资源模块
            return assetsModule;
        }
        /// <summary>
        /// 检测资源版本是否需要热更
        /// </summary>
        /// <param name="bundleModule">热更模块</param>
        /// <param name="callBack">热更回调</param>
        public void CheckAssetsVersion(BundleModuleEnum bundleModule, Action<bool, float> callBack)
        {
           //从字典获取资源模块，或者创建一个资源模块并添加到字典
           HotAssetsModule assetsModule= GetOrNewAssetModule(bundleModule);
           //调用资源模块的CheckAssetsVersion方法，传入参数8
           //参数1：热更回调
           //参数2：资源版本
           assetsModule.CheckAssetsVersion((isHot,sizem)=> {
               if (isHot==false)
               {
                //如果不需要更新

                    //加载资源模块配置Ab包，并读取其中的信息，然后在AssetBundleManager中用这些信息初始化BundleItem对象
                    //并且用mAllBundleAssetDic字典保存，key值是BundleItem对象的CRC值，value值是BundleItem对象
                    //BundleItem对象是AB包里的资源信息的自定义数据结构
                   AssetBundleManager.Instance.LoadAssetBundleConfig(bundleModule);
               }
               callBack?.Invoke(isHot, sizem);
           } );
        }
        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <returns></returns>
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModule)
        {   
            //如果所有热更资源模块字典中包含资源模块类型
            if (mAllAssetsModuleDic.ContainsKey(bundleModule))
            {
                //返回热更资源模块
                return mAllAssetsModuleDic[bundleModule];
            }
            //返回null
            return null;
        }
        /// <summary>
        /// 热更模块资源完成
        /// </summary>
        /// <param name="bundleModule">资源模块类型枚举</param>
        public void HotModuleAssetsFinish(BundleModuleEnum bundleModule)
        {
            //把下载完成的模块从下载中的字典中移除掉
            if (mDownLoadingAssetsModuleDic.ContainsKey(bundleModule))
            {
                //从正在下载热更资源模块的字典中获取热更资源模块
                HotAssetsModule assetsModule = mDownLoadingAssetsModuleDic[bundleModule];
                
                //如果正在下载热更资源的列表中包含热更资源模块
                if (mDownLoadAssetsModuleList.Contains(assetsModule))
                {
                    //设置热更资源模块的热更完成回调
                    //这里设置为ture后，在主线程更新中会移除热更资源模块-> OnMainThreadUpdate()
                    assetsModule.IsNeedRemove = true;
                }
                //从正在下载热更资源模块的字典中移除热更资源模块
                mDownLoadingAssetsModuleDic.Remove(bundleModule);
            }

            //判断等待下载的队列中是否有等待热更的模块，如果有，就可以进行热更了，因为已经有下载线程空闲下来
            if (mWaitDownLoadQueue.Count>0)
            {
                //从等待下载的队列中获取等待热更的模块
                WaitDownLoadModule downLoadModule= mWaitDownLoadQueue.Dequeue();
                //开始热更资源，传入参数
                //参数1：资源模块类型
                //参数2：开始热更回调
                //参数3：热更完成回调
                //参数4：没有传入第三个参数，采用默认值null，即不进行等待下载回调
                HotAssets(downLoadModule.bundleModule, downLoadModule.startHot, downLoadModule.hotFinish,null);
            }
            else
            {
                //在没有等待热更模块的情况下，并且已经有下载线程空闲下来了，
                //我们就需要把闲置下来的下载线程分配给其他正在热更的模块，增加该模块的热更速度
                //多线程均衡
                MultipleThreadBalancing();
            }
            //加载资源模块配置
            AssetBundleManager.Instance.LoadAssetBundleConfig(bundleModule);
        }
        /// <summary>
        /// 多线程均衡
        /// </summary>
        public void MultipleThreadBalancing()
        {
            //获取当前正在下载热更资源模块的一个长度个数
            int count = mDownLoadingAssetsModuleDic.Count;
            //计算多线程均衡后的线程分配个数
            //以最大下载线程个数为3 举例子
            //1.  3/1=3 最大并发下载线程个数为3  （偶数）
            //2.  3/2=1.5 向上取整 2 1 （奇数）
            //3.  3/3= 1  每一个模块 都拥有一个下载线程 
            float threadCount= MAX_THREAD_COUNT * 1.0f / count;
            //主下载线程个数
            int mainThreadCount = 0;
            //通过(int) 进行强转  (int)强转：表示向下强转
            //假如threadCount=1.5 ，(int)threadCount=1，
            //假如threadCount=2.5 ，(int)threadCount=2，
            int threadBalancingCount = (int)threadCount;

            //如果(int)threadCount< threadCount，表示threadCount为小数
            if ((int)threadCount< threadCount)
            {
                //主线程数量
                //向上取整
                //假如threadCount=1.5 ，Mathf.CeilToInt(threadCount)=2，
                mainThreadCount = Mathf.CeilToInt(threadCount);
                //多线程均衡数量
                //向下取整
                //假如threadCount=1.5 ，Mathf.FloorToInt(threadCount)=1，
                threadBalancingCount = Mathf.FloorToInt(threadCount);
            }
            //多线程均衡
            //线程分配策略：
            //第一个模块（i==0）获得mainThreadCount个线程
            //其他模块获得threadBalancingCount个线程
            //这是一种优先级分配机制
            int i = 0;
            //遍历正在下载热更资源模块的字典中的所有热更资源模块
            foreach (var item in mDownLoadingAssetsModuleDic.Values)
            {
                //如果主线程数量不为0，并且i==0
                if (mainThreadCount!=0&&i==0)
                {
                    //设置主下载线程个数
                    item.SetDownLoadThreadCount(mainThreadCount);//设置主下载线程个数
                }
                else
                {
                    //设置多线程均衡下载线程个数
                    item.SetDownLoadThreadCount(threadBalancingCount);
                }
                //i++
                i++;
            }
        }
        /// <summary>
        /// 主线程更新
        /// </summary>
        public void OnMainThreadUpdate()
        {
            //遍历正在下载热更资源的列表中的所有热更资源模块
            for (int i = 0; i < mDownLoadAssetsModuleList.Count; i++)
            {
                //调用热更资源模块的主线程更新方法
                mDownLoadAssetsModuleList[i].OnMainThreadUpdate();
            }
            //反向遍历正在下载热更资源的列表中的所有热更资源模块
            for (int i = mDownLoadAssetsModuleList.Count-1; i >=0; i--)
            {
                //如果热更资源模块的IsNeedRemove为true
                if (mDownLoadAssetsModuleList[i].IsNeedRemove)
                {
                    //从正在下载热更资源的列表中移除热更资源模块
                    mDownLoadAssetsModuleList.Remove(mDownLoadAssetsModuleList[i]);
                }
            }
        }
    }
}
