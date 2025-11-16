using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class ActionCtl : ILogicBehaviour
    {
        public static int RoundNums = 0;
        private CharacterLogic currentCharacter;

        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public Action triggerEvent;
        public Action setCurrentCharacter;

        // 记录每个技能的使用次数
        private Dictionary<SkillConfig, int> skillUsageCount = new Dictionary<SkillConfig, int>();

        public void OnCreate()
        {
            triggerEvent = TriggerEvent;
            setCurrentCharacter = SetCurrentCharacter;
            RegisterEvent();

            AIStateCtl.Instance.Initialize();

            //SetCurrentCharacter();
        }
        #region 注册事件
        public void RegisterEvent()
        {
            EventCenter.Instance.AddListener("ActionCtl_TriggerEvent", triggerEvent);
            EventCenter.Instance.AddListener("ActionCtl_SetCurrentCharacter", setCurrentCharacter);
        }
        public void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("ActionCtl_TriggerEvent", triggerEvent);
            EventCenter.Instance.RemoveListener("ActionCtl_SetCurrentCharacter", setCurrentCharacter);
        }
        #endregion

        public void SetCurrentCharacter()
        {
            currentCharacter = EventCenter.Instance.TriggerAction<string, CharacterLogic>("RoundCtl_GetOtherCharacter", "CurrentCharacter");
            //检查技能点
            if(currentCharacter.team == Team.Player)
            {
                EventCenter.Instance.TriggerAction<CharacterLogic>("BattleUICtl_UpdateSkillButtonUI", currentCharacter);
            }
            ActionStart();
        }

        public void ActionStart()
        {
            RoundNums++;
            if(currentCharacter.aiState.GetState()!=AIStateEnum.death)
            {
                AIStateCtl.Instance.ChangeState(AIStateEnum.going, currentCharacter);
            }
            _ = OnActioningAsync();
        }


        public async Task OnActioningAsync()
        {
            if (currentCharacter.team == Team.Enemy)
            {
                EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setCurrentCanvasGroup", false);
                EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setAviodCanvasGroup", true);
                SkillConfig skillConfig = null;
                if (currentCharacter.skillList.Count > 0 && currentCharacter.skillList != null)
                {
                    skillConfig = GetFairRandomSkill(currentCharacter);
                    EventCenter.Instance.TriggerAction<CharacterLogic, SkillConfig>("SkillCtl_ReleaseSkill", currentCharacter, skillConfig);
                }
            }
            await _tcs.Task;
            if(currentCharacter.aiState.GetState()!=AIStateEnum.death)
            {
                currentCharacter.characterRender.transform.localPosition = Vector3.zero;
            }
            OnActionAfter();
        }

        /// <summary>
        /// 获取一个相对公平的随机技能
        /// </summary>
        private SkillConfig GetFairRandomSkill(CharacterLogic character)
        {
            if (character == null || character.skillList == null || character.skillList.Count == 0)
            {
                Debug.LogWarning("角色没有可用技能");
                return null;
            }

            // 初始化技能使用次数记录
            foreach (var skill in character.skillList)
            {
                if (!skillUsageCount.ContainsKey(skill))
                {
                    skillUsageCount[skill] = 0;
                }
            }

            // 找出使用次数最少的技能
            int minUsage = skillUsageCount.Values.Min();
            var leastUsedSkills = character.skillList.Where(skill => skillUsageCount[skill] == minUsage).ToList();

            // 从使用次数最少的技能中随机选择一个
            int randomIndex = UnityEngine.Random.Range(0, leastUsedSkills.Count);
            SkillConfig selectedSkill = leastUsedSkills[randomIndex];

            // 更新使用次数
            skillUsageCount[selectedSkill]++;

            return selectedSkill;
        }

        public void OnActionAfter()
        {
            if(currentCharacter.aiState.GetState()==AIStateEnum.death)
            {
                currentCharacter.characterRender.OnEnterAnimation("dead");
            }
            else
            {
                AIStateCtl.Instance.ChangeState(AIStateEnum.waiting, currentCharacter);
            }
            ActionEnd();
        }

        public void ActionEnd()
        {
            if(currentCharacter.team == Team.Enemy)
            {
                EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setAviodCanvasGroup", false);
            }

            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setCurrentCanvasGroup", true);

            EventCenter.Instance.TriggerAction("RoundCtl_OnAction");
            ResetEvent();
            SetCurrentCharacter();
        }

        public void TriggerEvent()
        {
            _tcs.SetResult(true);
        }

        public void ResetEvent()
        {
            _tcs = new TaskCompletionSource<bool>();
        }

        public void OnDestroy()
        {
            UnRegisterEvent();
            triggerEvent = null;
            _tcs = null;
            currentCharacter = null;
            skillUsageCount.Clear(); // 清理技能使用记录
        }


    }
}
