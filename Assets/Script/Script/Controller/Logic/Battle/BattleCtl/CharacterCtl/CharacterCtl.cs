using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using ZM.UI;

namespace ZMGCFrameWork.Battle
{
    public class CharacterCtl : ILogicBehaviour
    {
        public List<CharacterLogic> playerLogicLsit;
        public List<CharacterLogic> enemyLogicList;
        public List<CharacterLogic> AllLogicList;

        private Func<string, List<CharacterLogic>> getCharacterList;
        private Action<CharacterLogic, Team> removeCharacterLogic;
        public void OnCreate()
        {
            getCharacterList = GetCharacterLogicList;
            removeCharacterLogic = RemoveCharacterLogic;
            RegisterEvent();

            playerLogicLsit = new List<CharacterLogic>();
            enemyLogicList = new List<CharacterLogic>();
            AllLogicList = new List<CharacterLogic>();

            CreatCharacterLogic();
        }
        #region 注册事件

        //注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("CharacterCtl_GetCharacterList", getCharacterList);
            EventCenter.Instance.AddListener("CharacterCtl_RemoveCharacterLogic", removeCharacterLogic);
        }
        //注销事件
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("CharacterCtl_GetCharacterList");
            EventCenter.Instance.RemoveListener("CharacterCtl_RemoveCharacterLogic");
        }
        #endregion
        public void CreatCharacterLogic()
        {
            List<CharacterConfig> playerConfigList = EventCenter.Instance.TriggerAction<string, List<CharacterConfig>>("WorldDataCtl_GetCharacterConfigList", "player");
            List<CharacterConfig> enemyConfigList = EventCenter.Instance.TriggerAction<string, List<CharacterConfig>>("WorldDataCtl_GetCharacterConfigList", "enemy");
            Transform[] playerPosArr = EventCenter.Instance.TriggerAction<string, Transform[]>("WorldDataCtl_GetCharacterPositions", "player");
            Transform[] enemyPosArr = EventCenter.Instance.TriggerAction<string, Transform[]>("WorldDataCtl_GetCharacterPositions", "enemy");

            //创建玩家角色
            CreatCharacter(playerConfigList, playerLogicLsit, playerPosArr);
            //创建敌方角色
            CreatCharacter(enemyConfigList, enemyLogicList, enemyPosArr);

        }

        private void CreatCharacter(List<CharacterConfig> characterConfigList, List<CharacterLogic> characterLogicList, Transform[] characterPosArr)
        {

            for (int i = 0; i < characterConfigList.Count; i++)
            {
                CharacterLogic characterLogic = new CharacterLogic(characterConfigList[i]);
                characterLogic.Initialize();
               
                GameObject characterObj = ResourcesMgr.Instance.InstantiateGameObject(characterLogic.m_characterConfig.characterPrefab, characterPosArr[(int)characterLogic.m_characterConfig.posID], true, true, true);
                CharacterRender characterRender = characterObj.GetComponent<CharacterRender>();

                //给角色的逻辑体和渲染体设置对应的渲染体和逻辑题
                characterRender.SetLogic0bject(characterLogic);
                characterLogic.SetRenderObiect(characterRender);
                characterLogic.SetCharacterRender();
                characterRender.SetCharacterLogic();

                //初始化渲染体和技能列表（表现层）
                characterRender.Initialize(characterConfigList[i]);
                characterRender.SetSkills();

                characterLogicList.Add(characterLogic);
                AllLogicList.Add(characterLogic);
            }
        }

        public void RemoveCharacterLogic(CharacterLogic characterLogic, Team teamType)
        {
            if (teamType == Team.Player)
            {
                playerLogicLsit.Remove(characterLogic);
            }
            else if (teamType == Team.Enemy)
            {
                enemyLogicList.Remove(characterLogic);
            }
            AllLogicList.Remove(characterLogic);

            if (playerLogicLsit.Count == 0)
            {
              
                EventCenter.Instance.TriggerAction("BattleUICtl_HideBattleUI",true);
                UIModule.Instance.PopUpWindow<FailureWindow>();

            }
            else if (enemyLogicList.Count == 0)
            {
              
                EventCenter.Instance.TriggerAction("BattleUICtl_HideBattleUI",true);
                TheWorld.Instance.ExecuteTimeStop(4f, 2f,null, () =>
                {
                    DarkMaskWindow.Instance.ShowAndHideDarkMaskWindow(null,
                    actionMiddle:CutSceneWindow.Instance.ShowCutSceneWindow,
                    AfterMiddle:()=>
                    {
                        CutSceneWindow.Instance.ResetSliderValue();
                        TheWorld.Instance.ResetFov();
                    },

                    actionEnd:()=>
                    {
                        CutSceneWindow.Instance.PlayAnimation();
                    },
                    
                    () =>
                    {

                        WorldManager.DestroyWorld<BattleWorld>();
                        UIModule.Instance.PopUpWindow<HallWindow>();
                        UIModule.Instance.PopUpWindow<BattleWorldWindow>();
                    });
                });
            }
            EventCenter.Instance.TriggerAction("RoundCtl_RemoveCharacterLogic", characterLogic);

        }

        #region 需要注册的事件
        /// <summary>
        /// 根据列表名称获取角色逻辑列表
        /// </summary>
        /// <param name="listName">列表名称(player,enemy,all)</param>
        /// <returns>角色逻辑列表</returns>
        public List<CharacterLogic> GetCharacterLogicList(string listName)
        {
            if (listName == "Player")
            {
                return playerLogicLsit;
            }
            else if (listName == "Enemy")
            {
                return enemyLogicList;
            }
            else if (listName == "All")
            {
                return AllLogicList;
            }
            return null;
        }
        #endregion
        public void OnDestroy()
        {
            // 移除事件监听
            UnregisterEvent();

            // 清理所有角色列表
            if (AllLogicList != null)
            {
                for (int i = 0; i < AllLogicList.Count; i++)
                {
                    if (AllLogicList[i] != null)
                    {
                        // 先销毁角色渲染体数据
                        if (AllLogicList[i].characterRender != null)
                        {
                            AllLogicList[i].characterRender.OnDestroy();
                        }
                        // 再销毁角色逻辑体数据
                        AllLogicList[i].Destroy();
                        // 最后置空引用
                        AllLogicList[i] = null;
                    }
                }
                AllLogicList.Clear();
                AllLogicList = null;
            }

            // 清理玩家角色列表
            if (playerLogicLsit != null)
            {
                for (int i = 0; i < playerLogicLsit.Count; i++)
                {
                    if (playerLogicLsit[i] != null)
                    {
                        if (playerLogicLsit[i].characterRender != null)
                        {
                            playerLogicLsit[i].characterRender.OnDestroy();
                        }
                        playerLogicLsit[i].Destroy();
                        playerLogicLsit[i] = null;
                    }
                }
                playerLogicLsit.Clear();
                playerLogicLsit = null;
            }

            // 清理敌人角色列表
            if (enemyLogicList != null)
            {
                for (int i = 0; i < enemyLogicList.Count; i++)
                {
                    if (enemyLogicList[i] != null)
                    {
                        if (enemyLogicList[i].characterRender != null)
                        {
                            enemyLogicList[i].characterRender.OnDestroy();
                        }
                        enemyLogicList[i].Destroy();
                        enemyLogicList[i] = null;
                    }
                }
                enemyLogicList.Clear();
                enemyLogicList = null;
            }
        }
    }
}
