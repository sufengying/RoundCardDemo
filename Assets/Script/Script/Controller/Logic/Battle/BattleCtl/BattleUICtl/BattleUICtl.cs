using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class BattleUICtl : ILogicBehaviour
    {
        // 战斗UI根对象引用
        private GameObject battleworldUIGameObject;
        // UI布局辅助组件引用
        private RoundChacater roundChacater;
        private ChacaterHead chacaterHead;
        private BattleSkillButtonUI battleSkillButtonUI;

        private Action<CharacterLogic,List<CharacterLogic>> updateRoundList;
        private Action<CharacterLogic> setSelectedChacaterHead;
        private Action<CharacterLogic> updateSkillButtonUI;
        private Action<CharacterLogic> removeCharacterUI;
        private Action<bool> hideBattleUI;

        public void OnCreate()
        {
            updateRoundList = UpdateRoundList;
            setSelectedChacaterHead = SetSelectedChacaterHead;
            updateSkillButtonUI = UpdateSkillButtonUI;
            removeCharacterUI = RemoveCharacterUI;
            hideBattleUI = HideBattleUI;
            RegisterEvent();



            battleworldUIGameObject = UIMgr.Instance.LoadBattleUI();
            roundChacater = battleworldUIGameObject.GetComponentInChildren<RoundChacater>();
            chacaterHead = battleworldUIGameObject.GetComponentInChildren<ChacaterHead>();
            battleSkillButtonUI = battleworldUIGameObject.GetComponentInChildren<BattleSkillButtonUI>();

            OnCreate_RoundChacater();
            OnCreate_chacaterHead();
            OnCreate_battleEventUI();
        }
 #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("BattleUICtl_UpdateRoundList",updateRoundList);
            EventCenter.Instance.AddListener("BattleUICtl_SetSelectedChacaterHead",setSelectedChacaterHead);
            EventCenter.Instance.AddListener("BattleUICtl_UpdateSkillButtonUI",updateSkillButtonUI);
            EventCenter.Instance.AddListener("BattleUICtl_RemoveCharacterUI",removeCharacterUI);
            EventCenter.Instance.AddListener("BattleUICtl_HideBattleUI",hideBattleUI);
        }       
        private void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("BattleUICtl_UpdateRoundList");
            EventCenter.Instance.RemoveListener("BattleUICtl_SetSelectedChacaterHead");
            EventCenter.Instance.RemoveListener("BattleUICtl_UpdateSkillButtonUI");
            EventCenter.Instance.RemoveListener("BattleUICtl_RemoveCharacterUI");
            EventCenter.Instance.RemoveListener("BattleUICtl_HideBattleUI");
        }

#endregion
        //初始化角色头像UI
        public void OnCreate_RoundChacater()
        {
            List<CharacterLogic> SortList=EventCenter.Instance.TriggerAction<List<CharacterLogic>>("RoundCtl_GetSortList");
            roundChacater.Initialize(SortList);
        }
        public void OnCreate_chacaterHead( )
        {
            List<CharacterLogic> playerLogicList=EventCenter.Instance.TriggerAction<string,List<CharacterLogic>>("CharacterCtl_GetCharacterList","Player");
            CharacterLogic currentFocusedPlayer=EventCenter.Instance.TriggerAction<CharacterLogic>("CameraCtl_GetCurrentFocusedPlayer");
            chacaterHead.Initialize(playerLogicList, currentFocusedPlayer);
        }
        public void OnCreate_battleEventUI()
        {
            CharacterLogic currentFocusedPlayer=EventCenter.Instance.TriggerAction<CharacterLogic>("CameraCtl_GetCurrentFocusedPlayer");
            battleSkillButtonUI.Initialize(currentFocusedPlayer);
        }

        //更新回合排序列表
        private void UpdateRoundList(CharacterLogic characterLogicLast, List<CharacterLogic> newSortList)
        {
            //更新回合排序列表
            roundChacater.UpdateRoundList(characterLogicLast,newSortList);
        }

        //更新选中角色头像UI(ChacaterHead,激活特效)
        private void SetSelectedChacaterHead(CharacterLogic character)
        {
            //更新选中角色头像UI(ChacaterHead)
            chacaterHead.SetSelected(character);
        }

        //更新技能按钮UI(BattleSkillButtonUI)
        private void UpdateSkillButtonUI(CharacterLogic character)
        {
            //更新技能按钮UI(BattleSkillButtonUI)
            battleSkillButtonUI.ChangedCharacter(character);
        }

        private void RemoveCharacterUI(CharacterLogic characterLogic)
        {
            roundChacater.RemoveCharacterUI(characterLogic);
        }

        public void HideBattleUI(bool isHide)
        {
            CanvasGroup canvasGroup=battleworldUIGameObject.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(isHide?0:1, 0.1f).SetEase(Ease.Linear);
            canvasGroup.interactable = !isHide;
            canvasGroup.blocksRaycasts = !isHide;
        }

        public void OnDestroy()
        {
            // 清理角色头像
            if (chacaterHead != null)
            {
                chacaterHead.OnDestroy();
                chacaterHead = null;
            }

            // 清理技能按钮UI
            if (battleSkillButtonUI != null)
            {
                battleSkillButtonUI.OnDestroy();
                battleSkillButtonUI = null;
            }

            // 清理UI根对象
            if (battleworldUIGameObject != null)
            {
                GameObject.Destroy(battleworldUIGameObject);
                battleworldUIGameObject = null;
            }

            // 移除所有事件监听
            UnRegisterEvent();
        }

    }
}
