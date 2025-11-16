using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class SkillInfo : MonoBehaviour
{
    public int skillId;
    public PlayableDirector skillPlayableDirector;
    public List<PooledControlPlayableAsset> pooledControlPlayableAssets;
    
    //设置动画状态机给playableDirector
    //获得PlayableDirector的所有动画轨道
    //设置动画轨道的动画状态机

    //获得timelineAsset的所有动画轨道
    //给轨道设置动画机
    //获得timelineAsset的所有PooledControlTrack轨道上的所有clip
    public void SetAnimatorInAnimationTrack(Animator m_animator)
    {
        TimelineAsset timelineAsset = skillPlayableDirector.playableAsset as TimelineAsset;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is AnimationTrack )
            {
                skillPlayableDirector.SetGenericBinding(track, m_animator);            
            }
        }     
    }

    //播放技能动画
    public void PlaySkill(Animator m_animator,bool isInvoke)
    {
        skillPlayableDirector.Play();
        EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setCurrentCanvasGroup",false);
        StartCoroutine(CheckSkillTimeLineComplete(m_animator,isInvoke));
    }

    IEnumerator CheckSkillTimeLineComplete(Animator m_animator,bool isInvoke)
    {
        while(skillPlayableDirector.state==PlayState.Playing)
        {
            yield return null;
        }
        m_animator.Rebind();
        if(!isInvoke)
        {
            EventCenter.Instance.TriggerAction("ActionCtl_TriggerEvent");
        }

    }

    public void SetPlayableAssetsFromTrackTypes<T>() where T : PlayableAsset
    {
        TimelineAsset timelineAsset = skillPlayableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null) return ;

        pooledControlPlayableAssets = new List<PooledControlPlayableAsset>();

        // 查找所有指定类型的轨道
        var tracks = timelineAsset.GetOutputTracks();
        
        // 获取每个轨道上的所有Clip的PlayableAsset
        foreach (var track in tracks)
        {
            var clips = track.GetClips();
            foreach (var clip in clips)
            {
                if (clip.asset is T playableAsset)
                {
                    if (playableAsset is PooledControlPlayableAsset pooledAsset)
                    {
                        pooledControlPlayableAssets.Add(pooledAsset);
                    }
                }
            }
        }
    }

    public void SetSourceGameObject(int index,GameObject sourceGameObject)
    {

        skillPlayableDirector.SetReferenceValue(
                pooledControlPlayableAssets[index].sourceGameObject.exposedName,
                sourceGameObject
            );           
    }

    public void OnDestroy()
    {

    }
}

