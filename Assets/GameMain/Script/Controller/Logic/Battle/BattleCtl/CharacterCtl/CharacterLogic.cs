using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ZMGCFrameWork.Battle;

namespace ZMGCFrameWork.Battle
{
    //角色的逻辑体，负责数据运算，发出指令让渲染体执行表现
    public class CharacterLogic : LogicObject
    {
        // 用于生成唯一实例 ID 的静态计数器
        private static int nextInstanceID = 0;
        // 角色的唯一实例 ID
        public int InstanceID { get; private set; }
        //角色渲染体
        public CharacterRender characterRender { get; private set; }

        #region 角色血量
        //角色当前血量
        public float currentHp { get; private set; }
        //角色最大血量
        public float maxHp { get; private set; }
        #endregion

        #region 角色技能点
        //角色当前技能点
        public int currentSkillPoints { get; private set; }
        //角色初始技能点
        public int skillPoints { get; private set; }
        //角色技能点最大值
        public int maxSkillPoints { get; private set; }
        #endregion

        #region 角色属性
        //角色名字
        public string name { get; private set; }
        //攻击力
        public float atk { get; private set; }
        //物理防御
        public float def { get; private set; }
        //魔法防御
        public float res { get; private set; }
        //角色速度
        public int speed { get; private set; }
        //角色阵营
        public Team team { get; private set; }
        #endregion

        #region 角色行动值
        //角色行动值
        public CalculateActionValue action { get; private set; }
        #endregion
        //角色的在地图上的位置id-->地图上有若干个设置好的带编号的位置，角色会根据位置id生成到对应的地图位置
        //角色头像UI的位置（与战斗站位保持一致）
        public int posID { get; private set; }
        //角色技能列表
        public List<SkillConfig> skillList { get; private set; }
        //角色的配置数据
        public CharacterConfig m_characterConfig { get; private set; }

        public AIStateBase aiState;

        private Action onUpdateActionValue;
        private Action onUpdateNextRoundActionValue;
        private Action<int> onUpdateNextRoundPath;
        private Action<int> onUpdateSkillPoints;

        //战斗世界创建时，为这个玩家初始化(输入玩家信息，阵营)
        public CharacterLogic(CharacterConfig characterConfig)
        {
            m_characterConfig = characterConfig;
        }

