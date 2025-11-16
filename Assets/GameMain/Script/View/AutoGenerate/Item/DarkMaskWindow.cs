using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DarkMaskWindow : SingletionMono<DarkMaskWindow>
{
    CanvasGroup canvasGroup;

    public void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public  void ShowDarkMaskWindow(Action onComplete = null)
    {
        // 执行显示逻辑
        // ...
        
        canvasGroup.DOFade(1, 0.5f).SetEase(Ease.Linear).OnComplete(()=>{
            onComplete?.Invoke();
        });
        // 显示完成后调用回调
        
    }

    public  void HideDarkMaskWindow(Action onComplete = null)
    {
        
       canvasGroup.DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(()=>{
        onComplete?.Invoke();
       });
    }

    public void ShowAndHideDarkMaskWindow(Action actionStart=null,Action actionMiddle=null,Action AfterMiddle=null ,Action actionEnd=null,
                                            Action AfterEnd=null)
    {
        Instance.StartCoroutine(ShowAndHideDarkMaskWindowCoroutine(actionStart,actionMiddle,AfterMiddle,actionEnd,AfterEnd));
        
    }

    IEnumerator ShowAndHideDarkMaskWindowCoroutine(Action actionStart=null,Action actionMiddle=null,Action AfterMiddle=null ,Action actionEnd=null,
                                                    Action AfterEnd=null)
    {
        actionStart?.Invoke();
        
        // 等待ShowDarkMaskWindow执行完成
        bool isShowCompleted = false;
        ShowDarkMaskWindow(() => {
            isShowCompleted = true;
        }); 
        yield return new WaitUntil(() => isShowCompleted);
        

        actionMiddle?.Invoke();
        yield return new WaitForSeconds(0.2f);
        AfterMiddle?.Invoke();

        bool isHideCompleted = false;
        HideDarkMaskWindow(() => {
            isHideCompleted = true;
        });
        yield return new WaitUntil(() => isHideCompleted);


        actionEnd?.Invoke();
        AfterEnd?.Invoke();
    }

}
