using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ZM.AssetFrameWork
{
    public abstract class IDecompressAssets
    {
        /// <summary>
        /// 需要解压的资源的总大小
        /// </summary>
        public float TotalSizem { get; protected set; }
        /// <summary>
        /// 已经解压的大小
        /// </summary>
        public float AlreadyDecompressSizem { get; protected set; }
        /// <summary>
        /// 是否开始解压
        /// </summary>
        public bool IsStartDecompress { get; protected set; }

        /// <summary>
        /// 开始解压内嵌文件
        /// </summary>
        /// <returns></returns>
        abstract public IDecompressAssets StartDeCompressBuiltinFile(BundleModuleEnum bundleModule, Action callBack);
        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns></returns>
        abstract public float GetDecompressProgress();
    }
}
