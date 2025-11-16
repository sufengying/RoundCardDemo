
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 热更资源清单
    /// </summary>
    public class HotAssetsManifest
    {
        /// <summary>
        /// 热更公告
        /// </summary>
        public string updateNotice;
        /// <summary>
        /// 下载地址
        /// </summary>
        public string downLoadURL;
        /// <summary>
        /// 热更资源补丁列表
        /// </summary>
        public List<HotAssetsPatch> hotAssetsPatchList = new List<HotAssetsPatch>();
    }
    /// <summary>
    /// 热更资源补丁
    /// </summary>
    public class HotAssetsPatch
    {
        /// <summary>
        /// 补丁版本
        /// </summary>
        public int patchVersion;
        /// <summary>
        /// 热更资源信息列表
        /// </summary>
        public List<HotFileInfo> hotAssetsList = new List<HotFileInfo>();
    }

    /// <summary>
    /// 热更文件信息
    /// </summary>
    public class HotFileInfo
    {
        public string abName;//AssetBundle名字

        public string md5;//文件的Md5

        public float size;//文件的大小
    }
}
