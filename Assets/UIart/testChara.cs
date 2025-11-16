using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testChara : MonoBehaviour
{
    public Animator animator;
    private bool isPlayingAviod = false;  // 添加标志来跟踪动画状态

    void Start()
    {
        animator = GetComponent<Animator>();
        // // 添加动画事件监听
        // AnimationEvent animEvent = new AnimationEvent();
        // animEvent.functionName = "OnAviodAnimationComplete";
        // animEvent.time = animator.GetCurrentAnimatorStateInfo(0).length;
        
        // // 获取aviod动画片段
        // AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        // foreach (AnimationClip clip in clips)
        // {
        //     if (clip.name.ToLower().Contains("aviod"))
        //     {
        //         clip.AddEvent(animEvent);
        //         break;
        //     }
        // }
    }

    public void Playaviod()
    {
        // 只有在没有播放aviod动画时才触发
        if (!isPlayingAviod)
        {
            isPlayingAviod = true;
            animator.SetTrigger("aviod");
        }
    }

    // 动画事件回调方法
    public void OnAviodAnimationComplete()
    {
        Debug.Log("OnAviodAnimationComplete");
        isPlayingAviod = false;  // 动画播放完成，重置标志
        animator.SetTrigger("Idle");
    }

    // 添加新的动画事件回调方法
    public void OnCompleteAviod()
    {
        Debug.Log("OnCompleteAviod");
        isPlayingAviod = false;
        animator.SetTrigger("Idle");
    }

    public void Ontest()
    {
        Debug.Log("test");
    }
}
