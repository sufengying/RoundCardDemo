using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HighlightPlus;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class LockCtl : ILogicBehaviour
    {
        //是否开启锁定特效
        private bool isLockEffecft;
        //目前进行锁定行为的玩家角色
        public CharacterLogic currentCharacterLogic;
        //目前锁定的目标
        public CharacterLogic lockCharacter;

        // 旋转速度
        private float rotationSpeed = 1.5f;
        public string[] selectLayerNames = { "character_enemy" };
        public string[] ignoreLayerNames = { "character_player", "character_NPC" };

        private Func<SkillTarget, List<CharacterLogic>> getLockList;
        private Action<CharacterLogic> setCurrentCharacterLogic;
        private Action<bool> setLockEffect;
        private Action cancelLockEffectAll;
        private Action<SkillTarget> setetLockListEffect;
        public void OnCreate()
        {
            getLockList = GetLockList;
            setCurrentCharacterLogic = SetCurrentCharacterLogic;
            setLockEffect = SetLockEffect;
            cancelLockEffectAll = CancelLockEffectAll;
            setetLockListEffect = SetetLockListEffect;
            RegisterEvent();

            isLockEffecft = false;
            //初始化锁定目标
            currentCharacterLogic = null;
            lockCharacter = null;
            CharacterLogic FirstEnemy = EventCenter.Instance.TriggerAction<string, CharacterLogic>("RoundCtl_GetOtherCharacter", "FirstEnemy");
            List<CharacterLogic> SortList = EventCenter.Instance.TriggerAction<List<CharacterLogic>>("RoundCtl_GetSortList");
            currentCharacterLogic = SortList.Find(x => x.team == Team.Player);

            //初始化锁定目标
            if (FirstEnemy != null)
            {
                 lockCharacter = FirstEnemy;
            }
        }
        #region 注册事件
        private void RegisterEvent()
        {
            EventCenter.Instance.AddListener("LockCtl_GetLockList", getLockList);
            EventCenter.Instance.AddListener("LockCtl_SetCurrentCharacterLogic", setCurrentCharacterLogic);
            EventCenter.Instance.AddListener("LockCtl_SetLockEffect", setLockEffect);
            EventCenter.Instance.AddListener("LockCtl_CancelLockEffectAll", cancelLockEffectAll);
            EventCenter.Instance.AddListener("LockCtl_SetetLockListEffect", setetLockListEffect);
        }
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("LockCtl_GetLockList");
            EventCenter.Instance.RemoveListener("LockCtl_SetCurrentCharacterLogic");
            EventCenter.Instance.RemoveListener("LockCtl_SetLockEffect");
            EventCenter.Instance.RemoveListener("LockCtl_CancelLockEffectAll");
            EventCenter.Instance.RemoveListener("LockCtl_SetetLockListEffect");
        }

        #endregion
        private void SetCurrentCharacterLogic(CharacterLogic characterLogic)
        {
            currentCharacterLogic = characterLogic;
        }
        //按下鼠标锁定敌人（激活锁定特效）
        private void SetLockCharacter()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (RaycastUtils.Instance.RaycastFromMouseSelect(out RaycastHit hitInfo, Mathf.Infinity, selectLayerNames))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("character_enemy"))
                    {
                        CharacterRender characterRender = hitInfo.collider.gameObject.GetComponent<CharacterRender>();
                        if (characterRender == null || characterRender.characterLogic == null)
                        {
                            Debug.LogWarning("LockCtl: 该锁定物体缺少CharacterRender或CharacterLogic组件");
                            return;
                        }

                        CharacterLogic clickedCharacter = characterRender.characterLogic;

                        if (lockCharacter != null && lockCharacter == clickedCharacter)
                        {
                            return;
                        }
                        if (isLockEffecft)
                        {
                            //取消当前锁定目标的高光特效
                            if (lockCharacter != null && lockCharacter.characterRender != null)
                            {
                                lockCharacter.characterRender.m_highlightEffect.highlighted = false;
                            }
                        }

                        //设置新的锁定目标
                        lockCharacter = clickedCharacter;

                        if (isLockEffecft)
                        {
                            //激活新的锁定目标的高光特效
                            if (lockCharacter != null && lockCharacter.characterRender != null)
                            {
                                lockCharacter.characterRender.m_highlightEffect.highlighted = true;
                            }
                        }
                    }
                }
            }
        }
        // 平滑旋转到目标（点击不同的敌人，玩家角色平滑的转向敌人）
        private void RotateToTarget(CharacterLogic characterLogicNow)
        {

            if (lockCharacter != null && lockCharacter.characterRender != null && characterLogicNow.team == Team.Player)
            {
                Transform parent = characterLogicNow.characterRender.transform;
                Transform target = lockCharacter.characterRender.transform;

                Vector3 targetDirection = target.position - parent.position;
                targetDirection.y = 0;
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    parent.rotation = Quaternion.Slerp(parent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        #region 获取锁定目标列表
        private List<CharacterLogic> GetLockList(SkillTarget skillTarget)
        {
            switch (skillTarget)
            {

                case SkillTarget.Self:
                    return GetPlayerSelf();

                case SkillTarget.PlayerSingle:
                    return GetPlayerSingle();

                case SkillTarget.playerAll:
                    return GetPlayerAll();

                case SkillTarget.playerExceptSelf:
                    return GetPlayerExceptSelf();

                case SkillTarget.EnemySelf:
                    return GetEnemySelf();

                case SkillTarget.EnemySingle:
                    return GetEnemySingle();

                case SkillTarget.EnemyAll:
                    return GetEnemyAll();

                case SkillTarget.EnemyExceptSelf:
                    return GetEnemyExceptSelf();

                case SkillTarget.All:
                    return GetAll();

                default:
                    return null;
            }
        }

        private List<CharacterLogic> GetPlayerSelf()
        {
            //获取当前进行锁定目标行为的自己
            if (currentCharacterLogic == null)
            {
                Debug.Log("LockCtl_GetSelf()当前角色为空");
                return null;
            }
            //返回当前目标
            return new List<CharacterLogic> { currentCharacterLogic };
        }
        private List<CharacterLogic> GetPlayerSingle()
        {
            if(currentCharacterLogic!=null)
            {
                return new List<CharacterLogic> { currentCharacterLogic };
            }

            return null;    
        }     
        private List<CharacterLogic> GetPlayerAll()
        {
            //获取所有玩家角色
            List<CharacterLogic> playerList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Player");
            return playerList;
        }
        private List<CharacterLogic> GetPlayerExceptSelf()
        {
            //获取所有玩家角色（不包括当前角色）
            List<CharacterLogic> playerExceptSelfList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Player");
            //返回指定目标
            return playerExceptSelfList;
        }

        private List<CharacterLogic> GetEnemySelf()
        {
            return null;
        }
        private List<CharacterLogic> GetEnemySingle()
        {
            //获取单体锁定目标
            if (lockCharacter == null)
            {
                Debug.Log("LockCtl_GetSingle()锁定目标为空");
                return null;
            }
            //返回当前目标
            return new List<CharacterLogic> { lockCharacter };
        }
        private List<CharacterLogic> GetEnemyAll()
        {
            //获取所有敌方角色
            List<CharacterLogic> enemyList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Enemy");

            return enemyList;
        }
        private List<CharacterLogic> GetEnemyExceptSelf()
        {
            return null;
        }

        private List<CharacterLogic> GetAll()
        {
            //获取所有角色
            List<CharacterLogic> allList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "all");

            return allList;
        }
        #endregion
        #region 激活锁定目标高光特效
        private void SetetLockListEffect(SkillTarget skillTarget)
        {
            switch (skillTarget)
            {
                case SkillTarget.Self:
                    SetSelfLockEffect();
                    break;

                case SkillTarget.playerAll:
                    SetPlayerAllLockEffect();
                    break;

                case SkillTarget.playerExceptSelf:
                    SetPlayerExceptSelfLockEffect();
                    break;

                case SkillTarget.EnemyAll:
                    SetEnemyAllLockEffect();
                    break;

                case SkillTarget.EnemySingle:
                    SetSingleLockEffect();
                    break;

                case SkillTarget.All:
                    SetAllLockEffect();
                    break;

                default:
                    break;
            }
        }

        private void SetSelfLockEffect()
        {
            //获取当前进行锁定目标行为的自己
            if (currentCharacterLogic == null)
            {
                Debug.Log("LockCtl_GetSelf()当前角色为空");
                return;
            }
            //取消所有高光特效
            CancelLockEffectAll();
            //激活目标特效
            if (currentCharacterLogic.characterRender != null && currentCharacterLogic.characterRender.m_highlightEffect != null)
            {
                if(currentCharacterLogic.aiState.GetState()!=AIStateEnum.death)
                {
                    currentCharacterLogic.characterRender.m_highlightEffect.highlighted = true;
                }
            }
            else
            {
                Debug.Log("LockCtl_GetSelf()目标缺少渲染体或者高光特效");
            }
        }

        private void SetSingleLockEffect()
        {
            //获取单体锁定目标
            if (lockCharacter == null)
            {
                Debug.Log("LockCtl_GetSingle()锁定目标为空");
                return;
            }
            //取消所有高光特效
            CancelLockEffectAll();
            //激活目标特效
            if (lockCharacter.characterRender != null && lockCharacter.characterRender.m_highlightEffect != null)
            {
                if(lockCharacter.aiState.GetState()!=AIStateEnum.death)
                {
                    lockCharacter.characterRender.m_highlightEffect.highlighted = true;
                    isLockEffecft = true;
                }
            
            }
            else
            {
                Debug.Log("LockCtl_GetSingle()目标缺少渲染体或者高光特效");
            }

        }

        private void SetPlayerExceptSelfLockEffect()
        {
            //获取所有玩家角色（不包括当前角色）
            List<CharacterLogic> playerExceptSelfList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Player");
            //取消所有高光特效
            CancelLockEffectAll();
            //激活目标特效
            playerExceptSelfList.Where(player => player != currentCharacterLogic).ToList();
            for (int i = 0; i < playerExceptSelfList.Count; i++)
            {
                if (playerExceptSelfList[i].characterRender != null && playerExceptSelfList[i].characterRender.m_highlightEffect != null)
                {
                    if(playerExceptSelfList[i].aiState.GetState()!=AIStateEnum.death)
                    {
                        playerExceptSelfList[i].characterRender.m_highlightEffect.highlighted = true;
                    }
                }
                else
                {
                    Debug.Log("LockCtl_GetPlayerExceptSelf()目标缺少渲染体或者高光特效");
                }
            }
        }

        private void SetPlayerAllLockEffect()
        {
            //获取所有玩家角色
            List<CharacterLogic> playerList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Player");
            //取消所有高光特效
            CancelLockEffectAll();
            //激活所有玩家角色身上的高光特效
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].characterRender != null && playerList[i].characterRender.m_highlightEffect != null)
                {
                    if(playerList[i].aiState.GetState()!=AIStateEnum.death)
                    {
                        playerList[i].characterRender.m_highlightEffect.highlighted = true;
                    }
                }
                else
                {
                    Debug.Log("LockCtl_GetPlayerAll()目标缺少渲染体或者高光特效");
                }
            }
        }

        private void SetEnemyAllLockEffect()
        {
            //获取所有敌方角色
            List<CharacterLogic> enemyList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Enemy");
            //取消所有高光特效
            CancelLockEffectAll();
            //激活所有敌方角色身上的高光特效
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i].characterRender != null && enemyList[i].characterRender.m_highlightEffect != null)
                {
                    if(enemyList[i].aiState.GetState()!=AIStateEnum.death)
                    {
                        enemyList[i].characterRender.m_highlightEffect.highlighted = true;
                    }
                }
                else
                {
                    Debug.Log("LockCtl_GetEnemyAll()目标缺少渲染体或者高光特效");
                }
            }
        }

        private void SetAllLockEffect()
        {
            //获取所有角色
            List<CharacterLogic> allList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "all");
            //取消所有高光特效
            CancelLockEffectAll();
            //激活所有角色身上的高光特效
            for (int i = 0; i < allList.Count; i++)
            {
                if (allList[i].characterRender != null && allList[i].characterRender.m_highlightEffect != null)
                {
                    if(allList[i].aiState.GetState()!=AIStateEnum.death)
                    {
                        allList[i].characterRender.m_highlightEffect.highlighted = true;
                    }
                }
                else
                {
                    Debug.Log("LockCtl_GetAll()目标缺少渲染体或者高光特效");
                }
            }
        }
        #endregion
        private void SetLockEffect(bool value)
        {
            isLockEffecft = value;
        }
        private void CancelLockEffectAll()
        {
            isLockEffecft = false;
            //取消所有高光特效
            List<CharacterLogic> AllList = EventCenter.Instance.TriggerAction<string, List<CharacterLogic>>("CharacterCtl_GetCharacterList", "All");
            for (int i = 0; i < AllList.Count; i++)
            {
                if (AllList[i].characterRender != null && AllList[i].characterRender.m_highlightEffect != null)
                {
                    AllList[i].characterRender.m_highlightEffect.highlighted = false;
                }
                else
                {
                    Debug.Log("LockCtl_GetPlayerExceptSelf()目标缺少渲染体或者高光特效");
                }
            }

        }
        // 每帧更新
        public void OnLogicFrameUpdate()
        {
            SetLockCharacter();
            RotateToTarget(currentCharacterLogic);
        }
        public void OnDestroy()
        {
            // 移除事件监听
            UnregisterEvent();

            // 清理角色引用
            if (currentCharacterLogic != null)
            {
                currentCharacterLogic = null;
            }

            if (lockCharacter != null)
            {
                lockCharacter = null;
            }

            // 清理层名称数组
            if (selectLayerNames != null)
            {
                selectLayerNames = null;
            }

            if (ignoreLayerNames != null)
            {
                ignoreLayerNames = null;
            }
        }

    }
}