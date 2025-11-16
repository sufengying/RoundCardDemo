using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZMGCFrameWork.Battle
{
    public class SkillReleaase : MonoBehaviour
    {
        [SerializeField] private Button release;
        [SerializeField] private Button cancel;

        private CharacterLogic currentCharacterLogic;
        private SkillConfig currentSkillConfig;
        private Action<CharacterLogic, SkillConfig> setDate;

        public void Initialize()
        {
            setDate = SetDate;
            RegisterEvent();

            AddListener();
        }
        #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("SkillReleaase_SetData", setDate);
        }
        private void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("SkillReleaase_SetData");
        }
        #endregion
        private void AddListener()
        {
            //把数据传递给SkillCtrl,然后释放技能
            release.onClick.AddListener(ReleaseSkill);
            //隐藏技能释放面板（canvasgroup），并把数据清空,取消
            cancel.onClick.AddListener(CancelSkill);
        }

        private void ReleaseSkill()
        {
            EventCenter.Instance.TriggerAction("LockCtl_CancelLockEffectAll");
            if (currentCharacterLogic != null && currentSkillConfig != null)
            {

                EventCenter.Instance.TriggerAction("SkillCtl_ReleaseSkill", currentCharacterLogic, currentSkillConfig);
            }
            CancelSkill();
        }

        private void CancelSkill()
        {

            currentCharacterLogic = null;
            currentSkillConfig = null;
            EventCenter.Instance.TriggerAction("LockCtl_CancelLockEffectAll");
            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setSkillReleaseCanvasGroup", false);
            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_cancelcurrent_buttonAndShadow");

        }

        private void SetDate(CharacterLogic characterLogic, SkillConfig skillConfig)
        {

            if (currentCharacterLogic != characterLogic)
            {
                currentCharacterLogic = characterLogic;
            }
            if (currentSkillConfig != skillConfig)
            {
                currentSkillConfig = skillConfig;
            }
        }

        public void OnDestroy()
        {
            // 清理按钮事件监听
            if (release != null)
            {
                release.onClick.RemoveListener(ReleaseSkill);
                release = null;
            }

            if (cancel != null)
            {
                cancel.onClick.RemoveListener(CancelSkill);
                cancel = null;
            }

            // 清理角色引用
            if (currentCharacterLogic != null)
            {
                currentCharacterLogic = null;
            }

            // 清理技能配置引用
            if (currentSkillConfig != null)
            {
                currentSkillConfig = null;
            }
        }


    }
}