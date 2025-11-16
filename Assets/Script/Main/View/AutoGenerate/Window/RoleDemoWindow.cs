/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/5/25 15:37:18
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使�?
---------------------------------*/
using UnityEngine.UI;
using UnityEngine;
using System;

namespace ZM.UI
{
	public class RoleDemoWindow : WindowBase
	{

		public RoleDemoWindowDataComponent dataCompt;

		#region 生命周期函数
		//调用机制与Mono Awake一�?
		public override void OnAwake()
		{
			dataCompt = gameObject.GetComponent<RoleDemoWindowDataComponent>();
			dataCompt.InitComponent(this);
			FullScreenWindow = true;
			mDisableAnim = false;
			isoverrideAnim = true;
			base.OnAwake();
		}
		//物体显示时执�?
		public override void OnShow()
		{
			Debug.Log("OnShow_RoleDemoWindow");
			ShowWindowAnimation();
			RefershViewList();
			
		}


		//物体隐藏时执�?
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
		private void RefershViewList()
		{
			dataCompt.TaskZMUIIGridView.RefreshListView(true, 2, (index) => { return index; });
		}
		public override void HideWindow()
		{
			dataCompt.HideWindowAnimation(Canvas, mDisableAnim, mUIMaskCanvasGroup, Name);
		}

		private void ShowWindowAnimation()
		{
			dataCompt.ShowWindowAnimation(Canvas, mDisableAnim, mUIMaskCanvasGroup, mCanvasGroup);
		}


		#endregion
		#region UI组件事件
		public void OnCloseButtonClick()
		{
			HideWindow();
		}
		#endregion
	}
}
