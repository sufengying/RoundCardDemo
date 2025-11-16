using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMGCFrameWork.Battle;
public class BattleWorldScriptExecutionOrder  :IBehaviourExecution
{
    private static Type[] LogicBehaviorExecutions = new Type[] {
       typeof(CharacterCtl),
       typeof(RoundCtl),
       typeof(CameraCtl),
       typeof(BattleUICtl),
       typeof(LockCtl),
       typeof(SkillCtl),
       typeof(ActionCtl),
       
     };

    private static Type[] DataBehaviorExecutions = new Type[] {
        typeof(WorldDataCtl),
     };

    private static Type[] MsgBehaviorExecutions = new Type[] {
       
     };

    public Type[] GetDataBehaviourExecution()
    {
        return DataBehaviorExecutions;
    }

    public Type[] GetLogicBehaviourExecution()
    {
        return LogicBehaviorExecutions;
    }

    public Type[] GetMsgBehaviourExecution()
    {
        return MsgBehaviorExecutions;
    }
}
