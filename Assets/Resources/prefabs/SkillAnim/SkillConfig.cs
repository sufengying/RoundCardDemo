using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName ="SkillConfig",menuName ="SkillConfig",order =0)]
public class SkillConfig : ScriptableObject
{
    [HideInInspector]
    public bool hideMagnificatio=false;

    [HideInInspector]
    public bool hidepersistentTime=false;

    [LabelText("技能预制体(Timeline)")]
    public GameObject skillPrefab;
    [LabelText("伤害数字预制体")]
    public GameObject DamageNumberPrefab;
    //逻辑数据层
    [Header("逻辑数据层")]
    [LabelText("技能图标"),LabelWidth(0.1f),PreviewField(70,ObjectFieldAlignment.Left),SuffixLabel("技能图标")]
    public Sprite skillIcon;

    [LabelText("不按照正常技能流程执行")]
    public bool isInvoke=false;

    [LabelText("技能ID")]
    public int skillId;
    [LabelText("技能名称")]
    public string skillName;
    [LabelText("技能描述")]
    public string skillDescription;


    [LabelText("技能类型"),OnValueChanged("OnSkillTypeChanged")]   
    public SkillType skillType;

    [LabelText("技能目标")]
    public SkillTarget skillTarget;

    [LabelText("技能伤害类型"),OnValueChanged("OnDamageTypeChanged")]
    public SkillDamageType skillDamageType;
    
    [LabelText("技能消耗点数")]
    public int skillCost;

    [LabelText("每次攻击伤害倍率（整数，百分比）"),HideIf("hideMagnificatio")]
    [SerializeField]private int magnificatio;

    [LabelText("技能伤害倍率（小数）"),ReadOnly,HideIf("hideMagnificatio")]
    [SerializeField]public float Magnificatio;

    [LabelText("技能持续轮次"),HideIf("hidepersistentTime"),Min(0)]
    public int persistentTime;

    [LabelText("每个轮次持续次数"),OnValueChanged("OnTakenTimesChanged"),Min(0)]
    public int takenTimes;

    [LabelText("受到攻击的前摇(毫秒)")]
    public int[] attackPreDelay;

    // 当在Inspector中修改值时会自动调用
    private void OnValidate()
    {
       Magnificatio =magnificatio / 100f;
 
    }



    private void OnTakenTimesChanged()
    {
        if (attackPreDelay == null)
        {
            attackPreDelay = new int[takenTimes];
        }
        
        // 如果当前数量小于takenTimes，添加新的元素
        for(int i=0;i<takenTimes;i++)
        {
            attackPreDelay[i] = 0;
        }
        
        // 如果当前数量大于takenTimes，移除多余的元素
        for(int i=0;i<attackPreDelay.Length;i++)
        {
            attackPreDelay[i] = 0;
        }
    }
    private void OnDamageTypeChanged(SkillDamageType type)
    {
        switch(type)
        {
            case SkillDamageType.None:
                magnificatio=0;
                hideMagnificatio=true;
                break;
            default:
                hideMagnificatio=false;
                break;
        }  
    }
    private void OnSkillTypeChanged(SkillType type)
    {
        switch(type)
        {
            case SkillType.ActiveSkill:
                persistentTime=1;
                hidepersistentTime=true;
                break;
            case SkillType.PassiveSkill:
                hidepersistentTime=false;
                break;
        }

    }

    [HideInInspector]
    public bool hideTargetAllEffectInfos=true;
    //渲染层（存储特效位置，用timeline播放技能动画）
    [Header("渲染层设置")]
    //应该存储特效父物体的Transform ,以及每个特效的Source GameObject
    [LabelText("攻击特效生成位置"),OnValueChanged("OnEffectRenderedPosChanged")]
    public EffectRendered effectRenderedPos;

    [LabelText("群体指定目标身上特效"),HideIf("hideTargetAllEffectInfos")]
    public List<TargetAllEffectInfo> targetAllEffectInfos=new List<TargetAllEffectInfo>();

