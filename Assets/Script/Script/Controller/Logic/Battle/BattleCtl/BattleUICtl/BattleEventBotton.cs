using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ZMGCFrameWork.Battle
{
    public class BattleEventButton : MonoBehaviour
    {
        public CanvasGroup skillCanvasGroup;
        public Button button;
        public Image skillIcon;

        [LabelText("是否不走常规技能流程")]
        public bool isInvoke = false;
        public CanvasGroup shadow;
        private SkillConfig skill;
        private Action<Button, bool> setButtonInteractable;
        //private Action<int> checkSkillPoint;


        #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("BattleSkillButtonUI_setButtonInteractable", setButtonInteractable);
            //EventCenter.Instance.AddListener("BattleSkillButtonUI_checkSkillPoint", checkSkillPoint);
        }

        private void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("BattleSkillButtonUI_setButtonInteractable" );
           // EventCenter.Instance.RemoveListener("BattleSkillButtonUI_checkSkillPoint");
        }
        #endregion
        public void Initialize()
        {

            setButtonInteractable = SetButtonInteractable;
            //checkSkillPoint = CheckSkillPoint;
            RegisterEvent();
            AddListener();

        }

        public void SetData(SkillConfig skill)
        {
            this.skill = skill;
            skillIcon.sprite = skill.skillIcon;
        }

        public void AddListener()
        {
            if (isInvoke)
            {
                return;
            }
            button.onClick.AddListener(Trigger);

        }

        public void CheckSkillPoint(int currentSkillPoint)
        {
            Debug.Log("CheckSkillPoint:"+currentSkillPoint);
            if(currentSkillPoint < skill.skillCost)
            {
                skillCanvasGroup.alpha = 0.5f;
                button.interactable = false;
            }
            else
            {
                skillCanvasGroup.alpha = 1f;
                button.interactable = true;
            }
        }

        private void Trigger()
        {
            SetButtonInteractable(button, false);
            //激活按钮选中特效
            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setShadowAndButtonInteractable", button, shadow);
            // 显示技能释放界面
            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setSkillReleaseCanvasGroup", true);
            //给技能释放界面设置数据
            EventCenter.Instance.TriggerAction("SkillReleaase_SetData",
                                                EventCenter.Instance.TriggerAction<CharacterLogic>("BattleSkillButtonUI_getCurrentCharacter"),
                                                skill);
            //激活锁定Ctrl的选中特效
            EventCenter.Instance.TriggerAction("LockCtl_SetetLockListEffect", skill.skillTarget);
        }

        private void SetButtonInteractable(Button button, bool isInteractable)
        {
            button.interactable = isInteractable;
        }

        public void OnDestroy()
        {
            // 移除按钮点击事件监听
            if (button != null)
            {
                button.onClick.RemoveListener(Trigger);
                button = null;
            }

            if (skill != null)
            {
                skill = null;
            }

            // 移除所有事件监听
            UnRegisterEvent();
            
            // 清理委托
            setButtonInteractable = null;
        }
    }
}