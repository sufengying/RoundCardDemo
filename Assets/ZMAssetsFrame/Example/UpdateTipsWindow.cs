
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTipsWindow : MonoBehaviour
{
    //更新回调
    Action OnUpdateCallBack;
    //退出回调
    Action OnQuitCallBack;
    //内容文本
    public Text contentText;
    public CanvasGroup canvasGroup;

    public void InitView(string content, Action updateCallBack, Action quitCallBack)
    {
        OnUpdateCallBack = updateCallBack;
        OnQuitCallBack = quitCallBack;
        contentText.text = content;
    }

    public void OnUpdateButtonClick()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        OnUpdateCallBack?.Invoke();
       
        Destroy(gameObject);
    }

    public void OnQuitButtonClick()
    {
        OnQuitCallBack?.Invoke();
        Destroy(gameObject);
    }
}