    [LabelText("特效父物体位置参数")]
    public List<SkillEffectInfo> skillEffectInfos=new List<SkillEffectInfo>();
    
    
    private void OnEffectRenderedPosChanged(EffectRendered effectRenderedPos)
    {
        switch(effectRenderedPos)
        {
            case EffectRendered.TargetAll:
                hideTargetAllEffectInfos=false;
                break;

            default:
                if(targetAllEffectInfos!=null&&targetAllEffectInfos.Count>0)
                {
                    for(int i=0;i<targetAllEffectInfos.Count;i++)
                    {
                        targetAllEffectInfos[i].effectPerfab=null;
                    }
                }
                targetAllEffectInfos.Clear();
                hideTargetAllEffectInfos=true;
                break;
        }
    }
}
/// <summary>
/// 技能类型
/// </summary>
public enum SkillType
{
    [LabelText("主动技能")]ActiveSkill,
    [LabelText("被动技能")]PassiveSkill

}
/// <summary>
/// 技能目标
/// </summary>
public enum SkillTarget 
{
    [LabelText("玩家角色单个")]PlayerSingle,//玩家单个
    [LabelText("玩家角色自身")]Self,//自己,如果SkillTarget 为Self，则技能目标为当前角色
    [LabelText("玩家角色全部")]playerAll,//友方全部（包括自己）,如果SkillTarget 为playerAll，则技能目标为友方全部角色
    [LabelText("玩家角色除进行行动的角色外全部")]playerExceptSelf,//友方除自己外全部,如果SkillTarget 为playerExceptSelf，则技能目标为友方除自己外的全部角色
    [LabelText("敌人角色单个")]EnemySingle,//敌人单个
    [LabelText("敌人角色自身")]EnemySelf,//敌人自身,如果SkillTarget 为EnemySelf，则技能目标为当前敌人
    [LabelText("敌人角色全部")]EnemyAll,//敌人全部
    [LabelText("敌人角色除进行行动的角色外全部")]EnemyExceptSelf,//敌人除进行行动的角色外全部
    [LabelText("所有角色")]All//所有角色
}

/// <summary>
/// 技能类型
/// </summary>
public enum SkillDamageType 
{
    [LabelText("无伤害")]None,
    [LabelText("物理伤害")]Physics,
    [LabelText("魔法伤害")]Spell,
    [LabelText("真实伤害")]real
   
}


[System.Serializable]
public class SkillEffectInfo
{
    [HideInInspector]
    public bool hideEffectTransform=false;

    [LabelText("特效位置设置"),OnValueChanged("ChangedEffectSetting")]
    public NeedValueOnEffectInfo Isneed;
      
    [LabelText("特效父物体ID"),HideIf("hideEffectTransform")]
    public int effectParentId;  // 对应 CharacterLogic 中的 effectParentId
        
    [LabelText("本地位置"),HideIf("hideEffectTransform")]
    public Vector3 localPosition;
        
    [LabelText("本地旋转"),HideIf("hideEffectTransform")]
    public Vector3 localRotation;
        
    [LabelText("本地缩放"),HideIf("hideEffectTransform")]
    public Vector3 localScale = Vector3.one;


    private void ChangedEffectSetting(NeedValueOnEffectInfo Isneed)
    {
        switch(Isneed)
        {
            case NeedValueOnEffectInfo.DontNeed:
                hideEffectTransform=false;
                break;
            case NeedValueOnEffectInfo.Need:
                effectParentId=-1;
                localPosition=Vector3.zero;
                localRotation=Vector3.zero;
                localRotation=Vector3.zero;
                hideEffectTransform=true;
                break;
        }
    }

}

[System.Serializable]
public class TargetAllEffectInfo
{
    [LabelText("特效预制体")]
    public GameObject effectPerfab;

    //偏移值
    [LabelText("本地偏移值")]
    public Vector3 localOffset;

    //旋转值
    [LabelText("本地旋转值")]
    public Vector3 localRotation;

    //缩放值
    [LabelText("本地缩放值")]
    public Vector3 localScale;


}

public enum EffectRendered
{
    [LabelText("不需要设置攻击特效生成位置")]None,
    [LabelText("地图中间")]mapMid,
    [LabelText("玩家阵营中间")]playerMid,
    [LabelText("敌人阵营中间")]enemyMid,

    [LabelText("施法者自身")] Self,

    [LabelText("单体指定目标身上")]TargetSingle,

    [LabelText("群体指定目标身上")]TargetAll
 
}

public enum NeedValueOnEffectInfo
{
    [LabelText("不需要传递源物体，直接设置模型自身的特效挂点位置即可")]
    DontNeed,
    [LabelText("需要传递源目标物体，来设置特效生成位置")]
    Need

}