        public override void Initialize()
        {
            //注册事件
            onUpdateActionValue = UpdateActionValue;
            onUpdateNextRoundActionValue = UpdateNextRoundActionValue;
            onUpdateNextRoundPath = UpdateNextRoundPath;
            onUpdateSkillPoints = UpdateSkillPoints;
            RegisterEvent();

            name = m_characterConfig.characterName;
            speed = m_characterConfig.speed;
            atk = m_characterConfig.atk;
            def = m_characterConfig.def;
            res = m_characterConfig.res;

            this.team = m_characterConfig.team;
            this.posID = (int)m_characterConfig.posID;
            this.maxHp = m_characterConfig.maxHp;
            this.maxSkillPoints = m_characterConfig.maxSkillPoints;
            this.skillPoints = m_characterConfig.firstSkillPoints;

            //当前血量初始为最大血量
            this.currentHp = maxHp;
            //当前技能点初始为初始技能点
            this.currentSkillPoints = skillPoints;

            //初始化技能列表
            this.skillList = m_characterConfig.skillList;

            // 为此实例分配一个唯一的 ID
            InstanceID = System.Threading.Interlocked.Increment(ref nextInstanceID);

            //初始化状态
            aiState = new WaitingState(AIStateEnum.waiting);

            action = new CalculateActionValue();
            action.InitializeActionValue(speed);

        }
        #region 事件注册
        //注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("CharacterLogic_UpdateActionValue", onUpdateActionValue);
            EventCenter.Instance.AddListener("CharacterLogic_UpdateNextRoundActionValue", onUpdateNextRoundActionValue);
            EventCenter.Instance.AddListener("CharacterLogic_UpdateNextRoundPath", onUpdateNextRoundPath);
            EventCenter.Instance.AddListener("CharacterLogic_UpdateSkillPoints", onUpdateSkillPoints);
        }
        //注销事件
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("CharacterLogic_UpdateActionValue");
            EventCenter.Instance.RemoveListener("CharacterLogic_UpdateNextRoundActionValue");
            EventCenter.Instance.RemoveListener("CharacterLogic_UpdateNextRoundPath");
            EventCenter.Instance.RemoveListener("CharacterLogic_UpdateSkillPoints");
        }
        #endregion

        public void SetCharacterRender()
        {
            characterRender = (CharacterRender)renderObject;
        }
        /// <summary>
        /// 模拟计算血量,通过传过来的伤害数组，来计算当前血量，获得血量比例数组，渲染层更新血条时会根据这个数组更新血条
        /// 只是模拟，并不会真的更新状态
        /// </summary>
        /// <param name="value">伤害数组</param>
        /// <returns>血量比例数组</returns>
        public float[] SimulateCalculateHP(float[] value)
        {
            float[] hpRateArray = new float[value.Length];
            if (value == null)
            {
                Debug.LogError("value is null");
                return null;
            }

            if (value.Length == 0)
            {
                Debug.LogError("value is empty");
                return null;
            }

            if (hpRateArray.Length != value.Length)
            {
                Debug.LogError("血量比例数组与伤害数组长度不一致");
                return null;
            }
            //每计算一次，更新一次血条
            //float simulateCurrentHp = currentHp;
            for (int i = 0; i < value.Length; i++)
            {
                //simulateCurrentHp -= value[i];
                //simulateCurrentHp = Mathf.Clamp(simulateCurrentHp, 0, maxHp); //防止血量小于0或大于最大值
                float hpRate = CalculateHPRate(value[i]);
                hpRateArray[i] = hpRate;
            }
            return hpRateArray;
        }
        /// <summary>
        /// 真实计算血量，通过传过来的伤害值，来计算当前血量，获得血量比例数组，渲染层更新血条时会根据这个数组更新血条
        /// </summary>
        /// <param name="value"></param>
        public void RealCalculateHP(float value)
        {
            currentHp -= value;
            currentHp = Mathf.Clamp(currentHp, 0, maxHp); //防止血量小于0或大于最大值
            if (currentHp <= 0)
            {


                AIStateCtl.Instance.ChangeState(AIStateEnum.death, this);

                characterRender.skills.StopSkill();
                characterRender.OnEnterAnimation("dead");

                EventCenter.Instance.TriggerAction("CharacterCtl_RemoveCharacterLogic", this, team);
            }

        }
        /// <summary>
        /// 根据传来的血量计算血量百分比
        /// </summary>
        /// <returns></returns>
        public float CalculateHPRate(float value)
        {
            return value / maxHp;
        }

        public float CalculateHPRate()
        {
            return currentHp / maxHp;
        }

        public void UpdateSkillPoints(int value)
        {
            Debug.Log("UpdateSkillPointsbefore:"+value);
            currentSkillPoints -= value;
            Debug.Log("UpdateSkillPointsafter:"+currentSkillPoints);
            currentSkillPoints = Mathf.Clamp(currentSkillPoints, 0, maxSkillPoints);

            if (team == Team.Player)
            {
                characterRender.chacaterHeadUI.UpdateSkillPoints(currentSkillPoints);
            }

        }

        public void AddSkillPoints(int value)
        {
            currentSkillPoints += value;
            currentSkillPoints = Mathf.Clamp(currentSkillPoints, 0, maxSkillPoints);
            if (team == Team.Player)
            {
                characterRender.chacaterHeadUI.UpdateSkillPoints(currentSkillPoints);
            }
        }

        public void Block()
        {
            if (team == Team.Player && aiState.GetState() == AIStateEnum.waiting)
            {
               
                characterRender.PlayHighlightEffect(true);
                AIStateCtl.Instance.ChangeState(AIStateEnum.block, this);
                characterRender.OnEnterAnimation("block");
            }
        }

        public void Counterattack()
        {
          

            EventCenter.Instance.TriggerAction("SkillCtl_ReleaseSkill", this, skillList[4]);
          
            characterRender.OnExitBlock();
            characterRender.OnEnterAnimation("Idle");
        }


        #region 需要注册的事件
        public void UpdateActionValue()
        {
            action.UpdateActionValue(speed);
        }
        public void UpdateNextRoundActionValue()
        {
            action.UpdateNextRoundActionValue(speed);
        }
        public void UpdateNextRoundPath(int value)
        {
            action.UpdateNextRoundPath(value);
        }
        #endregion
        /// <summary>
        /// 销毁角色数据，释放内存
        /// </summary>
        /// 
        public override void Destroy()
        {
            // 移除事件监听
            UnregisterEvent();

            // 清理渲染相关引用
            if (renderObject != null)
            {
                renderObject = null;
            }
            if (characterRender != null)
            {
                characterRender = null;
            }

            // 清理技能列表
            if (skillList != null)
            {
                skillList = null;
            }

            // 清理配置数据
            if (m_characterConfig != null)
            {
                m_characterConfig = null;
            }

            // 清理其他引用
            if (name != null)
            {
                name = null;
            }

            // 清理状态相关
            if (aiState != null)
            {
                aiState = null;
            }

            // 清理其他可能的引用
            currentHp = 0;
            maxHp = 0;
            posID = -1;
            team = Team.Player;
        }
    }
}