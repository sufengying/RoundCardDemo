using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Skills : MonoBehaviour
{
    private Dictionary<int, SkillInfo> skillInfoDic = new Dictionary<int, SkillInfo>();
    [SerializeField] private Animator m_animator;

    [Header("技能特效挂点位置(不需要传递参数)")]
    [SerializeField]
    private List<EffectParentPos> effectParentPos = new List<EffectParentPos>();

    //资源加载技能子物体
    //根据技能id加载技能子物体
    public void LoadSkillInfo(List<SkillConfig> skillConfigList, Animator animator)
    {
        m_animator = animator;
        foreach (var skillConfig in skillConfigList)
        {
            GameObject skillObj = ResourcesMgr.Instance.InstantiateGameObject(skillConfig.skillPrefab, this.transform, true, true, true);
            skillObj.name = skillConfig.skillName;
        }
    }

    //初始化技能组件
    //获得所有子物体技能组件
    public void InitializeSkillInfo()
    {
        SkillInfo[] skillInfos = this.GetComponentsInChildren<SkillInfo>();
        foreach (var skillInfo in skillInfos)
        {
            skillInfo.SetAnimatorInAnimationTrack(m_animator);
            skillInfo.SetPlayableAssetsFromTrackTypes<PooledControlPlayableAsset>();
            skillInfoDic.Add(skillInfo.skillId, skillInfo);
        }
    }

    //根据技能ID释放技能,配置技能父物体transform信息
    //配置好位置信息后，把配置好的父物体设置 成对应特效的源物体
    public void PlaySkill(SkillConfig skillConfig, GameObject skillParent = null)
    {
        //通过技能配置表的ID属性来找出应该要释放的技能的SkillInfo组件
        if (skillInfoDic.TryGetValue(skillConfig.skillId, out SkillInfo skill))
        {
            //1.设置该技能的特效父物体的transform属性
            //遍历技能配置表的skillEffectInfos列表，根据列表的特效生成位置信息，来设置特效的父物体的transform属性
            for (int i = 0; i < skillConfig.skillEffectInfos.Count; i++)
            {
                //如果特效位置设置信息为Need，意思时需要传递一个参数信息来设置特效的父物体，则不设置特效位置
                if (skillConfig.skillEffectInfos[i].Isneed == NeedValueOnEffectInfo.Need)
                {
                    continue;
                }
                //如果特效位置设置信息为DontNeed，意思时不需要传递一个参数信息来设置特效的父物体，则直接设置特效位置
                else if (skillConfig.skillEffectInfos[i].Isneed == NeedValueOnEffectInfo.DontNeed)
                {
                    //根据技能配置表的skillEffectInfos列表的effectParentId属性，
                    //遍历effectParentPos列表，找到对应的特效父物体，并设置特效位置
                    for (int j = 0; j < effectParentPos.Count; j++)
                    {
                        if (skillConfig.skillEffectInfos[i].effectParentId == effectParentPos[j].GetEffectParentId())
                        {
                            effectParentPos[j].ChangedTransform(skillConfig.skillEffectInfos[i]);
                        }
                    }
                }
            }
            //2.为timneline资源里的特效Clip设置源物体（source GameObject）
            if (skill.pooledControlPlayableAssets.Count <= 0 || skill.pooledControlPlayableAssets == null)
            {
                Debug.LogError("技能" + skillConfig.skillId + "没有特效Clip");
            }
            //遍历该技能的特效Clip
            for (int i = 0; i < skill.pooledControlPlayableAssets.Count; i++)
            {
                //如果特效Clip需要传递参数，则需要传递参数
                if (skill.pooledControlPlayableAssets[i].needValueOnEffectInfo == NeedValueOnEffectInfo.Need)
                {

                    if (skillParent != null)
                    {
                     
                        skill.SetSourceGameObject(i, skillParent);
                        continue;
                    }
                }
                //如果特效Clip不需要传递参数，则不需要传递参数
                if (skill.pooledControlPlayableAssets[i].needValueOnEffectInfo == NeedValueOnEffectInfo.DontNeed)
                {

                   
                    for (int j = 0; j < effectParentPos.Count; j++)
                    {
                        if (effectParentPos[j].GetEffectParentId() == skill.pooledControlPlayableAssets[i].effectParentId)
                        {
                            skill.SetSourceGameObject(i, effectParentPos[j].gameObject);
                        }
                    }

                }
            }
            skill.PlaySkill(m_animator,skillConfig.isInvoke);
        }
        else
        {
            Debug.LogError("根据技能ID查找技能信息失败，技能ID：" + skillConfig.skillId);
        }
    }
    /// <summary>
    /// 根据技能特效位置获取技能特效位置
    /// </summary>
    /// <param name="skillEffectInfo">技能特效位置</param>
    /// <returns>技能特效位置</returns>
    public EffectParentPos GetEffectParentPos(SkillEffectInfo skillEffectInfo)
    {
        for (int i = 0; i < effectParentPos.Count; i++)
        {
            if (effectParentPos[i].GetEffectParentId() == skillEffectInfo.effectParentId)
            {
                return effectParentPos[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 设置技能特效位置
    /// </summary>
    /// <param name="skillEffectInfo">技能特效位置</param>
    public void SetEffectParentPos(SkillEffectInfo skillEffectInfo)
    {
        for (int i = 0; i < effectParentPos.Count; i++)
        {
            if (effectParentPos[i].GetEffectParentId() == skillEffectInfo.effectParentId)
            {
                effectParentPos[i].ChangedTransform(skillEffectInfo);
            }
        }
    }

    public void StopSkill()
    {
        foreach (var skillInfo in skillInfoDic)
        {
            skillInfo.Value.skillPlayableDirector.Stop();
            m_animator.enabled = true;
        }
    }
    public void OnDestroy()
    {


    }
}
