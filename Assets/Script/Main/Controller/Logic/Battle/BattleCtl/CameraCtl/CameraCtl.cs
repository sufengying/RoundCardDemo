using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    // 假设 CameraIdentifier.cs 脚本已存在，并已附加到场景/预制体中相关的虚拟相机上
    // public class CameraIdentifier : MonoBehaviour { public int playerPosID = -1; }

    public class CameraCtl:ILogicBehaviour
    {
        // Cinemachine 优先级常量
        private const int ACTIVE_PRIORITY = 100; // 激活状态优先级
        private const int INACTIVE_PRIORITY = 10; // 非激活状态优先级

        // 存储玩家专属虚拟相机，映射 playerPosID 到对应的 VCam
        private Dictionary<int, CinemachineVirtualCamera> playerVCamMap;
        // 追踪由此控制器管理的当前活动虚拟相机
        private CinemachineVirtualCamera currentActiveVCam;

        // 当前聚焦的玩家角色
        public CharacterLogic currentFocusedPlayer;
        // 所有敌人的引用列表
        private List<CharacterLogic> enemyList;
        // 旋转速度
        private const float ROTATION_SPEED = 1.5f;

        private Action<CharacterLogic> onActorChanged;
        private Func<CharacterLogic> getCurrentFocusedPlayer;

        public void OnCreate()
        {
            onActorChanged=OnActorChanged;
            getCurrentFocusedPlayer=GetCurrentFocusedPlayer;
            RegisterEvent();

            playerVCamMap = new Dictionary<int, CinemachineVirtualCamera>();
            enemyList=EventCenter.Instance.TriggerAction<string,List<CharacterLogic>>("CharacterCtl_GetCharacterList", "Enemy");
            List<CinemachineVirtualCamera> cinemachineVirtualCameraList=EventCenter.Instance.TriggerAction<List<CinemachineVirtualCamera>>("WorldDataCtl_GetCinemachineCameraList");
            foreach (CinemachineVirtualCamera vcam in cinemachineVirtualCameraList)
            {
                // 尝试获取 CameraIdentifier 组件
                CameraIdentifier identifier = vcam.GetComponent<CameraIdentifier>();
                if (identifier != null)
                {
                    // 如果找到标识符，将其添加到映射中
                    playerVCamMap[identifier.playerPosID] = vcam;
                    // 设置初始优先级为低
                    vcam.Priority = INACTIVE_PRIORITY;
                }
            }
            Initialize();
        }
  
#region 注册事件
        private void RegisterEvent()
        {
            // 订阅镜头变更事件,RoundCtl会触发这个事件,触发后会移动镜头到下一个角色（如果下一个角色是敌人则镜头不变）
            EventCenter.Instance.AddListener("CameraCtl_OnActorChanged", onActorChanged);
            EventCenter.Instance.AddListener("CameraCtl_GetCurrentFocusedPlayer", getCurrentFocusedPlayer);

        }
        private void UnregisterEvent()
        {
            EventCenter.Instance.RemoveListener("CameraCtl_OnActorChanged");
            EventCenter.Instance.RemoveListener("CameraCtl_GetCurrentFocusedPlayer");
        }
#endregion


        /// <summary>
        /// 初始化相机控制器，设置初始聚焦的玩家和下一个玩家。
        /// </summary>
        /// <param name="characterLogicNow">当前行动的角色。</param>
        /// <param name="characterLogicNextPlayer">下一个行动的玩家。</param>
        public void Initialize()
        {
            // 寻找并保存第一个行动的玩家
            List<CharacterLogic> SortList=EventCenter.Instance.TriggerAction< List<CharacterLogic> >("RoundCtl_GetSortList");
            currentFocusedPlayer = SortList.Find(x=>x.team==Team.Player);
            // 获取所有敌人列表
            enemyList = EventCenter.Instance.TriggerAction<string,List<CharacterLogic>>("CharacterCtl_GetCharacterList","Enemy");

            // 初始化时就让敌人看向当前玩家
            UpdateEnemyRotations();

            // 如果当前玩家有对应的相机，激活它
            if (currentFocusedPlayer != null && playerVCamMap.ContainsKey(currentFocusedPlayer.posID))
            {
                SwitchTo(playerVCamMap[currentFocusedPlayer.posID], currentFocusedPlayer);
            }
            else
            {
                // 即使没有对应的相机，也要触发角色头像UI选中事件         
                BattleEventCenter.Instance.TriggerCharacterSelected(currentFocusedPlayer);
                BattleEventCenter.Instance.TriggerCharacterSkillChanged(currentFocusedPlayer);
            }

        }

        /// <summary>
        /// 变更玩家镜头事件
        /// </summary>
        /// <param name="character"></param>
        private void OnActorChanged(CharacterLogic character)
        {
            // 只有当当前行动的是玩家时才更新聚焦的玩家
            if (character.team == Team.Player)
            {
                // 如果当前玩家有对应的相机，激活它
                if (character != null && playerVCamMap.ContainsKey(character.posID))
                {
                    SwitchTo(playerVCamMap[character.posID], character);
                }
            }
        }
        /// <summary>
        /// 获取当前聚焦的玩家角色
        /// </summary>
        public CharacterLogic GetCurrentFocusedPlayer()
        {
            return currentFocusedPlayer;
        }

        /// <summary>
        /// 通过调整优先级来切换活动的 Cinemachine 相机。
        /// </summary>
        /// <param name="target">要激活的虚拟相机。</param>
        /// <param name="newFocusedPlayer">新的聚焦玩家。</param>
        public void SwitchTo(CinemachineVirtualCamera target, CharacterLogic newFocusedPlayer)
        {
            // 1. 验证目标相机
            if (target == null)
            {
                Debug.LogWarning("调用 SwitchTo 时传入了 null 的目标相机。");
                return;
            }

            // 2. 如果目标已是当前活动相机，避免不必要的切换
            if (target == currentActiveVCam)
            {
                return;
            }

            // 3. 停用之前的相机（如果存在活动相机）
            if (currentActiveVCam != null)
            {
                currentActiveVCam.Priority = INACTIVE_PRIORITY;
            }

            // 4. 激活新的目标相机
            target.Priority = ACTIVE_PRIORITY;

            // 5. 更新对当前活动相机的引用
            currentActiveVCam = target;

            // 6. 更新当前聚焦的玩家并让敌人看向他
            currentFocusedPlayer = newFocusedPlayer;
            UpdateEnemyRotations();


            // 7. 触发事件，通知角色头像UI更新选中状态(BattleCtl)   
            EventCenter.Instance.TriggerAction("BattleUICtl_SetSelectedChacaterHead",currentFocusedPlayer);
            // 8. 触发事件，通知技能UI更新技能(BattleCtl)
            //EventCenter.Instance.TriggerAction("BattleUICtl_UpdateSkillButtonUI",currentFocusedPlayer);
            // 9. 触发事件，通知更新进锁定行为的友方角色(LockCtl)
            EventCenter.Instance.TriggerAction("LockCtl_SetCurrentCharacterLogic",currentFocusedPlayer);
        }

        // 更新所有敌人的朝向
        private void UpdateEnemyRotations()
        {
            if (currentFocusedPlayer == null || currentFocusedPlayer.characterRender == null) return;

            foreach (var enemy in enemyList)
            {
                if (enemy?.characterRender == null) continue;

                // 计算朝向目标的方向
                Vector3 targetDirection = currentFocusedPlayer.characterRender.transform.position -
                                       enemy.characterRender.transform.position;
                targetDirection.y = 0; // 保持Y轴不变

                if (targetDirection != Vector3.zero)
                {
                    // 计算目标旋转
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    // 平滑旋转到目标方向
                    enemy.characterRender.transform.rotation = Quaternion.Slerp(
                        enemy.characterRender.transform.rotation,
                        targetRotation,
                        ROTATION_SPEED * Time.deltaTime
                    );
                }
            }
        }

        public void OnLogicFrameUpdate()
        {
            // 每帧更新敌人的朝向
            UpdateEnemyRotations();
        }

        /// <summary>
        /// 在控制器销毁时清理资源。
        /// </summary>
        public void OnDestroy()
        {
            // 清理相机映射
            if (playerVCamMap != null)
            {
                foreach (var vcam in playerVCamMap.Values)
                {
                    if (vcam != null)
                    {
                        vcam.Priority = INACTIVE_PRIORITY;
                    }
                }
                playerVCamMap.Clear();
                playerVCamMap = null;
            }

            // 清理当前活动相机
            if (currentActiveVCam != null)
            {
                currentActiveVCam.Priority = INACTIVE_PRIORITY;
                currentActiveVCam = null;
            }

            // 清理角色引用
            if (currentFocusedPlayer != null)
            {
                currentFocusedPlayer = null;
            }

            // 清理敌人列表
            if (enemyList != null)
            {
                for (int i = 0; i < enemyList.Count; i++)
                {
                    enemyList[i] = null;
                }
                enemyList.Clear();
                enemyList = null;
            }

            // 移除所有事件监听
            UnregisterEvent();

            // 清理委托
            getCurrentFocusedPlayer = null;
        }


    }
}
