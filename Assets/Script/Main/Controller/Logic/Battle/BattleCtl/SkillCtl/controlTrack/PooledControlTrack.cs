using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(PooledControlPlayableAsset))]
public class PooledControlTrack : TrackAsset
{
#if UNITY_EDITOR
    private static readonly HashSet<PlayableDirector> s_ProcessedDirectors = new HashSet<PlayableDirector>();

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        if (director == null)
            return;

        // 避免递归
        if (s_ProcessedDirectors.Contains(director))
            return;

        s_ProcessedDirectors.Add(director);

        try
        {
            // 收集需要预览的粒子系统
            var particlesToPreview = new HashSet<ParticleSystem>();
            var activationToPreview = new HashSet<GameObject>();

            foreach (var clip in GetClips())
            {
                var controlPlayableAsset = clip.asset as PooledControlPlayableAsset;
                if (controlPlayableAsset == null)
                    continue;

                var gameObject = controlPlayableAsset.sourceGameObject.Resolve(director);
                if (gameObject == null && controlPlayableAsset.prefabGameObject != null)
                {
                    gameObject = controlPlayableAsset.prefabGameObject;
                }

                if (gameObject != null)
                {
                    // 收集粒子系统
                    if (controlPlayableAsset.updateParticle)
                    {
                        var particles = gameObject.GetComponentsInChildren<ParticleSystem>(true);
                        foreach (var ps in particles)
                        {
                            if (ps != null)
                            {
                                particlesToPreview.Add(ps);
                                
                            }
                        }
                    }

                    // 收集激活状态
                    if (controlPlayableAsset.active)
                    {
                        activationToPreview.Add(gameObject);
                    }
                }
            }

        }
        finally
        {
            s_ProcessedDirectors.Remove(director);
        }
    }
#endif
}

