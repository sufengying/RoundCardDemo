
using UnityEngine.UI;
using UnityEngine;

namespace ZMUIFrameWork
{
	public class SelectWindowDataComponent:MonoBehaviour
	{
		public   Text  TitleText;

		public   Button  OKButton;

		public   Text  OKText;

		public   Button  CancelButton;

		public   Text  CancelText;

		public   Text  ContentText;

		public  void InitComponent(WindowBase target)
		{
		     //组件事件绑定
		     SelectWindow mWindow=(SelectWindow)target;
		     target.AddButtonClickListener(OKButton,mWindow.OnOKButtonClick);
		     target.AddButtonClickListener(CancelButton,mWindow.OnCancelButtonClick);
		}
	}
}
