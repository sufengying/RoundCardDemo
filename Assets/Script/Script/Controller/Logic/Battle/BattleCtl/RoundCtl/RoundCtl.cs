using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    /// <summary>
    /// 根据行动值来安排角色的出手顺序的控制器
    /// </summary>
    public class RoundCtl : ILogicBehaviour
    {
        //当前排序队列
        public static List<CharacterLogic> SortList { get; private set; }
        //当前行动角色（行动值为0）
        public CharacterLogic CurrentCharacter { get; private set; }
        //下一个行动角色
        public CharacterLogic NextCharacter { get; private set; }
        //下一个行动的我方角色
        public CharacterLogic NextPlayerCharacter { get; private set; }

        //第一个行动的敌人,在初始化时计算，主要传递给LockCtl初始化首回合锁定的敌人
        public CharacterLogic FirstEnemy { get; private set; }
        //上一个行动的角色（只有行动过一次才会赋值），首回合不会赋值
        public CharacterLogic LastCharacter { get; private set; }

        private Func<List<CharacterLogic>> getSortList;
        private Func<string,CharacterLogic> getOtherCharacter;
        private Action<CharacterLogic> removeCharacterLogic;
        private Action onAction;
        public void OnCreate()
        {
            getSortList=GetSortList;
            getOtherCharacter=GetOtherCharacter;
            onAction=OnAction;
            removeCharacterLogic=RemoveCharacterLogic;
            RegisterEvent();

            SortList=EventCenter.Instance.TriggerAction<string,List<CharacterLogic>>("CharacterCtl_GetCharacterList","All");
            
            if(SortList==null||SortList.Count==0)
            {
                Debug.LogError("SortList-->列表为null或列表数量空");
                return;
            }
            SortList.Sort((a, b) => a.action.ActionValue.CompareTo(b.action.ActionValue));
            NormalizeTimeline();
            SetCharacter();
            FirstEnemy = SortList.Find(x => x != null && x.action != null && x.team == Team.Enemy);
            

        }
#region 事件注册
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("RoundCtl_GetSortList",getSortList);
            EventCenter.Instance.AddListener("RoundCtl_GetOtherCharacter",getOtherCharacter);
            EventCenter.Instance.AddListener("RoundCtl_OnAction",onAction);
            EventCenter.Instance.AddListener("RoundCtl_RemoveCharacterLogic",removeCharacterLogic);
        }
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("RoundCtl_GetSortList");
            EventCenter.Instance.RemoveListener("RoundCtl_GetOtherCharacter");
            EventCenter.Instance.RemoveListener("RoundCtl_OnAction");
            EventCenter.Instance.RemoveListener("RoundCtl_RemoveCharacterLogic");
        }
#endregion
        /// <summary>
        /// 执行行动,
        /// </summary>
        /// <param name="characterLogicList">执行行动的角色列表</param>
        public void Action( )
        {
            SortList[0].UpdateNextRoundActionValue();
            
            for (int i = 0; i < SortList.Count; i++)
            {
                if (SortList[i] != null)
                {
                    if(i==0)
                    {
                        LastCharacter = SortList[i];
                        continue;
                    }           
                    //行动过后重新计算行动值
                    SortList[i].UpdateActionValue();
                }
            }

            // 重新排序
            SortList.Sort((a, b) => a.action.ActionValue.CompareTo(b.action.ActionValue));

            // 再次归一化处理，推进时间轴到新的第一个行动者
            NormalizeTimeline();
            // 更新当前/下一个行动角色
            SetCharacter();
            

            EventCenter.Instance.TriggerAction("CameraCtl_OnActorChanged",CurrentCharacter);
            EventCenter.Instance.TriggerAction("BattleUICtl_UpdateRoundList",CurrentCharacter,SortList);

        }
#region 需要注册的事件
        private List<CharacterLogic> GetSortList()
        {
            return SortList;
        }
        private CharacterLogic GetOtherCharacter(string parName)
        {
            if(parName=="CurrentCharacter")
            {
                return CurrentCharacter;
            }
            else if(parName=="NextCharacter")
            {
                return NextCharacter;
            }
            else if(parName=="NextPlayerCharacter")
            {
                return NextPlayerCharacter;
            }
            else if(parName=="FirstEnemy")
            {
                return FirstEnemy;
            }
            else if(parName=="LastCharacter")
            {
                return LastCharacter;
            }
            else
            {
                Debug.LogError("parName-->参数错误");
                return null;
            }
        }
        //执行行动
        private void OnAction()
        {
             Action();
        }

        private void RemoveCharacterLogic(CharacterLogic characterLogic)
        {
            if(CurrentCharacter!=null)
            {
                SortList.Remove(characterLogic);   
                EventCenter.Instance.TriggerAction("BattleUICtl_RemoveCharacterUI",characterLogic);
            }
            
        }
