using System;
using System.Collections.Generic;
using UnityEngine;
using ZMGCFrameWork.Battle;

/// <summary>
/// 战斗事件中心，管理所有战斗相关的事件
/// </summary>
public class BattleEventCenter : Singletion<BattleEventCenter>, IMsgBehaviour
{
    // 角色相关事件
    public event Action<CharacterLogic> OnCharacterCreated;
    public event Action<CharacterLogic> OnCharacterDestroyed;
    public event Action<CharacterLogic> OnCharacterSelected;
    public event Action<CharacterLogic, int> OnCharacterHealthChanged;
    public event Action<CharacterLogic, int> OnCharacterActionValueChanged;

    // 回合相关事件
    public event Action<CalculateActionValue> OnRoundStarted;
    public event Action<CalculateActionValue> OnRoundEnded;
    public event Action<CalculateActionValue> OnActorChanged;

    // 相机相关事件
    public event Action<CharacterLogic> OnCameraTargetChanged;
    public event Action<Vector3> OnCameraPositionChanged;

    // UI相关事件
    public event Action<CalculateActionValue, List<CalculateActionValue>> OnSortListUpdated;
    public event Action<CharacterLogic> OnSelectedCharacterChanged;
    public event Action<CharacterLogic> OnCharacterSkillChanged;
    public event Action<SkillConfig> OnSkillTrigger;

    // 锁定相关事件
    public event Action<CharacterLogic> OnSetCurrentCharacterLogic;
    public event Func<CharacterLogic> OnGetTargetLocked;
    public event Func<CharacterLogic> OnGetCurrentCharacterLogic;
    public event Action<List<CharacterLogic>> OnSetPlayerAll;
    public event Action<List<CharacterLogic>> OnSetEnemyAll;
    public event Func<List<CharacterLogic>> OnGetPlayerAll;
    public event Func<List<CharacterLogic>> OnGetEnemyAll;
    public event Func<List<CharacterLogic>> OnGetCharacterAll;
    public event Func<List<CharacterLogic>> OnGetPlayerExceptSelf;

    //技能控制器相关事件
    public event Action<CharacterLogic, SkillConfig> OnSkillCtl;

    // 技能特效父物体相关事件
    public event Func<EffectRendered, GameObject> OnGetSkillParent;

    public void OnCreate()
    {
        throw new NotImplementedException();
    }
    // 触发事件的方法
    public void TriggerCharacterCreated(CharacterLogic character)
    {
        OnCharacterCreated?.Invoke(character);
    }

    public void TriggerCharacterDestroyed(CharacterLogic character)
    {
        OnCharacterDestroyed?.Invoke(character);
    }

    public void TriggerCharacterSelected(CharacterLogic character)
    {
        OnCharacterSelected?.Invoke(character);
    }

    public void TriggerCharacterHealthChanged(CharacterLogic character, int newHealth)
    {
        OnCharacterHealthChanged?.Invoke(character, newHealth);
    }

    public void TriggerCharacterActionValueChanged(CharacterLogic character, int newValue)
    {
        OnCharacterActionValueChanged?.Invoke(character, newValue);
    }

    public void TriggerRoundStarted(CalculateActionValue actor)
    {
        OnRoundStarted?.Invoke(actor);
    }

    public void TriggerRoundEnded(CalculateActionValue actor)
    {
        OnRoundEnded?.Invoke(actor);
    }

    public void TriggerActorChanged(CalculateActionValue actor)
    {
        OnActorChanged?.Invoke(actor);
    }

    public void TriggerCameraTargetChanged(CharacterLogic target)
    {
        OnCameraTargetChanged?.Invoke(target);
    }

    public void TriggerCameraPositionChanged(Vector3 position)
    {
        OnCameraPositionChanged?.Invoke(position);
    }

    public void TriggerSortListUpdated(CalculateActionValue characterLogicLast, List<CalculateActionValue> sortList)
    {
        OnSortListUpdated?.Invoke(characterLogicLast, sortList);
    }

    public void TriggerSelectedCharacterChanged(CharacterLogic character)
    {
        OnSelectedCharacterChanged?.Invoke(character);
    }

    #region 锁定相关事件
    public void Trigger_SetCurrentCharacterLogic(CharacterLogic character)
    {
        OnSetCurrentCharacterLogic?.Invoke(character);
    }

    public CharacterLogic Trigger_GetTargetLocked()
    {
        return OnGetTargetLocked?.Invoke();
    }

    public CharacterLogic Trigger_GetCurrentCharacterLogic()
    {
        return OnGetCurrentCharacterLogic?.Invoke();
    }

    public void Trigger_SetPlayerAll(List<CharacterLogic> playerAll)
    {
        OnSetPlayerAll?.Invoke(playerAll);
    }

    public void Trigger_SetEnemyAll(List<CharacterLogic> enemyAll)
    {
        OnSetEnemyAll?.Invoke(enemyAll);
    }

    public List<CharacterLogic> Trigger_GetPlayerAll()
    {
        return OnGetPlayerAll?.Invoke();
    }

    public List<CharacterLogic> Trigger_GetEnemyAll()
    {
        return OnGetEnemyAll?.Invoke();
    }

    public List<CharacterLogic> Trigger_GetCharacterAll()
    {
        return OnGetCharacterAll?.Invoke();
    }

    public List<CharacterLogic> Trigger_GetPlayerExceptSelf()
    {
        return OnGetPlayerExceptSelf?.Invoke();
    }
    #endregion

    public void TriggerCharacterSkillChanged(CharacterLogic character)
    {
        OnCharacterSkillChanged?.Invoke(character);
    }

    public void TriggerSkillTrigger(SkillConfig skill)
    {
        OnSkillTrigger?.Invoke(skill);
    }

    public void TriggerSkillCtl(CharacterLogic character, SkillConfig skill)
    {
        OnSkillCtl?.Invoke(character, skill);
    }

    public GameObject TriggerGetSkillParent(EffectRendered skillTarget)
    {
        return OnGetSkillParent?.Invoke(skillTarget);
    }



    public void OnDestroy()
    {

    }

}
