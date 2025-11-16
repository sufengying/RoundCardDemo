/*---------------------------------
 *Title:UI自动化组件生成代码生成工�???
 *Author:铸梦
 *Date:2025/5/14 10:30:56
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—�? 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

namespace ZM.UI
{
	public class BarrierInfoWindowDataComponent : MonoBehaviour
	{
		public Button CreatBattleWorldButton;

		public Text titleText;

		public Image baseMap;

		public void InitComponent(WindowBase target)
		{
			//组件事件绑定
			BarrierInfoWindow mWindow = (BarrierInfoWindow)target;
			target.AddButtonClickListener(CreatBattleWorldButton, mWindow.OnCreatBattleWorldButtonClick);
		}

		public void OnChangedTitleInput(string title)
		{
			titleText.text = title;
		}

		void Update()
		{

			if (Input.GetMouseButtonDown(0)&&EventCenter.Instance.TriggerAction<CanvasGroup>("HideBarrierInfoWindow_GetCanvasGroup").alpha == 1)
			{
				
				var obj = DetectUIControlAtMouse();
				

				if (obj.name == "[Button]Close"||obj.name == "BaseMap")
				{
					
					EventCenter.Instance.TriggerAction("HideBarrierInfoWindow_OnHideWindow");
				}

			}
		}

		public GameObject DetectUIControlAtMouse()
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			eventData.position = Input.mousePosition;

			List<RaycastResult> raycastResults = new List<RaycastResult>();

			EventSystem.current.RaycastAll(eventData, raycastResults);
			if (raycastResults.Count > 0)
			{
				return raycastResults[0].gameObject;
			}
			return null;
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

		public void ShowWindowAnimation(Canvas Canvas, bool mDisableAnim, CanvasGroup mUIMaskCanvasGroup, CanvasGroup mCanvasGroup)
		{
			//基础弹窗不需要动�?
			if (Canvas.sortingOrder > 90 && mDisableAnim == false)
			{
				//Mask动画
				mUIMaskCanvasGroup.alpha = 0;
				mUIMaskCanvasGroup.DOFade(1, 0.2f);


				//mCanvasGroup.DOFade(1, 0.2f).SetEase(Ease.Linear).WaitForCompletion();
				StartCoroutine(FadeAnimation(mCanvasGroup, 1, 0.2f));
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
