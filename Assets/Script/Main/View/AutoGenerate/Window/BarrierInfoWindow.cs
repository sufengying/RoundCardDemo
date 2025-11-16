/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/5/14 10:31:06
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使�??
---------------------------------*/
using UnityEngine.UI;
using UnityEngine;
using System;
using DG.Tweening;

namespace ZM.UI
{
	public class BarrierInfoWindow : WindowBase
	{

		public BarrierInfoWindowDataComponent dataCompt;
		private Action OnClickClose;
		private Func<CanvasGroup> getCanvasGroup;

		#region 生命周期函数
		//调用机制与Mono Awake一�??
		public override void OnAwake()
		{
			OnClickClose = HideWindow;
			getCanvasGroup = GetCanvasGroup;
			EventCenter.Instance.AddListener("HideBarrierInfoWindow_OnHideWindow", OnClickClose);
			EventCenter.Instance.AddListener("HideBarrierInfoWindow_GetCanvasGroup", getCanvasGroup);

			dataCompt = gameObject.GetComponent<BarrierInfoWindowDataComponent>();
			dataCompt.InitComponent(this);
			FullScreenWindow = false;
			isoverrideAnim = true;
			base.OnAwake();
		}
		//物体显示时执�??
		public override void OnShow()
		{
	
			ShowWindowAnimation();
		}
		//物体隐藏时执�??
		public override void OnHide()
		{
			dataCompt.HideWindowAnimation(mCanvasGroup);
			base.OnHide();
		}
		//物体销毁时执行
		public override void OnDestroy()
		{
			base.OnDestroy();
		}
		#endregion
		#region API Function
		public override void HideWindow()
		{
			dataCompt.HideWindowAnimation(Canvas, mDisableAnim, mUIMaskCanvasGroup, Name);
		}

		private void ShowWindowAnimation()
		{
			dataCompt.ShowWindowAnimation(Canvas, mDisableAnim, mUIMaskCanvasGroup, mCanvasGroup);
		}

		public CanvasGroup GetCanvasGroup()
		{
			return mCanvasGroup;
		}
		#endregion
		#region UI组件事件
		public void OnCreatBattleWorldButtonClick()
		{
			//EventCenter.Instance.TriggerAction("EnterBattleWorld");
			PopUpWindow<SelectWindow>();
			
			//UIModule.Instance.HideAllWindow();
		}

		public void OnChangedTitleInput(string title)
		{
			dataCompt.OnChangedTitleInput(title);
		}

		#endregion
	}
}
