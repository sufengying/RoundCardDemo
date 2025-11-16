/*---------------------------------
 *Title:UI表现层脚本自动化生成工具
 *Author:ZM 铸梦
 *Date:2025/5/25 19:28:55
 *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码
 *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用
---------------------------------*/
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;


namespace ZM.UI
{
	public class SelectWindow : WindowBase
	{

		public SelectWindowDataComponent dataCompt;

		private Action refreshImage;

		// 创建一个字典来记录每个角色配置在数组中的位置
		CharacterConfig[] m_characterConfigList = new CharacterConfig[3];

		#region 生命周期函数
		//调用机制与Mono Awake一致
		public override void OnAwake()
		{
			dataCompt = gameObject.GetComponent<SelectWindowDataComponent>();
			dataCompt.InitComponent(this);
			FullScreenWindow = true;
			mDisableAnim = false;
			isoverrideAnim = true;

			refreshImage = RefreshImage;
			EventCenter.Instance.AddListener("SelectWindowDataComponent_RefreshImage", refreshImage);

			base.OnAwake();
		}
		//物体显示时执行
		public override void OnShow()
		{
			ShowWindowAnimation();
			ClearImage();
			RefreshImage();
		}
		//物体隐藏时执行
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

		private void RefreshImage()
		{
			// 创建一个字典来记录每个角色配置在数组中的位置
			Dictionary<CharacterConfig, int> configPositions = new Dictionary<CharacterConfig, int>();

			// 第一次遍历，检查重复并处理
			for (int i = 0; i < dataCompt.battleWorldConfig.playerConfigList.Length; i++)
			{
				if (dataCompt.battleWorldConfig.playerConfigList[i] != null)
				{
					// 如果这个配置已经存在，说明有重复
					if (configPositions.ContainsKey(dataCompt.battleWorldConfig.playerConfigList[i]))
					{
						// 获取之前的位置
						int previousIndex = configPositions[dataCompt.battleWorldConfig.playerConfigList[i]];
						// 将之前位置的角色设为null
						dataCompt.battleWorldConfig.playerConfigList[previousIndex] = null;
					}
					// 记录当前位置
					configPositions[dataCompt.battleWorldConfig.playerConfigList[i]] = i;
				}
			}

			// 更新图片显示
			for (int i = 0; i < dataCompt.battleWorldConfig.playerConfigList.Length; i++)
			{
				if (dataCompt.battleWorldConfig.playerConfigList[i] != null)
				{
					dataCompt.image[i].sprite = dataCompt.battleWorldConfig.playerConfigList[i].characterPainting;
					dataCompt.image[i].SetNativeSize();
				}
				else
				{
					dataCompt.image[i].sprite = Resources.Load<Sprite>("prefabs/Icon/RoleImage/AddGlow");
					dataCompt.image[i].SetNativeSize();
				}
			}
		}

		private void ClearImage()
		{

			// 再清空数组
			Array.Clear(dataCompt.battleWorldConfig.playerConfigList, 0, dataCompt.battleWorldConfig.playerConfigList.Length);
		}

		#endregion
		#region UI组件事件
		public void OnCloseButtonClick()
		{

			HideWindow();
		}
		public void OnSelect1ButtonClick()
		{
			if (UIModule.Instance.GetWindow("SelectCharacterWindow") != null)
			{

				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 1, dataCompt.battleWorldConfig.playerConfigList[0]);
				PopUpWindow<SelectCharacterWindow>();
			}
			else
			{

				PopUpWindow<SelectCharacterWindow>();
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 1, dataCompt.battleWorldConfig.playerConfigList[0]);
			}

		}
		public void OnSelect2ButtonClick()
		{
			if (UIModule.Instance.GetWindow("SelectCharacterWindow") != null)
			{
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 2, dataCompt.battleWorldConfig.playerConfigList[1]);
				PopUpWindow<SelectCharacterWindow>();
			}
			else
			{
				PopUpWindow<SelectCharacterWindow>();
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 2, dataCompt.battleWorldConfig.playerConfigList[1]);
			}

		}
		public void OnSelect3ButtonClick()
		{
			if (UIModule.Instance.GetWindow("SelectCharacterWindow") != null)
			{
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 3, dataCompt.battleWorldConfig.playerConfigList[2]);
				PopUpWindow<SelectCharacterWindow>();
			}
			else
			{
				PopUpWindow<SelectCharacterWindow>();
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetSelectIndex", 3, dataCompt.battleWorldConfig.playerConfigList[2]);
			}

		}
		public void OnStartWorldButtonClick()
		{


			for (int i = 0; i < dataCompt.battleWorldConfig.playerConfigList.Length; i++)
			{
				if (dataCompt.battleWorldConfig.playerConfigList[i] != null)
				{
					
					dataCompt.battleWorldConfig.playerConfigList[i].posID = (MapPosID)i;
				}
			}

			DarkMaskWindow.Instance.ShowAndHideDarkMaskWindow(
							actionStart:null,
							actionMiddle:CutSceneWindow.Instance.ShowCutSceneWindow,
							AfterMiddle:() =>
							{
								CutSceneWindow.Instance.ResetSliderValue();
							},
							actionEnd:() =>
							{
								EventCenter.Instance.TriggerAction("GameMain_SetBattleWorldConfig", dataCompt.battleWorldConfig);
								EventCenter.Instance.TriggerAction("EnterBattleWorld");
								UIModule.Instance.HideAllWindow();
								HideWindow();
								

							},
							AfterEnd:()=>
							{
								CutSceneWindow.Instance.PlayAnimation(
									() =>
									{
										EventCenter.Instance.TriggerAction("BattleUICtl_HideBattleUI", false);
										EventCenter.Instance.TriggerAction("ActionCtl_SetCurrentCharacter");
									}
								);
							}
							);



		}
		#endregion
	}
}
