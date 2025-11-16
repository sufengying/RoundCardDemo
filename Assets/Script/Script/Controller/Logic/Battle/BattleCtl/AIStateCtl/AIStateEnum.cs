using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public enum AIStateEnum
    {
        //正在行动
        going,
        //等待  
        waiting,
        //攻击
        hit,
        //格挡
        block,
        //闪避
        dodge,
        //死亡
        death

    }
}

