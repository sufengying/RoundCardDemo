using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CutSceneWindow : SingletionMono<CutSceneWindow>
{
    public CanvasGroup canvasGroup;
    public Image roundImage;
    public Slider sliderSlider;
    // Start is called before the first frame update

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    //通过加载时间播放动画
    public void PlayAnimation(Action action=null)
    {
        // 重置slider值
        sliderSlider.value = 0;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 添加slider动画
        sequence.Join(sliderSlider.DOValue(1f, 3f).SetEase(Ease.Linear));
        
        // 添加旋转动画（跟随slider动画，转3圈）
        sequence.Join(roundImage.transform.DORotate(new Vector3(0, 0, 1080f), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(1));  // 只循环一次，与slider动画同步
            
        // 设置动画不受时间缩放影响
        sequence.SetUpdate(true);
        sequence.OnComplete(() => {
            // 动画结束后执行
      
            HideCutSceneWindow(action);
        });
    }

    public void ShowCutSceneWindow()
    {
      
        canvasGroup.DOFade(1, 0.2f).SetEase(Ease.Linear);
    }

    public void ResetSliderValue()
    {
        sliderSlider.value = 0;
    }

    public void HideCutSceneWindow(Action actionEnd=null)
    {
    
        DarkMaskWindow.Instance.ShowAndHideDarkMaskWindow(null,
                                        actionMiddle:()=>{
                                            canvasGroup.DOFade(0, 0.2f).SetEase(Ease.Linear);
                                        },
                                        actionEnd:actionEnd);
    }



}
