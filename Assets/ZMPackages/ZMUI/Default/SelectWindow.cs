
using UnityEngine.UI;
using UnityEngine;
using System;
using ZMUIFrameWork;

public enum SelectType
{
    Normal,
    Only_OK,
}

// 1.请在菜单 编辑器扩展/Namespace Settings 里设置命名空间
public class SelectWindow : WindowBase
{
    private Action mOnSureCallback;
    public Action mOnCancelCallback;

    public SelectWindowDataComponent dataCompt;

    private SelectType mSelectType;
    #region 生命周期函数
    //优先执行 但只在实例化后执行一次
    public override void OnAwake()
    {
        dataCompt = gameObject.GetComponent<SelectWindowDataComponent>();
        dataCompt.InitComponent(this);
        base.OnAwake();
    }
    //物体显示时执行
    public override void OnShow()
    {
        base.OnShow();

        mOnSureCallback = null;
        mOnCancelCallback = null;
    }
    //Window隐藏时触发
    public override void OnHide()
    {
        base.OnHide();
    }
    //Window销毁时触发
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion

    #region AIP Function
    public void InitViewState(SelectType type, string content, Action sureCallback=null, Action cancelCallback=null, string suretext = "确认", string canceltext = "取消", string title = "提示", TextAnchor aligment = TextAnchor.MiddleCenter)
    {
        mSelectType = type;
        mOnSureCallback = sureCallback;
        mOnCancelCallback = cancelCallback;
        //dataCompt.TitleText.text = title;
        dataCompt.ContentText.text = content;
        dataCompt.ContentText.alignment = aligment;
        dataCompt.OKText.text = suretext;
        dataCompt.CancelText.text = canceltext;
        dataCompt.OKButton.gameObject.SetActive(type == SelectType.Normal || type == SelectType.Only_OK);
        dataCompt.CancelButton.gameObject.SetActive(type == SelectType.Normal);
    }
    #endregion

    #region UI组件事件
    public void OnOKButtonClick()
    {
        HideWindow();
        mOnSureCallback?.Invoke();
    }
    public void OnCancelButtonClick()
    {
        HideWindow();
        mOnCancelCallback?.Invoke();
    }

    #endregion
}
