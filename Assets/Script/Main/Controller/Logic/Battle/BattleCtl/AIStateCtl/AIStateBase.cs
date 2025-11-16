using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public abstract class AIStateBase
    {
        protected AIStateEnum state;

        public AIStateBase(AIStateEnum state)
        {
            this.state = state;
        }
        public AIStateEnum GetState()
        {
            return state;
        }

        public abstract void OnEnterState();

        public abstract void OnExitState();

    }
}


