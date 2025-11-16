using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZM.AssetFrameWork;
using ZM.UI;

public class Main : MonoBehaviour
{
    private Func<string, List<CharacterConfig>> getCharacterConfigList;
    private Action<BattleWorldConfig> setBattleWorldConfig;
    List<CharacterConfig> playerConfigList = new List<CharacterConfig>();
    List<CharacterConfig> enemyConfigList = new List<CharacterConfig>();

    public void Awake()
    {
        ZMAssetsFrame.Instance.InitFrameWork();

        setBattleWorldConfig = TestData;


        getCharacterConfigList = GetCharacterConfigList;

        EventCenter.Instance.AddListener("GameMain_SetBattleWorldConfig", setBattleWorldConfig);
        EventCenter.Instance.AddListener("GameMain_GetCharacterConfigList", getCharacterConfigList);

        //UIMgr.Instance.Initialize();
        UIModule.Instance.Initialize();

        GlobalWorldMgr.Instance.Initialize();


        GlobalWorldMgr.Instance.UpdateCharacterData(playerConfigList, enemyConfigList);

    }

    void Start()
    {
        HotUpdateManager.Instance.HotAndUnPackAssets(BundleModuleEnum.UIWindow, this);
        
    }

    public void StartGame()
    {
        UIModule.Instance.PopUpWindow<StartWindow>();
    }

    void Update()
    {
        GlobalWorldMgr.Instance.Update();
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     UIModule.Instance.PopUpWindow<FailureWindow>();
        // }

    }

    public void TestData(BattleWorldConfig battleWorldConfig)
    {
        this.playerConfigList.Clear();


        CharacterConfig[] playerConfigList = battleWorldConfig.playerConfigList;

        for (int i = 0; i < playerConfigList.Length; i++)
        {
            if (playerConfigList[i] != null)
            {

                this.playerConfigList.Add(playerConfigList[i]);
            }
        }


        enemyConfigList = battleWorldConfig.enemyConfigList;


    }

    private List<CharacterConfig> GetCharacterConfigList(string characterTypeName)
    {
        switch (characterTypeName)
        {
            case "player":
                return playerConfigList;
            case "enemy":
                return enemyConfigList;
        }
        Debug.LogError("GetCharacterConfigList: 鏃犳晥鐨勮鑹茬被鍨�??");
        return null;
    }

    void OnDestroy()
    {
        GlobalWorldMgr.Instance.OnDestroy();
    }


}
