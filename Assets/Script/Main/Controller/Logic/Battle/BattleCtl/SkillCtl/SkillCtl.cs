using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ZMGCFrameWork.Battle
{
    public class SkillCtl : ILogicBehaviour
    {
        //当前技能施法者
        public CharacterLogic currentCharacterLogic;
        //当前释放的技能
        public SkillConfig currentSkill;

        private Action<CharacterLogic, SkillConfig> OnReleaseSkill;

        private List<CharacterLogic> targetList;
        private List<float[]> skillDamageArray;
        private int takenTimes;

        private int[] attackPreDelay;

        private List<float[]> hpRateArray;

        public void OnCreate()
        {
            OnReleaseSkill = ReleaseSkill;
            RegisterEvent();
        }
        #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("SkillCtl_ReleaseSkill", OnReleaseSkill);
        }
        private void UnRegisterEvent()
        {
            EventCenter.Instance.RemoveListener("SkillCtl_ReleaseSkill");
        }

        #endregion

        public void ReleaseSkill(CharacterLogic characterLogic, SkillConfig skill)
        {
            targetList = null;
            skillDamageArray = null;
            hpRateArray = null;
            takenTimes = 0;
            //当前释放者，当前释放技能
            currentCharacterLogic = characterLogic;
            currentSkill = skill;

            //更新角色行动点
            UpdateSkillPoints();

            //获得技能目标列表
            //List<CharacterLogic> targetList = null;
            GetTargetList(currentSkill);

            for (int i = targetList.Count - 1; i >= 0; i--)
            {

                if (targetList[i].aiState.GetState() == AIStateEnum.death)
                {
                    targetList.RemoveAt(i);
                }

            }
            if (targetList.Count == 0)
            {
                return;
            }

            //缓存技能数据
            UpdateSkillDate();

            //获得技能伤害数组，传入目标列表，技能生效次数，技能伤害数组。根据伤害计算规则，计算每个角色受到的技能伤害数组
            GetSkillDamageArray(currentSkill, targetList, takenTimes);

            //播放技能动画
            SkillRender();

            //更新目标状态
            UpdateCharacterState();
            // //获得技能每个行动轮次生效次数
            // //根据技能目标，技能每个行动轮次生效次数来得到每个角色受到的技能伤害数组
            // //获得血量比例数组，根据技能目标数量，声明对应数量列表元素数量。根据技能伤害次数，声明对应数量列表元素数量。
            // //int takenTimes = skill.takenTimes;
            // takenTimes = currentSkill.takenTimes;

            // //List<float[]> skillDamageArray = new List<float[]>(targetList.Count);
            // skillDamageArray = new List<float[]>(targetList.Count);
            // //List<float[]> hpRateArray = new List<float[]>(targetList.Count);
            // hpRateArray = new List<float[]>(targetList.Count);

            // //int[] attackPreDelay = skill.attackPreDelay;
            // attackPreDelay = currentSkill.attackPreDelay;

            // for (int i = 0; i < targetList.Count; i++)
            // {
            //     hpRateArray.Add(new float[takenTimes]);
            // }

            //获得技能伤害数组，传入目标列表，技能生效次数，技能伤害数组。根据伤害计算规则，计算每个角色受到的技能伤害数组




            // GameObject skillRenderParent = null;
            // //播放技能
            // switch (currentSkill.effectRenderedPos)
            // {
            //     case EffectRendered.None:

            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill);
            //         break;

            //     case EffectRendered.enemyMid:
            //         skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.enemyMid);
            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
            //         break;

            //     case EffectRendered.playerMid:
            //         skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.playerMid);
            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
            //         break;

            //     case EffectRendered.mapMid:
            //         skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.mapMid);
            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
            //         break;

            //     case EffectRendered.Self:
            //         GameObject self = currentCharacterLogic.characterRender.gameObject;
            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, self);
            //         break;

            //     case EffectRendered.TargetSingle:
            //         GameObject targetSingle = targetList[0].characterRender.gameObject;
            //         currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, targetSingle);
            //         break;

            //     case EffectRendered.TargetAll:
            //         break;
            // }


            // if (attackPreDelay == null)
            // {
            //     Debug.LogError("受到攻击的前摇数量为空");
            //     return;
            // }
            // //把伤害计算数组传给目标角色，更新目标的属性状态（逻辑层更新），获得血量比例数组
            // for (int i = 0; i < targetList.Count; i++)
            // {
            //     hpRateArray[i] = targetList[i].CalculateHP(skillDamageArray[i]);
            //     if (hpRateArray[i] == null || hpRateArray[i].Length != takenTimes)
            //     {
            //         Debug.LogError("血量比例数组为空或者血量比例数组长度与技能伤害次数不一致");
            //         return;
            //     }
            // }

            // //更新目标血条
            // for (int i = 0; i < targetList.Count; i++)
            // {
            //     if (attackPreDelay.Length != hpRateArray[i].Length)
            //     {
            //         Debug.LogError("受到攻击的前摇数量与血量更新比例数量不一致");
            //         return;
            //     }
            //     if (hpRateArray[i].Length != skillDamageArray[i].Length)
            //     {
            //         Debug.LogError("血量更新比例数量与技能伤害数量不一致");
            //         return;
            //     }
            //     //传入攻击前摇数组，血量比例数组，伤害数字数组，伤害数字预制体
            //     targetList[i].characterRender.UpdateHPSlider(attackPreDelay, hpRateArray[i], skillDamageArray[i], currentSkill.DamageNumberPrefab);
            // }


        }

        private void GetTargetList(SkillConfig skill)
        {
            switch (skill.skillTarget)
            {
                case SkillTarget.Self:
                    //技能目标为玩家方技能施法者本身
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.Self);
                    break;

                case SkillTarget.PlayerSingle:
                    //技能目标为玩家方角色单个角色
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.PlayerSingle);
                    break;

                case SkillTarget.playerAll:
                    //技能目标为所有玩家角色
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.playerAll);
                    break;

                case SkillTarget.playerExceptSelf:
                    //技能目标为所有玩家角色（不包括当前角色）
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.playerExceptSelf);
                    break;

                case SkillTarget.EnemySelf:
                    //技能目标为敌方技能施法者本身
                    targetList = new List<CharacterLogic>() { currentCharacterLogic };
                    break;

                case SkillTarget.EnemySingle:
                    //技能目标为单个敌方角色
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.EnemySingle);
                    break;

                case SkillTarget.EnemyAll:
                    //技能目标为所有敌方角色
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.EnemyAll);
                    break;

                case SkillTarget.EnemyExceptSelf:

                    break;

                case SkillTarget.All:
                    //技能目标为所有角色
                    targetList = EventCenter.Instance.TriggerAction<SkillTarget, List<CharacterLogic>>("LockCtl_GetLockList", SkillTarget.All);
                    break;
            }
        }
        private void GetSkillDamageArray(SkillConfig skill, List<CharacterLogic> targetList, int takenTimes)
        {
            if (takenTimes <= 0)
            {
                Debug.Log("技能生效次数为空");
                return;
            }
            if (targetList == null)
            {
                Debug.Log("技能目标为空");
                return;
            }
            //根据技能目标，技能每个行动轮次生效次数来得到每个角色受到的技能伤害数组
            for (int i = 0; i < targetList.Count; i++)
            {

                float[] damageArray = CalculateSkill(skill, targetList[i], takenTimes);
                skillDamageArray.Add(damageArray);
            }
            //如果技能目标数量与技能伤害数量不一致，则报错
            if (targetList.Count != skillDamageArray.Count)
            {
                Debug.LogError("技能目标数量与技能伤害数量不一致");
                return;
            }

        }
        public float[] CalculateSkill(SkillConfig skill, CharacterLogic target, int takenTimes)
        {

            float[] damageArray = null;
            switch (skill.skillDamageType)
            {
                case SkillDamageType.None:
                    //没有伤害
                    CalculateNone(takenTimes, ref damageArray);
                    break;
                case SkillDamageType.Physics:
                    //物理伤害
                    CalculatePhsics(skill, target, takenTimes, ref damageArray);
                    break;
                case SkillDamageType.Spell:
                    //魔法伤害
                    CalculateSpell(skill, target, takenTimes, ref damageArray);
                    break;
                case SkillDamageType.real:
                    //真实伤害
                    CalculateReal(skill, target, takenTimes, ref damageArray);
                    break;
            }
            return damageArray;
        }

        ///真实伤害
        private void CalculateReal(SkillConfig skill, CharacterLogic target, int takenTimes, ref float[] damageArray)
        {
            damageArray = new float[takenTimes];
            for (int i = 0; i < takenTimes; i++)
            {
                damageArray[i] = currentCharacterLogic.atk * skill.Magnificatio;
            }
        }
        //计算法术伤害
        //根据技能伤害次数来计算每次伤害
        //计算方式：释放技能角色的攻击力乘与技能倍率的值 乘与（100-目标魔法防御）除以100
        private void CalculateSpell(SkillConfig skill, CharacterLogic target, int takenTimes, ref float[] damageArray)
        {

            damageArray = new float[takenTimes];
            for (int i = 0; i < takenTimes; i++)
            {

                damageArray[i] = currentCharacterLogic.atk * skill.Magnificatio * ((100.0f - target.res) / 100.0f);

            }
        }
        //没有伤害类型
        //返回全是零的数组
        private void CalculateNone(int takenTimes, ref float[] damageArray)
        {
            damageArray = new float[takenTimes];
            Array.Clear(damageArray, 0, damageArray.Length);
        }
        //计算物理伤害
        //根据技能伤害次数来计算每次伤害
        //计算方式：释放技能角色的攻击力乘与技能倍率的值减去目标角色物理防御
        private void CalculatePhsics(SkillConfig skill, CharacterLogic target, int takenTimes, ref float[] damageArray)
        {

            damageArray = new float[takenTimes];
            for (int i = 0; i < takenTimes; i++)
            {
                damageArray[i] = currentCharacterLogic.atk * skill.Magnificatio - target.def;

            }

        }

        private void PrintDamageArray(List<float[]> skillDamageArray, List<CharacterLogic> targetList, SkillConfig skill, int takenTimes)
        {
            Debug.Log("技能者：" + currentCharacterLogic.name);
            Debug.Log("释放技能" + skill.skillName);
            Debug.Log("技能攻击次数" + takenTimes);
            for (int i = 0; i < targetList.Count; i++)
            {
                Debug.Log("技能目标：" + targetList[i].name);
                for (int j = 0; j < skillDamageArray[i].Length; j++)
                {
                    Debug.Log("技能伤害：" + skillDamageArray[i][j]);
                }
            }
        }

        private void UpdateSkillPoints()
        {
            //更新角色行动点
            currentCharacterLogic.UpdateSkillPoints(currentSkill.skillCost);
        }

        private void UpdateSkillDate()
        {
            //获得技能每个行动轮次生效次数
            //根据技能目标，技能每个行动轮次生效次数来得到每个角色受到的技能伤害数组
            //获得血量比例数组，根据技能目标数量，声明对应数量列表元素数量。根据技能伤害次数，声明对应数量列表元素数量。
            //int takenTimes = skill.takenTimes;
            takenTimes = currentSkill.takenTimes;

            //List<float[]> skillDamageArray = new List<float[]>(targetList.Count);
            skillDamageArray = new List<float[]>(targetList.Count);
            //List<float[]> hpRateArray = new List<float[]>(targetList.Count);
            hpRateArray = new List<float[]>(targetList.Count);

            //int[] attackPreDelay = skill.attackPreDelay;
            attackPreDelay = currentSkill.attackPreDelay;

            for (int i = 0; i < targetList.Count; i++)
            {
                hpRateArray.Add(new float[takenTimes]);
            }

        }
        private void SkillRender()
        {
            GameObject skillRenderParent = null;
            //播放技能
            switch (currentSkill.effectRenderedPos)
            {
                case EffectRendered.None:

                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill);
                    break;

                case EffectRendered.enemyMid:
                    skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.enemyMid);
                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
                    break;

                case EffectRendered.playerMid:
                    skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.playerMid);
                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
                    break;

                case EffectRendered.mapMid:
                    skillRenderParent = EventCenter.Instance.TriggerAction<EffectRendered, GameObject>("WorldDataCtl_GetSkillParent", EffectRendered.mapMid);
                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, skillRenderParent);
                    break;

                case EffectRendered.Self:
                    GameObject self = currentCharacterLogic.characterRender.gameObject;
                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, self);
                    break;

                case EffectRendered.TargetSingle:
                    GameObject targetSingle = targetList[0].characterRender.gameObject;
                    currentCharacterLogic.characterRender.skills.PlaySkill(currentSkill, targetSingle);
                    break;

                case EffectRendered.TargetAll:
                    break;
            }

        }

        private void UpdateCharacterState()
        {
            if (attackPreDelay == null)
            {
                Debug.LogError("受到攻击的前摇数量为空");
                return;
            }
            //把伤害计算数组传给目标角色，更新目标的属性状态（逻辑层更新），获得血量比例数组
            for (int i = 0; i < targetList.Count; i++)
            {
                hpRateArray[i] = targetList[i].SimulateCalculateHP(skillDamageArray[i]);
                if (hpRateArray[i] == null || hpRateArray[i].Length != takenTimes)
                {
                    Debug.LogError("血量比例数组为空或者血量比例数组长度与技能伤害次数不一致");
                    return;
                }
            }

            //更新目标血条
            for (int i = 0; i < targetList.Count; i++)
            {
                if (attackPreDelay.Length != hpRateArray[i].Length)
                {
                    Debug.LogError("受到攻击的前摇数量与血量更新比例数量不一致");
                    return;
                }
                if (hpRateArray[i].Length != skillDamageArray[i].Length)
                {
                    Debug.LogError("血量更新比例数量与技能伤害数量不一致");
                    return;
                }
                //传入攻击前摇数组，血量比例数组，伤害数字数组，伤害数字预制体
                targetList[i].characterRender.UpdateHPSlider(attackPreDelay, hpRateArray[i], skillDamageArray[i], currentSkill.DamageNumberPrefab);
            }

        }

        public void OnDestroy()
        {
            try
            {
                // 取消事件监听
                UnRegisterEvent();
                
                // 清理委托
                if (OnReleaseSkill != null)
                {
                    OnReleaseSkill = null;
                }
                
                // 清理列表
                if (targetList != null)
                {
                    targetList.Clear();
                    targetList = null;
                }
                
                if (skillDamageArray != null)
                {
                    foreach (var damageArray in skillDamageArray)
                    {
                        if (damageArray != null)
                        {
                            Array.Clear(damageArray, 0, damageArray.Length);
                        }
                    }
                    skillDamageArray.Clear();
                    skillDamageArray = null;
                }
                
                if (hpRateArray != null)
                {
                    foreach (var rateArray in hpRateArray)
                    {
                        if (rateArray != null)
                        {
                            Array.Clear(rateArray, 0, rateArray.Length);
                        }
                    }
                    hpRateArray.Clear();
                    hpRateArray = null;
                }
                
                // 清理数组
                if (attackPreDelay != null)
                {
                    attackPreDelay = null;
                }
                
                // 清理当前角色和技能引用
                if (currentCharacterLogic != null)
                {
                    currentCharacterLogic = null;
                }
                
                if (currentSkill != null)
                {
                    currentSkill = null;
                }
                
                // 重置计数器
                takenTimes = 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"SkillCtl OnDestroy Error: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
