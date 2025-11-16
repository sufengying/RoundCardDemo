/*---------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Author:铸梦
 *Date:2025/5/25 19:23:41
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，再次生成后会以代码追加的形式新增,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using System;

namespace ZM.UI
{
	public class SelectCharaItem : MonoBehaviour, IZMUIViewListItem
	{
		#region 自定义字段
		public Image CharacterIconImage;

		public Button SelectedButton;

		public CanvasGroup shadowCanvasGroup;

		private CharacterConfig m_characterConfig;

		private bool isInit = false;

		private Action setShadowCanvasGroup;
		private Action<CharacterConfig> setDataOnShow;


		#endregion


		#region 生命周期
		//脚本初始化接口 (为保证生命周期的执行顺序，请在View层调用该接口确保需要初始化的数据正常执行)
		public void OnInitialize()
		{

		}
		//物体设置数据接口 (请自定以你的参数，方便外部调用传参)
		public void SetItemData()
		{


		}
		//物体销毁时执行 (为保证生命周期的执行顺序，请在View层调用该接口确保需要释放时的接口正常调用)
		public void OnDispose()
		{
		}
		#endregion


		#region UI组件事件
		private void OnSelectedButtonClick()
		{
			
			if (shadowCanvasGroup.alpha == 1)
			{			
				EventCenter.Instance.TriggerAction("SelectCharacterWindow_SelectCharaItem_SetShadow");
				EventCenter.Instance.TriggerAction("SelectCharacter_left_SetNullData");
			}
			else if (shadowCanvasGroup.alpha == 0)
			{
				EventCenter.Instance.TriggerAction("SelectCharacterWindow_SelectCharaItem_SetShadow");
				shadowCanvasGroup.alpha = 1;
				EventCenter.Instance.TriggerAction("SelectCharacter_left_SetItemData", m_characterConfig);

			}

			if (shadowCanvasGroup.alpha == 1)
			{
				EventCenter.Instance.TriggerAction("SelectCharacterWindowDataComponent_SetCurrentCharacterConfig", m_characterConfig);
			}
			else if (shadowCanvasGroup.alpha == 0)
			{
				EventCenter.Instance.TriggerAction<CharacterConfig>("SelectCharacterWindowDataComponent_SetCurrentCharacterConfig", null);
			}
		}

		private void SetShadow()
		{
			if (shadowCanvasGroup.alpha != 0)
			{
				shadowCanvasGroup.alpha = 0;
			}
		}

		private void SetDataOnShow(CharacterConfig otherCharacterConfig)
		{
			if(otherCharacterConfig == m_characterConfig)
			{
				
				OnSelectedButtonClick();
			}

		}


		#endregion

		public void InitListItem()
		{

			shadowCanvasGroup = GetComponentInChildren<CanvasGroup>();
			//按钮事件自动注册绑定
			SelectedButton.onClick.AddListener(OnSelectedButtonClick);

			setShadowCanvasGroup = SetShadow;
			setDataOnShow = SetDataOnShow;
			EventCenter.Instance.AddListener("SelectCharacterWindow_SelectCharaItem_SetShadow", setShadowCanvasGroup);
			EventCenter.Instance.AddListener("SelectCharacterWindow_SelectCharaItem_SetDataOnShow", setDataOnShow);
		}

		public void SetListItemShowData(int index, params object[] data)
		{

			if (isInit == false && m_characterConfig == null)
			{
				m_characterConfig = Resources.Load<CharacterConfig>($"prefabs/CharacterConfig/Player/{index + 1001}");
				CharacterIconImage.sprite = m_characterConfig.characterPainting;
				isInit = true;

			}
			else if (isInit == true && m_characterConfig != null)
			{
				CharacterIconImage.sprite = m_characterConfig.characterPainting;
			}

		}

		public void OnRelease()
		{
			EventCenter.Instance.RemoveListener("SelectCharacterWindow_SelectCharaItem_SetShadow");
			EventCenter.Instance.RemoveListener("SelectCharacterWindow_SelectCharaItem_SetDataOnShow");
		}


	}
}
