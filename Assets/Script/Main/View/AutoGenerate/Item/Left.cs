/*---------------------------------
 *Title:UI自动化组件生成代码生成工�?
 *Author:铸梦
 *Date:2025/5/25 19:22:51
 *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—�? 一键生成UI数据组件脚本即可
 *注意:以下文件是自动生成的，再次生成后会以代码追加的形式新�?,若手动修改后,尽量避免自动生成
---------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using System;

namespace ZM.UI
{
	public class Left:MonoBehaviour
	{
		#region 自定义字�?
		public   Text  nameText;

		public   Text  hpText;

		public   Text  defText;

		public   Text  atkText;

		public   Text  resText;

		public   Text  minPointText;

		public   Text  maxPointText;

		public   Text  speedText;

		private Action<CharacterConfig> onClick;
		private Action onNullClick;

		#endregion


		#region 生命周期
		//脚本初始化接�? (为保证生命周期的执行顺序，请在View层调用该接口确保需要初始化的数据正常执�?)
		public void OnInitialize()
		{
			onClick=SetItemData;	
			onNullClick=SetNullData;
			EventCenter.Instance.AddListener("SelectCharacter_left_SetItemData",onClick);
			EventCenter.Instance.AddListener("SelectCharacter_left_SetNullData",onNullClick);
			//按钮事件自动注册绑定
		}
		//物体设置数据接口 (请自定以你的参数，方便外部调用传�?)
		private  void SetItemData(CharacterConfig characterConfig)
		{
			nameText.text = characterConfig.characterName;
			hpText.text = characterConfig.maxHp.ToString();
			defText.text = characterConfig.def.ToString();
			atkText.text = characterConfig.atk.ToString();
			resText.text = characterConfig.res.ToString();
			minPointText.text = characterConfig.firstSkillPoints.ToString();
			maxPointText.text = characterConfig.maxSkillPoints.ToString();
			speedText.text = characterConfig.speed.ToString();

		}

		private void SetNullData()
		{
			nameText.text = "";
			hpText.text = "";
			defText.text = "";
			atkText.text = "";
			resText.text = "";
			minPointText.text = "";
			maxPointText.text = "";
			speedText.text = "";
		}


		//物体销毁时执行 (为保证生命周期的执行顺序，请在View层调用该接口确保需要释放时的接口正常调�?)
		public  void OnDispose()
		{
			EventCenter.Instance.RemoveListener("SelectCharacter_left_SetItemData");
			EventCenter.Instance.RemoveListener("SelectCharacter_left_SetNullData");
		}
		#endregion


		#region UI组件事件
		 #endregion


	}
}