#endregion
        // 归一化时间轴：将所有单位的时间向前推进 baseActionValue
        private void NormalizeTimeline()
        {
            if (SortList == null || SortList.Count == 0)
            {
                return;
            }
            // 获取当前行动值最低的角色所需的行动值（即需要推进的时间量）
            int baseActionValue = SortList[0].action.ActionValue;

            // 如果 baseActionValue 已经是 0 或负数，说明时间无需推进（或数据有误），直接返回
            if (baseActionValue <= 0) return;
            for (int i = 0; i < SortList.Count; i++)
            {
                if (SortList[i].action == null || SortList[i] == null)
                {
                    Debug.LogWarning($"Invalid character data at index {i} during timeline normalization.");
                    continue;
                }

                // 使用配置的最小速度值
                int speed = Mathf.Max(1, SortList[i].speed);
                //计算移动的距离
                int distanceMoved = Mathf.RoundToInt(speed * baseActionValue);
                // 更新剩余路程 (不能小于0)
                int nextRoundPath = Mathf.Max(0, SortList[i].action.nextRoundPath - distanceMoved);
                SortList[i].UpdateNextRoundPath(nextRoundPath);

                // 更新剩余行动时间
                SortList[i].UpdateActionValue();
            }
        }

        //调用这个方法用来设置当前行动角色，下一个行动角色，以及洗一个我方行动角色
        private void SetCharacter()
        {
            if (SortList == null || SortList.Count == 0)
            {
                CurrentCharacter = null;
                NextCharacter = null;
                NextPlayerCharacter = null;
                return;
            }
            CurrentCharacter = SortList[0];
            NextCharacter = SortList.Count > 1 ? SortList[1] : null;
            NextPlayerCharacter = SortList.Find(x => x != null && x.team == Team.Player &&
                x != CurrentCharacter);


        }

        // 修改Print方法，添加调用来源标识，并使用预处理指令
        public void Print(string source = "")
        {
#if UNITY_EDITOR || DEBUG
            if (SortList == null || SortList.Count == 0)
            {
                Debug.Log($"[{source}] Print called: sortList is empty.");
                return;
            }
            Debug.Log($"---------- RoundCtl Print ({source}) ----------");
            for (int i = 0; i < SortList.Count; i++)
            {
                if (SortList[i].action != null && SortList[i] != null)
                {
                    Debug.Log($"  [{i}] 角色: {SortList[i].name}, 行动值: {SortList[i].action.ActionValue}, 剩余路程: {SortList[i].action.nextRoundPath}, 速度: {SortList[i].speed}");
                }
                else
                {
                    Debug.LogWarning($"  [{i}] Invalid entry in sortList.");
                }
            }
            string nowName = (CurrentCharacter != null && CurrentCharacter.action != null) ? CurrentCharacter.name : "None";
            string nextName = (NextCharacter != null && NextCharacter.action != null) ? NextCharacter.name : "None";
            string nextPlayerName = (NextPlayerCharacter != null && NextPlayerCharacter.action != null) ? NextPlayerCharacter.name : "None";
            Debug.Log($"当前行动角色: {nowName}");
            Debug.Log($"下一个行动角色: {nextName}");
            Debug.Log($"下一个行动我方角色: {nextPlayerName}");
            Debug.Log($"--------------------------------------------");
#endif
        }

        public void OnDestroy()
        {
            // 移除事件监听
            UnregisterEvent();

            // 清理排序列表
            if (SortList != null)
            {
                for (int i = 0; i < SortList.Count; i++)
                {
                    if (SortList[i] != null)
                    {
                        SortList[i] = null;
                    }
                }
                SortList.Clear();
                SortList = null;
            }

            // 清理角色引用
            if (CurrentCharacter != null)
            {
                CurrentCharacter = null;
            }
            if (NextCharacter != null)
            {
                NextCharacter = null;
            }
            if (NextPlayerCharacter != null)
            {
                NextPlayerCharacter = null;
            }
            if (FirstEnemy != null)
            {
                FirstEnemy = null;
            }
            if (LastCharacter != null)
            {
                LastCharacter = null;
            }

            // 清理委托引用
            getSortList = null;
            getOtherCharacter = null;
            removeCharacterLogic = null;
            onAction = null;
        }
    }
}

