using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    /// <summary>
    /// 战斗场景控制器
    /// </summary>
    public class BattleWorld : World
    {

        private LockCtl lockCtl;
        private CameraCtl cameraCtl;


        public override void OnCreate()
        {

            cameraCtl = GetExitsLogicCtrl<CameraCtl>();
            lockCtl = GetExitsLogicCtrl<LockCtl>();
        }
        public override void OnUpdate()
        {
            // if (Input.GetKeyDown(KeyCode.U))
            // {
            //     // 行动结束，更新当前行动角色
            //    // RoundCtl.Action(new List<CharacterLogic>() { RoundCtl.CurrentCharacter.Player });
            //    EventCenter.Instance.TriggerAction("RoundCtl_OnAction");
            // }
            // 自设的逻辑帧更新
            FrameUpdate();
        }


        private void FrameUpdate()
        {
            //敌人转向（看向玩家）
            //CameraCtl.OnLogicFrameUpdate();
            //玩家转向（看向敌人）
            // LockCtl.OnLogicFrameUpdate(RoundCtl.CurrentCharacter);
            if (cameraCtl != null)
                cameraCtl.OnLogicFrameUpdate();
            if (lockCtl != null)
                lockCtl.OnLogicFrameUpdate();
        }

        public override void OnDestroy()
        {
            try
            {
                // 清理控制器引用
                if (lockCtl != null)
                {
                    lockCtl.OnDestroy();
                    lockCtl = null;
                }

                if (cameraCtl != null)
                {
                    cameraCtl.OnDestroy();
                    cameraCtl = null;
                }

                // 调用基类的 OnDestroy
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError($"BattleWorld OnDestroy Error: {e.Message}\n{e.StackTrace}");
            }
        }

    }
}
