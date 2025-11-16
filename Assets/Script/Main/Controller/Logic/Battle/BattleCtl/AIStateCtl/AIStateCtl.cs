using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMGCFrameWork.Battle
{
    public class AIStateCtl : Singletion<AIStateCtl>
    {
        AIStateBase[] states;

        public void Initialize()
        {

            states = new AIStateBase[Enum.GetValues(typeof(AIStateEnum)).Length];
            states[(int)AIStateEnum.waiting] = new WaitingState(AIStateEnum.waiting);
            states[(int)AIStateEnum.going] = new GoingState(AIStateEnum.going);
            states[(int)AIStateEnum.block] = new BlockState(AIStateEnum.block);
            states[(int)AIStateEnum.death] = new DeathState(AIStateEnum.death);
            states[(int)AIStateEnum.hit] = new HitState(AIStateEnum.hit);

        }

        public void ChangeState(AIStateEnum newState, CharacterLogic characterLogic)
        {
            characterLogic.aiState.OnExitState();
            

            characterLogic.aiState = states[(int)newState];

            
            characterLogic.aiState.OnEnterState();
        }




    }
}
