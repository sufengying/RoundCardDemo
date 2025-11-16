/*---------------------------------
 *Title:UI自动化组件生成代码生成工�?
 *Author:铸梦
 *Date:2025/5/29 18:16:49
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—�? 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using DG.Tweening;
using System.Collections;
using System;

namespace ZM.UI
{
	public class FailureWindowDataComponent : MonoBehaviour
	{
		public Button closeButton;

		public Image bpImage;

		public Text textText;

		public void InitComponent(WindowBase target)
		{
			//组件事件绑定
			FailureWindow mWindow = (FailureWindow)target;
			target.AddButtonClickListener(closeButton, mWindow.OncloseButtonClick);
		}

		public void ShowWindowAnimation(Action action)
		{
			
			// 创建一个序列来同时控制两个材质
			Sequence sequence = DOTween.Sequence();

			// 添加第一个材质的动画
			sequence.Join(DOTween.To(
				() => bpImage.material.GetFloat("_DissolveAmount"),
				x => bpImage.material.SetFloat("_DissolveAmount", x),
				0f,
				3f
			).SetEase(Ease.Linear));

			// 添加第二个材质的动画
			sequence.Join(DOTween.To(
				() => textText.material.GetFloat("_DissolveAmount"),
				x => textText.material.SetFloat("_DissolveAmount", x),
				0f,
				3f
			).SetEase(Ease.Linear));

			// 设置完成回调
			sequence.OnComplete(() =>
			{
				action?.Invoke();
				
			});
		}

		public void HideWindowAnimation(Canvas Canvas, bool mDisableAnim, CanvasGroup mUIMaskCanvasGroup, string Name)
		{
			if (Canvas.sortingOrder > 90 && mDisableAnim == false)
			{
				mUIMaskCanvasGroup.DOFade(0, 0.1f).OnComplete(() =>
				{
					UIModule.Instance.HideWindow(name);
				});
			}
			else
			{
				UIModule.Instance.HideWindow(name);
			}
		}

		public void HideWindowAnimation(CanvasGroup mCanvasGroup)
		{

			//mCanvasGroup.DOFade(0, 0.1f).SetEase(Ease.Linear).WaitForCompletion();
			StartCoroutine(FadeAnimation(mCanvasGroup, 0, 0.1f));

		}

		IEnumerator FadeAnimation(CanvasGroup canvasGroup, float targetAlpha, float duration)
		{
			canvasGroup.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
			yield return new WaitForSeconds(duration);


		}
	}
}
