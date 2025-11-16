using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMGCFrameWork.Battle;


    /// <summary>
    /// 全局世界管理器，采用单例模式管理大世界场景与战斗场景
    /// </summary>
    public class GlobalWorldMgr : Singletion<GlobalWorldMgr>
    {
        private bool isInBattle = false;
        public BattleWorld BattleWorld { get; private set; }

        // 角色数据
        private List<CharacterConfig> playerConfigList;
        private List<CharacterConfig> enemyConfigList;

        private Action createBattleWorld;

        private void InitializeWithConfig()
        {
            playerConfigList = new List<CharacterConfig>();
            enemyConfigList = new List<CharacterConfig>();
        }

        public void Initialize()
        {
            
            createBattleWorld = EnterBattleWorld;
            RegisterEvents();
      
        }

        /// <summary>
        /// 更新角色数据
        /// </summary>
        public void UpdateCharacterData(List<CharacterConfig> playerConfigList, List<CharacterConfig> enemyConfigList)
        {
            if (playerConfigList == null || enemyConfigList == null)
            {
                throw new ArgumentNullException("Character data lists cannot be null");
            }
            this.playerConfigList = playerConfigList;
            this.enemyConfigList = enemyConfigList;
        }

        /// <summary>
        /// 进入战斗世界
        /// </summary>
        public void EnterBattleWorld()
        {
            if (playerConfigList == null || enemyConfigList == null)
            {
                throw new InvalidOperationException("Character data not set");
            }
                // 清理旧的战斗世界
                if (BattleWorld != null)
                {
                    BattleWorld.OnDestroy();
                    BattleWorld = null;
                }

               
                // 创建新的战斗世界
                WorldManager.CreateWorld<BattleWorld>();
                isInBattle = true;
                // 隐藏不相关的UI
                UIMgr.Instance.HideUI<GameTitle>();
        }

        public void Update()
        {
            if(isInBattle)
            {
                WorldManager.UpdateWorld();
            }
            
        }

        private void RegisterEvents()
        {

            EventCenter.Instance.AddListener("EnterBattleWorld", createBattleWorld);
        }

        private void UnregisterEvents()
        {

            EventCenter.Instance.RemoveListener("EnterBattleWorld");
        }

        public  void OnDestroy()
        {
            
            UnregisterEvents();

            if (BattleWorld != null)
            {
                BattleWorld.OnDestroy();
                BattleWorld = null;
            }
            if(playerConfigList != null)
            {
                for(int i=0;i<playerConfigList.Count;i++)
                {
                    playerConfigList[i] = null;
                }
                playerConfigList.Clear();
                playerConfigList = null;
            }
            if(enemyConfigList != null)
            {
                for(int i=0;i<enemyConfigList.Count;i++)
                {
                    enemyConfigList[i] = null;
                }
                enemyConfigList.Clear();
                enemyConfigList = null;
            }
        }
    }

