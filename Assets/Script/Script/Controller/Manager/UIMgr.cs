using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class UIMgr : Singletion<UIMgr>
{
    private Dictionary<string, UIBase> uiView=new Dictionary<string, UIBase>();

    private Transform _MainCanva;
 

    //UI管理器在游戏打开时就应该初始化游戏标题界面（异步）
    public async void Initialize()
    {
        //初始化的时候，先清理一下存储UI的字典
        uiView.Clear(); 
        //找到场景的主画布
        _MainCanva = GameObject.Find("UIRoot").transform;
        //异步加载游戏标题画面（可做加载动画）
        await LoadGameTitle();
        //注册事件
        Register();
        //foreach (var name in uiView.Keys)
        //{
        //    Debug.Log(name);
        //}
    }

    private void FindAllUi()
    {
        // 找到所有根物体（即场景中的最上层物体）
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            // 遍历根物体及其所有子物体，找到所有 UIBase 组件（包括隐藏的）
            UIBase[] uIBases = rootObject.GetComponentsInChildren<UIBase>(true);

            foreach (var ui in uIBases)
            {
                string key = ui.GetType().Name;  // 以 GameObject 名称作为 key
                if (!uiView.ContainsKey(key))
                {
                    uiView.Add(key, ui);
                    //ui.Initialize();
                }
            }
        }
    }

    //从预制体上加载UI，同时会把UI加入字典存储（异步）
    public async Task LoadGameTitle()
    {
           await ResourcesMgr.Instance.LoadObjectAsync(TestAssetsPath.ui_text_GameTitle,
                                                                        _MainCanva.transform);
            FindAllUi();
    }

    //从预制体上加载UI，同时会把UI加入字典存储（异步）
    public async Task LoadMainGameInterface()
    {
        await ResourcesMgr.Instance.LoadObjectAsync(TestAssetsPath.MainGameInterface,
                                                             _MainCanva.transform);
        FindAllUi();
    }

    //触发战斗时加载战斗UI
    public  GameObject LoadBattleUI()
    {
        
        GameObject battleUI = ResourcesMgr.Instance.LoadGameObject(TestAssetsPath.battleUI,GameNode.Instance.UIRoot);
        FindAllUi();
        return battleUI;
    }

    public ChacaterRoundInfo LoadPlayerBattleUI(RectTransform parent)
    {
        
        return ResourcesMgr.Instance.LoadObject<ChacaterRoundInfo>(TestAssetsPath.playerBattleUI, parent);
    }

    // 卸载玩家战斗UI元素
    public void UnloadPlayerBattleUI(ChacaterRoundInfo uiInstance)
    {
        if (uiInstance != null)
        {
            // 销毁UI对象
            UnityEngine.Object.Destroy(uiInstance.gameObject);
        }
    }

    public void ShowUI<T>() where T : UIBase
    {
        // 通过类型显示指定的 UI
        string key = typeof(T).Name;
        if (uiView.ContainsKey(key))
        {
            uiView[key].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"UI {key} not found in uiView.");
        }

    }

    public void HideUI<T>() where T: UIBase
    {
        // 通过类型隐藏指定的 UI
        string key = typeof(T).Name;
        if (uiView.ContainsKey(key))
        {
            //Debug.Log(uiView[key]);
            uiView[key].gameObject.SetActive(false);
        }


    }

    //在事件中心注册事件
    private void Register()
    {
        
        EventCenter.Instance.AddListener("EnterGame", EnterGame);
        EventCenter.Instance.AddListener("PrintUIInDic", PrintUIInDic);
    }

    //点击进入游戏，会加载游戏主界面UI
    public async void EnterGame()
    {
        await LoadMainGameInterface();
        HideUI<GameTitle>();  // 隐藏当前开始界面
        ShowUI<MainGameInterface>(); // 示例：显示主游戏 UI
       
    }

    public void PrintUIInDic()
    {
        foreach (var key in uiView.Keys)
        {
            Debug.Log(key);
        }
    }

    
}
