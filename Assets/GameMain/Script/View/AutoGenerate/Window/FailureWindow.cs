/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/5/29 18:04:28
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使�?
---------------------------------*/
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using ZMGCFrameWork.Battle;

namespace ZM.UI
{
	public class FailureWindow : WindowBase
	{

		public FailureWindowDataComponent dataCompt;

		#region 生命周期函数
		//调用机制与Mono Awake一�?
		public override void OnAwake()
		{
			dataCompt = gameObject.GetComponent<FailureWindowDataComponent>();
			dataCompt.InitComponent(this);
			FullScreenWindow = true;
			mDisableAnim = false;
			isoverrideAnim = true;

			dataCompt.bpImage.material.SetFloat("_DissolveAmount", 1);
			dataCompt.textText.material.SetFloat("_DissolveAmount", 1);
			dataCompt.closeButton.interactable = false;


			base.OnAwake();
		}
		//物体显示时执�?
		public override void OnShow()
		{
			dataCompt.bpImage.material.SetFloat("_DissolveAmount", 1);
			dataCompt.textText.material.SetFloat("_DissolveAmount", 1);
			mCanvasGroup.alpha = 1;
			dataCompt.closeButton.interactable = false;
			ShowWindowAnimation();
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
		public override void HideWindow()
		{
			dataCompt.HideWindowAnimation(Canvas, mDisableAnim, mUIMaskCanvasGroup, Name);
		}

		private void ShowWindowAnimation()
		{
			dataCompt.ShowWindowAnimation(SetCloseButtonInteractable);
		}

		private void SetCloseButtonInteractable()
		{
			dataCompt.closeButton.interactable = true;
		}

		#endregion
		#region UI组件事件
		public void OncloseButtonClick()
		{
			
			DarkMaskWindow.Instance.ShowAndHideDarkMaskWindow(null,
			CutSceneWindow.Instance.ShowCutSceneWindow,
			() =>
			{
				CutSceneWindow.Instance.ResetSliderValue();
				HideWindow();
			},
			()=>
			{
				CutSceneWindow.Instance.PlayAnimation();
			},
			() =>
			{
				WorldManager.DestroyWorld<BattleWorld>();
				UIModule.Instance.PopUpWindow<HallWindow>();
				UIModule.Instance.PopUpWindow<BattleWorldWindow>();
			});

		}
		#endregion
	}
}
