
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZM.AssetFrameWork
{
    /// <summary>
    /// 构建类型枚举
    /// </summary>
    public enum BuildType
    {
        AssetBundle,    // 普通AssetBundle打包
        HotPatch,      // 热更新补丁打包
    }

    /// <summary>
    /// AssetBundle构建编译器
    /// 负责处理资源打包、热更新资源生成、资源加密等核心功能
    /// </summary>
    public class BuildBundleCompiler
    {
        /// <summary>
        /// 更新公告内容
        /// </summary>
        private static string mUpdateNotice;

        /// <summary>
        /// 热更新补丁版本号
        /// </summary>
        private static int mHotPatchVersion;

        /// <summary>
        /// 当前打包类型（AssetBundle或热更新补丁）
        /// </summary>
        private static BuildType mBuildType;

        /// <summary>
        /// 当前打包的模块配置数据
        /// </summary>
        private static BundleModuleData mBuildModuleData;

        /// <summary>
        /// 当前打包的模块类型枚举
        /// </summary>
        private static BundleModuleEnum mBundleModuleEnum;

        /// <summary>
        /// 所有需要打包的AssetBundle文件路径列表
        /// </summary>
        private static List<string> mAllBundlePathList = new List<string>();

        /// <summary>
        /// 所有文件夹对应的Bundle字典
        /// Key: Bundle名称
        /// Value: 该Bundle包含的所有文件路径列表
        /// </summary>
        private static Dictionary<string, List<string>> mAllFolderBundleDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 所有预制体对应的Bundle字典
        /// Key: Bundle名称
        /// Value: 该Bundle包含的所有依赖文件路径列表
        /// </summary>
        private static Dictionary<string, List<string>> mAllPrefabsBundleDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// AssetBundle文件输出路径
        /// 格式：项目根目录/AssetBundle/模块名/平台/
        /// </summary>
        private static string mBundleOutPutPath { get { return Application.dataPath + "/../AssetBundle/" + mBundleModuleEnum + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/"; } }

        /// <summary>
        /// 热更新资源文件输出路径
        /// 格式：项目根目录/HotAssets/模块名/版本号/平台/
        /// </summary>
        private static string mHotAssetsOutPutPath { get { return Application.dataPath + "/../HotAssets/" + mBundleModuleEnum + "/" + mHotPatchVersion + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/"; } }

        /// <summary>
        /// 框架Resources路径
        /// </summary>
        private static string mResourcesPath { get { return Application.dataPath + "/ZMAssetsFrame/Resources/"; } }

        /// <summary>
        /// 构建AssetBundle的主入口方法
        /// </summary>
        /// <param name="moduleData">资源模块配置数据，包含打包路径等信息</param>
        /// <param name="buildType">打包类型，默认为AssetBundle</param>
        /// <param name="hotPatchVersion">热更新补丁版本号，默认为0</param>
        /// <param name="updateNotice">更新公告内容，默认为空</param>
        public static void BuildAssetBundle(BundleModuleData moduleData, BuildType buildType = BuildType.AssetBundle, int hotPatchVersion = 0, string updateNotice = "")
        {
            // 初始化打包环境
            Initlization(moduleData, buildType, hotPatchVersion, updateNotice);
            // 打包所有标记的文件夹
            BuildAllFolder();
            // 打包根目录下的所有子文件夹
            BuildRootSubFolder();
            // 打包所有预制体及其依赖
            BuildAllPrefabs();
            // 执行Unity的AssetBundle打包
            BuildAllAssetBundle();
        }

        /// <summary>
        /// 初始化打包环境
        /// 清理旧数据并设置新的打包参数
        /// </summary>
        public static void Initlization(BundleModuleData moduleData, BuildType buildType = BuildType.AssetBundle, int hotPatchVersion = 0, string updateNotice = "")
        {
            // 清理所有缓存数据，防止上次打包数据残留
            mAllBundlePathList.Clear();
            mAllFolderBundleDic.Clear();
            mAllPrefabsBundleDic.Clear();

            // 设置打包参数
            mBuildType = buildType;
            mUpdateNotice = updateNotice;
            mBuildModuleData = moduleData;
            mHotPatchVersion = hotPatchVersion;
            // 将模块名称字符串转换为对应的枚举值
            mBundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), moduleData.moduleName);

            // 清理并创建输出目录
            FileHelper.DeleteFolder(mBundleOutPutPath);
            Directory.CreateDirectory(mBundleOutPutPath);
        }

        /// <summary>
        /// 打包所有标记的文件夹
        /// 处理signFolderPathArr中配置的所有文件夹路径
        /// </summary>
        public static void BuildAllFolder()
        {
            // 检查是否有需要打包的文件夹
            if (mBuildModuleData.signFolderPathArr == null || mBuildModuleData.signFolderPathArr.Length == 0)
            {
                return;
            }

            // 遍历所有标记的文件夹路径
            for (int i = 0; i < mBuildModuleData.signFolderPathArr.Length; i++)
            {
                // 统一路径格式，将反斜杠转换为正斜杠
                string path = mBuildModuleData.signFolderPathArr[i].bundlePath.Replace(@"\", "/");

                // 检查是否重复打包
                if (IsRepeatBundleFile(path) == false)
                {
                    // 添加到打包路径列表
                    mAllBundlePathList.Add(path);
                    // 生成Bundle名称（模块名_文件夹名）
                    string bundleName = GenerateBundleName(mBuildModuleData.signFolderPathArr[i].abName);

                    // 将路径添加到对应的Bundle字典中
                    if (!mAllFolderBundleDic.ContainsKey(bundleName))
                    {
                        mAllFolderBundleDic.Add(bundleName, new List<string> { path });
                    }
                    else
                    {
                        mAllFolderBundleDic[bundleName].Add(path);
                    }
                }
                else
                {
                    Debug.LogError("重复的Bundle文件路径：" + path);
                }
            }
        }

        /// <summary>
        /// 打包父文件夹下的所有子文件夹
        /// </summary>
        public static void BuildRootSubFolder()
        {
            //检测父文件夹是否有配置，如果没配置就直接跳过
            if (mBuildModuleData.rootFolderPathArr == null || mBuildModuleData.rootFolderPathArr.Length == 0)
            {
                return;
            }

            //遍历父文件夹路径数组  
            for (int i = 0; i < mBuildModuleData.rootFolderPathArr.Length; i++)
            {
                //获取根目录路径 + "/" 在路径末尾添加斜杠
                string path = mBuildModuleData.rootFolderPathArr[i] + "/";
                //获取符文夹的所有的子文件夹
                //它会返回指定路径下所有子目录的完整路径数组
                string[] folderArr = Directory.GetDirectories(path);
                foreach (var item in folderArr)
                {
                    path = item.Replace(@"\", "/");
                    // 找到路径中最后一个斜杠的位置
                    // + 1 将位置移到斜杠后面，指向目录名的起始位置
                    int nameIndex = path.LastIndexOf("/") + 1;
                    //获取文件夹同名的AssetBundle名称（如果模块名是ZM_UI，那么文件夹名是UI，那么生成的AssetBundle名称为ZM_UI_UI）
                    string bundleName = GenerateBundleName(path.Substring(nameIndex, path.Length - nameIndex));
                    //检查路径
                    if (!IsRepeatBundleFile(path))
                    {
                        mAllBundlePathList.Add(path);
                        if (!mAllFolderBundleDic.ContainsKey(bundleName))
                        {
                            mAllFolderBundleDic.Add(bundleName, new List<string> { path });
                        }
                        else
                        {
                            mAllFolderBundleDic[bundleName].Add(path);
                        }
                    }
                    else
                    {
                        Debug.LogError("RepeatBundle file FolderPath:" + path);
                    }
                    //处理子文件夹资源的代码
                    //获取文件夹下所有文件的完整路径，“*”表示所有文件
                    string[] filePathArr = Directory.GetFiles(path, "*");
                    foreach (var filePath in filePathArr)
                    {
                        //遍历路径
                        //过滤.meta文件
                        if (!filePath.EndsWith(".meta"))
                        {
                            //将路径中的反斜杠转换为正斜杠
                            string abFilePath = filePath.Replace(@"\", "/");
                            //检查路径是否重复
                            if (!IsRepeatBundleFile(abFilePath))
                            {
                                //添加到打包路径列表
                                mAllBundlePathList.Add(abFilePath);
                                //将路径添加到对应的Bundle字典中
                                if (!mAllFolderBundleDic.ContainsKey(bundleName))
                                {
                                    //这个字典存储的key是模块名_文件夹名,value是该Bundle包含的所有文件路径列表
                                    //例如：模块：ZM，文件夹：UI，那么生成的Bundle名称为ZM_UI，
                                    //那么字典的key为ZM_UI，value为该Bundle包含的所有文件路径列表（如：ZM/UI/UI.prefab）
                                    mAllFolderBundleDic.Add(bundleName, new List<string> { abFilePath });
                                }
                                else
                                {
                                    //如果字典中已经存在该Bundle，则将路径添加到对应的Bundle字典中，如果有新的资源，就添加到列表中
                                    mAllFolderBundleDic[bundleName].Add(abFilePath);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 打包指定文件夹下的所有预制体
        /// </summary>
        public static void BuildAllPrefabs()
        {
            //检查是否有需要打包的预制体
            if (mBuildModuleData.prefabPathArr == null || mBuildModuleData.prefabPathArr.Length == 0)
            {
                return;
            }
            //获取所有预制体的GUID
            //"t:Prefab"表示只查找类型为Prefab的资源
            //mBuildModuleData.prefabPathArr表示要查找的文件夹路径数组
            //这些路径是在配置中指定的预制体所在目录
            string[] guidArr = AssetDatabase.FindAssets("t:Prefab", mBuildModuleData.prefabPathArr);

            for (int i = 0; i < guidArr.Length; i++)
            {
                //将GUID转换为完整的文件路径（包含后缀）
                string filePath = AssetDatabase.GUIDToAssetPath(guidArr[i]);
                //计算AssetBundle名称
                //Path.GetFileNameWithoutExtension(filePath)获取文件名（不包含扩展名）
                //例如：ZM/UI/UI.prefab，那么生成的AssetBundle名称为ZM_UI
                string bundleName = GenerateBundleName(Path.GetFileNameWithoutExtension(filePath));
                //如果该AssetBUndle不存在，就计算打包数据
                if (!mAllBundlePathList.Contains(filePath))
                {
                    //获取预制体所有的依赖项
                    //AssetDatabase.GetDependencies() 是 Unity 编辑器 API，用于获取资源的所有依赖项
                    //返回一个字符串数组，包含所有依赖资源的完整路径，除依赖资源外，也会返回资源本身
                    string[] dependsArr = AssetDatabase.GetDependencies(filePath);
                    //创建一个列表，用于存储所有依赖资源的路径
                    List<string> dependsList = new List<string>();
                    //遍历所有依赖资源
                    for (int k = 0; k < dependsArr.Length; k++)
                    {
                        string path = dependsArr[k];
                        //如果不是冗余文件，就归纳进打包（遍历mAllBundlePathList，非脚本，非相同路径，非被包含的路径）
                        if (!IsRepeatBundleFile(path))
                        {
                            mAllBundlePathList.Add(path);
                            dependsList.Add(path);
                        }
                    }
                    //如果不存在，添加到预制体字典中，每一个预制体都是一个ab包
                    if (!mAllPrefabsBundleDic.ContainsKey(bundleName))
                    {
                        mAllPrefabsBundleDic.Add(bundleName, dependsList);
                    }
                    else
                    {
                        Debug.LogError("重复预制体名字，当前模块下有预制体文件重复 Name:" + bundleName);
                    }
                }
            }
        }
        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        public static void BuildAllAssetBundle()
        {
            //修改所有要打包的文件的AssetBundleName
            ModifyAllFileBundleName();
            //生成一份AssetBundle配置
            WriteAssetBundleConfig();

            AssetDatabase.Refresh();

            //调用UnityAPI打包AssetBundle
            //返回值：AssetBundleManifest 对象，包含所有 Bundle 的信息
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                 //AssetBundle文件输出路径
                mBundleOutPutPath,
                //将字符串转换为枚举值
                //第一个参数：枚举类型 typeof(UnityEditor.BuildAssetBundleOptions)
                //第二个参数：要转换的字符串 BundleSettings.Instance.buildbundleOptions.ToString()
                //压缩
                (UnityEditor.BuildAssetBundleOptions)Enum.Parse(typeof(UnityEditor.BuildAssetBundleOptions), 

                BundleSettings.Instance.buildbundleOptions.ToString()), 
                (UnityEditor.BuildTarget)Enum.Parse(typeof(UnityEditor.BuildTarget), 
                BundleSettings.Instance.buildTarget.ToString()));

            //如果打包失败，显示错误信息
            if (manifest == null)
            {
                EditorUtility.DisplayProgressBar("BuildAssetBundle!", "BuildAssetBundle failed!", 1);
                Debug.LogError("AssetBundle Build failed!");
            }
            else
            {
                Debug.Log("AssetBundle Build Successs!:" + manifest);
                //删除所有AssetBundle自动生成的manifest文件
                DeleteAllBundleManifestFile();
                //加密所有AssetBundle
                EncryptAllBundle();
                //打包类型
                //如果打包类型为热更新补丁，就生成热更新资源
                if (mBuildType == BuildType.HotPatch)
                {
                    GeneratorHotAssets();
                }
            }
            //修改所有文件夹下AssetBundle name（），
            //这个字典的key是模块名_文件夹名,value是该Bundle包含的所有文件路径列表
            //例如：模块：ZM，文件夹：UI，那么生成的AssetBundle名称为ZM_UI，
            //那么字典的key为ZM_UI，value为该Bundle包含的所有文件路径列表（如：ZM/UI/UI.prefab）
            //这个子典的每个key都是一个ab包
            //做字典与列表的检查
            //传入true，表示清空所有AB包的名字
            //传入false,则不会
            ModifyAllFileBundleName(true);

            //清除进度条
            EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 生成AssetBundle配置文件
        /// </summary>
        public static void WriteAssetBundleConfig()
        {
            BundleConfig config = new BundleConfig();
            config.bundleInfoList = new List<BundleInfo>();
            //所有AssetBundle文件字典 key =路径 value =AssetBundleName
            Dictionary<string, string> allBundleFilePathDic = new Dictionary<string, string>();
            //获取到工程内所有的AssetBundleName（包名）
            string[] allBundleArr = AssetDatabase.GetAllAssetBundleNames();

            foreach (var bundleName in allBundleArr)
            {
                //获取指定AssetBundleName 下的所有的文件路径,获取ab包打包的文件
                string[] bundleFileArr = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

                foreach (var filePath in bundleFileArr)
                {       
                    //过滤.cs文件
                    if (!filePath.EndsWith(".cs"))
                    {
                        //将文件路径和AssetBundleName添加到字典中
                        //key是文件路径，value是AssetBundleName
                        //意思是 资源路径 对应的ab包名
                        allBundleFilePathDic.Add(filePath, bundleName);
                    }
                }
            }
            //计算AssetBundle数据，生成AsestBundle配置文件。
            foreach (var item in allBundleFilePathDic)
            {
                //获取文件路径
                string filePath = item.Key;
                //过滤.cs文件
                if (!filePath.EndsWith(".cs"))
                {
                    //创建一个BundleInfo对象，用于存储AssetBundle信息（资源文件信息数据结构）
                    BundleInfo info = new BundleInfo();
                    //设置文件路径
                    info.path = filePath;
                    //设置该资源文件对应的AssetBundle名称
                    info.bundleName = item.Value;
                    //设置资源名称，获取文件名  GetFileName
                    info.assetName = Path.GetFileName(filePath);
                    //设置CRC校验值
                    info.crc = Crc32.GetCrc32(filePath);
                    //设置依赖项列表
                    info.bundleDependce = new List<string>();
                    //获取该资源依赖项列表，如果有的话
                    //如果一张照片执行这个api
                    //对于图片资源，依赖项通常包括：
                    //图片文件本身
                    //图片的 .meta 文件（包含图片的导入设置）
                    string[] depence = AssetDatabase.GetDependencies(filePath);
                    foreach (var dePath in depence)
                    {
                        //如果依赖项不是当前的这个文件，以及依赖项不是cs脚本 就进行处理
                        if (!dePath.Equals(filePath) && dePath.EndsWith(".cs") == false)
                        {
                            string assetBundleName = "";
                            if (allBundleFilePathDic.TryGetValue(dePath, out assetBundleName))
                            {
                                //如果依赖项已经包含这个AssetBundle就不进行处理，否则添加进依赖项
                                //查看所有ab包打包的文件的路径，如果不包含这个路径，就添加进依赖项
                                if (!info.bundleDependce.Contains(assetBundleName))
                                {
                                    info.bundleDependce.Add(assetBundleName);
                                }
                            }
                        }
                    }
                    //把这一项资源文件信息添加到配置文件中
                    config.bundleInfoList.Add(info);
                }
            }
            //生成AsestBundle配置文件。
            //将配置对象序列化为 JSON 字符串
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            string bundleConfigPath = Application.dataPath + "/" + mBundleModuleEnum.ToString().ToLower() + "assetbundleconfig.json";
            StreamWriter writer = File.CreateText(bundleConfigPath);
            writer.Write(json);
            writer.Dispose();
            writer.Close();

            AssetDatabase.Refresh();
            //修改AssetBundle配置文件的AssetBundleName
            AssetImporter importer = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
            if (importer != null)
            {
                importer.assetBundleName = mBundleModuleEnum.ToString().ToLower() + "bundleconfig.unity";
            }
        }
        /// <summary>
        /// 修改或清空AssetBundle
        /// </summary>
        /// <param name="clear"></param>
        public static void ModifyAllFileBundleName(bool clear = false)
        {
            int i = 0;
            //修改所有文件夹下AssetBundle name（），
            //这个字典的key是模块名_文件夹名,value是该Bundle包含的所有文件路径列表
            //例如：模块：ZM，文件夹：UI，那么生成的AssetBundle名称为ZM_UI，
            //那么字典的key为ZM_UI，value为该Bundle包含的所有文件路径列表（如：ZM/UI/UI.prefab）
            //这个子典的每个key都是一个ab包
            foreach (var item in mAllFolderBundleDic)
            {
                i++;
                // 显示进度条，显示当前处理的Bundle名称和进度
                EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / mAllFolderBundleDic.Count);
                //遍历每个ab包包含的所有文件路径
                foreach (var path in item.Value)
                {
                    //返回该资源的导入器对象
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        //当 clear = false 时：
                       // -importer.assetBundleName 会被设置为 "UI_Prefabs.unity"
                       // - 这样该资源会被打包到 "UI_Prefabs.unity" 这个 AssetBundle 中

                       // 当 clear = true 时：
                       // -importer.assetBundleName 会被设置为 ""
                       // - 这样会清除该资源的 AssetBundle 名称
                        importer.assetBundleName = (clear ? "" : item.Key + ".unity");
                    }
                }
            }
            //修改所有预制体的AssetBundleName
            i = 0;
            foreach (var item in mAllPrefabsBundleDic)
            {
                //显示进度条，显示当前处理的Bundle名称和进度
                i++;
                //获得每个预制体对应的依赖文件完整路径列表
                List<string> bundleList = item.Value;
                //遍历每个预制体对应的依赖文件完整路径列表
                foreach (var path in bundleList)
                {
                    //显示进度条，显示当前处理的Bundle名称和进度
                    EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / mAllPrefabsBundleDic.Count);
                    //返回该资源的导入器对象
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        //当 clear = false 时：
                        //- importer.assetBundleName 会被设置为 "UI_Prefabs.unity"
                        //- 这样该资源会被打包到 "UI_Prefabs.unity" 这个 AssetBundle 中

                        //当 clear = true 时：
                        //- importer.assetBundleName 会被设置为 ""
                        //- 这样会清除该资源的 AssetBundle 名称
                        //每个预制体以及以来的文件被打成一个包，包名为模块名_预制体名
                        importer.assetBundleName = (clear ? "" : item.Key + ".unity");
                    }
                }

            }
            
            if (clear)
            {
                //bundleConfigPath 是配置文件的完整路径
                //格式为：项目根目录/Assets/模块名小写assetbundleconfig.json
                //例如："E:/Project/Assets/uiassetbundleconfig.json"
                string bundleConfigPath = Application.dataPath + "/" + mBundleModuleEnum.ToString().ToLower() + "assetbundleconfig.json";
                //返回该资源的导入器对象
                //bundleConfigPath.Replace(Application.dataPath, "Assets")
                //将完整路径转换为 Unity 资源路径
                //例如：
                //输入："E:/Project/Assets/uiassetbundleconfig.json"
                //输出："Assets/uiassetbundleconfig.json"
                AssetImporter importer = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
                if (importer != null)
                {
                    //获取配置文件的导入器
                    //将 assetBundleName 设置为空字符串
                    //这样配置文件就不会被打包到 AssetBundle 中
                    importer.assetBundleName = "";
                }
                //移除项目中所有未使用的 AssetBundle 名称
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }
        }
        /// <summary>
        /// 检查文件路径是否重复或无效
        /// </summary>
        /// <param name="path">要检查的文件路径</param>
        /// <returns>true表示重复或无效，false表示可以打包</returns>
        public static bool IsRepeatBundleFile(string path)
        {
            foreach (var item in mAllBundlePathList)
            {
                // 检查三种情况：
                // 1. 完全相同的路径
                // 2. 路径被其他路径包含（防止资源路径冲突）
                // 3. 是C#脚本文件（脚本不需要打包）
                if (string.Equals(item, path) || item.Contains(path) || path.EndsWith(".cs"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 生成AssetBundle名称
        /// 格式：模块名_资源名
        /// </summary>
        /// <param name="abName">资源名称</param>
        /// <returns>完整的Bundle名称</returns>
        public static string GenerateBundleName(string abName)
        {
            return mBundleModuleEnum.ToString() + "_" + abName;
        }

        /// <summary>
        /// 删除所有AssetBundle自动生成的manifest文件
        /// manifest文件是Unity自动生成的，不需要保留
        /// </summary>
        public static void DeleteAllBundleManifestFile()
        {
            //获取到AssetBundle文件输出路径下的所有文件
            string[] filePathArr = Directory.GetFiles(mBundleOutPutPath);
            //遍历所有文件
            foreach (var path in filePathArr)
            {
                //如果文件名包含.manifest，就删除   
                if (path.EndsWith(".manifest"))
                {
                    //删除文件
                    File.Delete(path);
                }
            }
        }
        /// <summary>
        /// 加密所有的AssetBundle文件
        /// 使用AES加密算法保护资源文件
        /// </summary>
        public static void EncryptAllBundle()
        {
            // 检查是否启用加密
            if (BundleSettings.Instance.bundleEncrypt.isEncrypt)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(mBundleOutPutPath);
                FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

                // 遍历所有文件进行加密
                for (int i = 0; i < fileInfoArr.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("加密文件", "正在加密: " + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                    // 使用AES加密，密钥为"zhumengxy"
                    AES.AESFileEncrypt(fileInfoArr[i].FullName, "zhumengxy");
                }

                EditorUtility.ClearProgressBar();
                Debug.Log("AssetBundle加密完成！");
            }
        }
        /// <summary>
        /// 将AssetBundle文件复制到StreamingAssets文件夹
        /// </summary>
        /// <param name="moduleData">模块数据</param>
        /// <param name="showTips">是否显示提示</param>
        public static void CopyBundleToStramingAssets(BundleModuleData moduleData, bool showTips = true)
        {
            //Enum.Parse() 方法用于将字符串转换为对应的枚举值
            //第一个参数 typeof(BundleModuleEnum) 指定要转换的目标枚举类型
            //第二个参数 moduleData.moduleName 是要转换的字符串
            mBundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), moduleData.moduleName);
            //获取目标文件夹下的所有AssetBundle文件
            //只会获取指定模块的文件夹
            //{ get { return Application.dataPath + "/../AssetBundle/" + mBundleModuleEnum + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/"; } }
            DirectoryInfo directoryInfo = new DirectoryInfo(mBundleOutPutPath);
            //获取指定目录所有文件信息
            //SearchOption.AllDirectories 表示搜索所有子目录
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            //Bundle内嵌的目标文件夹
            string streamingAssetsPath = Application.streamingAssetsPath + "/AssetBundle/" + mBundleModuleEnum + "/";

            FileHelper.DeleteFolder(streamingAssetsPath);
            Directory.CreateDirectory(streamingAssetsPath);

            List<BuiltinBundleInfo> bundleInfoList = new List<BuiltinBundleInfo>();
            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                EditorUtility.DisplayProgressBar("内嵌资源中", "Name:" + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                //拷贝文件
                //fileInfoArr[i].FullName: 获取当前文件的完整路径(包含盘符和目录)
                //streamingAssetsPath: StreamingAssets文件夹的路径
                //fileInfoArr[i].Name: 获取当前文件的文件名(不包含路径)
                //复制AB包到指定路径
                File.Copy(fileInfoArr[i].FullName, streamingAssetsPath + fileInfoArr[i].Name);
                //生成内嵌资源文件信息
                BuiltinBundleInfo info = new BuiltinBundleInfo();
                //获取当前文件的文件名(不包含路径)
                info.fileName = fileInfoArr[i].Name;
                //获取当前文件的MD5值
                info.md5 = MD5.GetMd5FromFile(fileInfoArr[i].FullName);
                //获取当前文件的大小
                info.size = fileInfoArr[i].Length / 1024;
                //将内嵌资源文件信息添加到列表中
                bundleInfoList.Add(info);
            }

            string json = JsonConvert.SerializeObject(bundleInfoList, Formatting.Indented);
            //如果Resources文件夹不存在，就创建
            if (!Directory.Exists(mResourcesPath))
            {
                Directory.CreateDirectory(mResourcesPath);
            }

            //写入配置文件到Resources文件夹
            FileHelper.WriteFile(mResourcesPath + mBundleModuleEnum + "info.json", System.Text.Encoding.UTF8.GetBytes(json));

            AssetDatabase.Refresh();
            //清除进度条
            EditorUtility.ClearProgressBar();
            //如果showTips为true，就显示对话框
            if (showTips)
            {
                EditorUtility.DisplayDialog("内嵌操作", "内嵌资源完成 Path：" + streamingAssetsPath, "确认");
            }
            Debug.Log(" Assets Copy toStreamingAssets Finish!");
        }

        /// <summary>
        /// 生成热更新资源
        /// 将AssetBundle复制到热更新目录并生成热更新清单
        /// </summary>
        public static void GeneratorHotAssets()
        {
            // 清理并创建热更新输出目录
            FileHelper.DeleteFolder(mHotAssetsOutPutPath);
            Directory.CreateDirectory(mHotAssetsOutPutPath);

            // 获取所有需要热更新的Bundle文件
            //所有匹配的文件的完整路径数组
            string[] bundlePatchArr = Directory.GetFiles(mBundleOutPutPath, "*.unity");

            // 复制文件到热更新目录
            for (int i = 0; i < bundlePatchArr.Length; i++)
            {
                string path = bundlePatchArr[i];
                EditorUtility.DisplayProgressBar("生成热更文件", "正在处理: " + Path.GetFileName(path), i * 1.0f / bundlePatchArr.Length);
                //拼接热更新目录输出路径
                string disPath = mHotAssetsOutPutPath + Path.GetFileName(path);
                //拷贝文件
                File.Copy(path, disPath);
            }

            Debug.Log("热更新文件生成成功");
            // 生成热更新清单文件
            GeneratorHotAssetsManifest();
        }

        /// <summary>
        /// 生成热更新资源配置清单
        /// 包含更新公告、下载地址、补丁版本等信息
        /// </summary>
        public static void GeneratorHotAssetsManifest()
        {
            // 创建热更新清单对象
            HotAssetsManifest assetsManifest = new HotAssetsManifest();
            assetsManifest.updateNotice = mUpdateNotice;
            // 设置热更新资源下载地址
            assetsManifest.downLoadURL = BundleSettings.Instance.AssetBundleDownLoadUrl + "/HotAssets/" + mBundleModuleEnum + "/" +
                mHotPatchVersion + "/" + BundleSettings.Instance.buildTarget;

            // 创建补丁信息对象
            HotAssetsPatch hotAssetsPatch = new HotAssetsPatch();
            hotAssetsPatch.patchVersion = mHotPatchVersion;

            // 获取所有热更新文件信息（也就是复制过来的AB包）
            DirectoryInfo directory = new DirectoryInfo(mHotAssetsOutPutPath);
            FileInfo[] bundleInfoArr = directory.GetFiles("*.unity");

            // 计算每个热更新文件的信息（MD5、大小等）
            foreach (var bundleInfo in bundleInfoArr)
            {
                HotFileInfo info = new HotFileInfo();
                info.abName = bundleInfo.Name;
                info.md5 = MD5.GetMd5FromFile(bundleInfo.FullName);
                info.size = bundleInfo.Length / 1024.0f; // 转换为KB

                //把这个模块的这个热更新文件（也就是复制过来的AB包）信息添加到热更新清单中
                hotAssetsPatch.hotAssetsList.Add(info);
            }

            // 添加补丁信息到清单
            //hotAssetsPatchList代表着这个热更新模块的每个版本的信息
            //例如：hotAssetsPatchList[0]代表着这个热更新模块的第一个版本的信息
            //例如：hotAssetsPatchList[1]代表着这个热更新模块的第二个版本的信息
            //而 hotAssetsPatchList[i].hotAssetsList代表着这个热更新模块的第i个版本中所有的资源文件信息
            //例如：hotAssetsPatchList[i].hotAssetsList[0]代表着这个热更新模块的第i个版本中的第一个资源文件信息
            assetsManifest.hotAssetsPatchList.Add(hotAssetsPatch);

            // 将清单对象序列化为JSON并保存
            string json = JsonConvert.SerializeObject(assetsManifest, Formatting.Indented);
            FileHelper.WriteFile(Application.dataPath + "/../HotAssets/" + mBundleModuleEnum + "AssetsHotManifest.json",
                System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}
