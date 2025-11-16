using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace ZMGCFrameWork.Battle
{
    public class BattleSkillButtonUI : MonoBehaviour
    {
        public List<BattleEventButton> SkillButtons;
        [SerializeField] private SkillReleaase skillReleaase;
        [SerializeField] private CanvasGroup skillReleaseCanvasGroup;
        private Button currentButton;
        private CanvasGroup currentShadow;
        private CharacterLogic currentCharacterLogic;


        [TitleGroup("格挡设置")]
        [SerializeField] private CanvasGroup aviodCanvasGroup;
        [SerializeField] private BlockItem blockItem;

        [TitleGroup("反击技能设置")]
        [SerializeField] private CanvasGroup blockSkillCanvasGroup;
        [SerializeField] private BlockSkill blockSkill;


        private CanvasGroup currentCanvasGroup;
        private Action<Button, CanvasGroup> setShadowAndButtonInteractable;
        private Func<CharacterLogic> getCurrentCharacter;
        private Action<bool> setSkillReleaseCanvasGroup;
        private Action cancelcurrent_buttonAndShadow;

        private Action<bool> setCurrentCanvasGroup;
        private Action<bool> setAviodCanvasGroup;

        private Action<bool> setBlockSkillCanvasGroup;

        //初始化
        public void Initialize(CharacterLogic characterLogic)
        {
            setShadowAndButtonInteractable = SetShadowAndButtonInteractable;
            getCurrentCharacter = GetCurrentCharacter;
            setSkillReleaseCanvasGroup = SetSkillReleaseCanvasGroup;
            cancelcurrent_buttonAndShadow = Cancelcurrent_buttonAndShadow;
            setCurrentCanvasGroup = SetCurrentCanvasGroup;
            setAviodCanvasGroup = SetAviodCanvasGroup;
            setBlockSkillCanvasGroup = SetBlockSkillCanvasGroup;
            RegisterEvent();

            currentCanvasGroup = GetComponent<CanvasGroup>();

            skillReleaseCanvasGroup.alpha = 0;
            skillReleaseCanvasGroup.blocksRaycasts = false;
            skillReleaseCanvasGroup.interactable = false;
            for (int i = 0; i < SkillButtons.Count; i++)
            {
                SkillButtons[i].Initialize();
            }
            if (characterLogic != null && characterLogic.team == Team.Player)
            {
                ChangedCharacter(characterLogic);
            }
            else
            {
                Debug.Log("BattleSkillButtonUI_不是玩家角色或者逻辑体为空");
            }
            blockItem.Initialize(characterLogic);
            blockSkill.Initialize(characterLogic);
            skillReleaase.Initialize();

        }
        #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setShadowAndButtonInteractable", setShadowAndButtonInteractable);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_getCurrentCharacter", getCurrentCharacter);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setSkillReleaseCanvasGroup", setSkillReleaseCanvasGroup);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_cancelcurrent_buttonAndShadow", cancelcurrent_buttonAndShadow);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setCurrentCanvasGroup", setCurrentCanvasGroup);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setAviodCanvasGroup", setAviodCanvasGroup);
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setBlockSkillCanvasGroup", setBlockSkillCanvasGroup);
        }
        private void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setShadowAndButtonInteractable");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_getCurrentCharacter");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setSkillReleaseCanvasGroup");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_cancelcurrent_buttonAndShadow");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setCurrentCanvasGroup");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setAviodCanvasGroup");
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setBlockSkillCanvasGroup");
        }
        #endregion
        public void ChangedCharacter(CharacterLogic characterLogic)
        {
            
            currentCharacterLogic = characterLogic;
            blockItem.SetCurrentCharacter(currentCharacterLogic);
            blockSkill.SetCurrentCharacter(currentCharacterLogic);
            if (currentCharacterLogic != null)
            {
                Debug.Log("ChangedCharacter:"+characterLogic.name+"  "+currentCharacterLogic.currentSkillPoints);
                for (int i = 0; i < currentCharacterLogic.skillList.Count; i++)
                {
                    SkillButtons[i].SetData(currentCharacterLogic.skillList[i]);
                    if(SkillButtons[i].isInvoke==false)
                    {
                        SkillButtons[i].CheckSkillPoint(currentCharacterLogic.currentSkillPoints);
                    }
                    
                }
            }

        }

        private void SetShadowAndButtonInteractable(Button button, CanvasGroup canvasGroup)
        {
            if (currentButton == null)
            {
                currentButton = button;
            }
            else
            {
                if (currentButton != button)
                {
                    EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setButtonInteractable", currentButton, true);
                    currentButton = button;
                }
            }
            if (currentShadow == null)
            {
                currentShadow = canvasGroup;
                currentShadow.alpha = 1;
            }
            else
            {
                if (currentShadow != canvasGroup)
                {
                    currentShadow.alpha = 0;
                    currentShadow = canvasGroup;
                    currentShadow.alpha = 1;
                }
            }
        }

        private void Cancelcurrent_buttonAndShadow()
        {
            if (currentButton != null)
            {
                EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setButtonInteractable", currentButton, true);
                currentButton = null;
            }
            if (currentShadow != null)
            {
                currentShadow.alpha = 0;
                currentShadow = null;
            }
        }
        private CharacterLogic GetCurrentCharacter()
        {
            if (currentCharacterLogic != null)
            {
                return currentCharacterLogic;
            }
            else
            {
                return null;
            }
        }

        private void SetCurrentCanvasGroup(bool isShow)
        {
            if (isShow)
            {
                currentCanvasGroup.DOFade(1, 0.1f);
                currentCanvasGroup.blocksRaycasts = true;
                currentCanvasGroup.interactable = true;
            }
            else
            {
                currentCanvasGroup.DOFade(0, 0.1f);
                currentCanvasGroup.blocksRaycasts = false;
                currentCanvasGroup.interactable = false;
            }
        }
        private void SetSkillReleaseCanvasGroup(bool isShow)
        {

            if (isShow)
            {
                skillReleaseCanvasGroup.DOFade(1, 0.1f);
                skillReleaseCanvasGroup.blocksRaycasts = true;
                skillReleaseCanvasGroup.interactable = true;
            }
            else
            {
                skillReleaseCanvasGroup.DOFade(0, 0.1f);
                skillReleaseCanvasGroup.blocksRaycasts = false;
                skillReleaseCanvasGroup.interactable = false;
            }

        }
        private void SetAviodCanvasGroup(bool isShow)
        {

            aviodCanvasGroup.alpha = isShow ? 1 : 0;
            aviodCanvasGroup.blocksRaycasts = isShow;
            aviodCanvasGroup.interactable = isShow;
        }

        private void SetBlockSkillCanvasGroup(bool isShow)
        {
            if (isShow)
            {
                SetAviodCanvasGroup(false);
                blockSkillCanvasGroup.blocksRaycasts = true;
                blockSkillCanvasGroup.interactable = true;
                blockSkillCanvasGroup.DOFade(1, 0.1f).OnComplete(() =>
                {
                    blockSkill.CompleteInDuration(2f);

                });
            }
            else
            {
                blockSkillCanvasGroup.blocksRaycasts = false;
                blockSkillCanvasGroup.interactable = false;
                blockSkillCanvasGroup.DOFade(0, 0.1f);
            }
        }

        public void OnDestroy()
        {
            // 清理技能按钮列表
            if (SkillButtons != null)
            {
                for (int i = 0; i < SkillButtons.Count; i++)
                {
                    if (SkillButtons[i] != null)
                    {
                        SkillButtons[i].OnDestroy();
                        SkillButtons[i] = null;
                    }
                }
                SkillButtons.Clear();
                SkillButtons = null;
            }

            // 清理角色引用
            if (currentCharacterLogic != null)
            {
                currentCharacterLogic = null;
            }

            // 移除所有事件监听
            UnRegisterEvent();

            // 清理委托
            setCurrentCanvasGroup = null;
            setAviodCanvasGroup = null;
            setShadowAndButtonInteractable = null;
            setSkillReleaseCanvasGroup = null;
            getCurrentCharacter = null;
        }


    }
}