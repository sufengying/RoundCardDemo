using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class CalculateActionValue
    {
        public int ActionValue { get; private set; }    // 计算得出的行动值 
        public int nextRoundPath { get; private set; } //该角色距离下一回合还有多少距离，行动值为 nextRoundPath/speed,
                                                       // 在战斗开始时，所有角色的下一回合路程都是为10000，然后计算出行动值


        public void InitializeActionValue(int speed)
        {
            nextRoundPath = GlobalConfig.roundData;
            ActionValue = nextRoundPath / speed;
        }

        //以当前的速度计算行动值（剩余路程不为0）
        public void UpdateActionValue(int speed)
        {
            ActionValue = Mathf.RoundToInt(nextRoundPath / Mathf.Max(speed, 1));
        }

        //当行动结束后重新计算行动值（剩余路程为0）
        public void UpdateNextRoundActionValue(int speed)
        {
            nextRoundPath = GlobalConfig.roundData;  // 重置剩余路程
            ActionValue = nextRoundPath / speed;
        }

        public void UpdateNextRoundPath(int value)
        {
            nextRoundPath = value;
        }

        public void OnDestroy()
        {
            // 重置所有值到初始状态
            ActionValue = 0;
            nextRoundPath = 0;
        }
    }
}
