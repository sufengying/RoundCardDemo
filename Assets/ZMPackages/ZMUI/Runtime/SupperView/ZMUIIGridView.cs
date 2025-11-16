

using System.Collections;
using System.Collections.Generic;
using SuperScrollView;
using UnityEngine;

namespace ZM.UI
{
    public class ZMUIIGridView : MonoBehaviour
    {
        public LoopGridView loopListView;

        private int m_ViewDataCount = 99; //浣犵殑鏁版嵁鍒楄〃闀垮害 寤鸿鍦ㄥ綋鍓嶆帴鍙ｈЕ鍙戞椂鍚戞暟鎹眰绱㈠彇

        private GetItemDataDelegate m_GetItemDataCallBack = null;


        private void Awake()
        {
            if (loopListView == null)
                loopListView = GetComponent<LoopGridView>();
        }

        /// <summary>
        /// 鍒锋柊鍒楄〃鏄剧ず
        /// </summary>
        /// <param name="reSetPos">鏄惁閲嶇疆鍒伴《閮ㄦ垨搴曢儴</param>
        /// <param name="viewDataCount">鏁版嵁闀垮害</param>
        /// <param name="getItemDataCallBack">鑾峰彇鏁版嵁鍥炶皟</param>
        public void RefreshListView(bool reSetPos, int viewDataCount, GetItemDataDelegate getItemDataCallBack)
        {
            m_ViewDataCount = viewDataCount;
            m_GetItemDataCallBack = getItemDataCallBack;

            if (!loopListView.ListViewInited)
            {
                //鍒濆鍖栨粴鍔ㄥ垪琛? 鍒囪涓嶅彲鍦ˋwake鎴朣tart涓垵濮嬪寲Count=0鐨勫垪琛ㄣ€係upperView浼氭湁BUG
                loopListView.InitGridView(m_ViewDataCount, OnShowItemByIndex);
            }
            else
            {
                //鏁版嵁鍙戠敓鍙樺寲锛岄噸鏂拌缃渶鏂扮殑鏁版嵁锛屾暟鎹鍒犲繀椤昏璋冪敤姝ゆ帴鍙ｏ紝鍚﹀垯浼氬嚭鐜癷tem绱㈠紩涓庢暟鎹笉涓€鑷村拰涓€鍒囧叾浠栫殑鏄剧ずBUG
                loopListView.SetListItemCount(m_ViewDataCount, false);
                if (reSetPos)
                {
                    loopListView.MovePanelToItemByIndex(0, 0);
                    loopListView.RefreshAllShownItem();
                }
                else
                {
                    loopListView.RefreshAllShownItem();
                }
            }
        }

        /// <summary>
        /// Item鍏冪礌鏄剧ず鍥炶皟
        /// </summary>
        /// <param name="listView">婊氬姩鍒楄〃</param>
        /// <param name="index">item绱㈠紩</param>
        /// <returns></returns>
        LoopGridViewItem OnShowItemByIndex(LoopGridView gridView, int index,int row,int column)
        {
            if (index < 0 || index >= m_ViewDataCount) return null;

            //鑾峰彇item鏄剧ず鏁版嵁
            object itemData = m_GetItemDataCallBack(index);
            if (itemData == null) return null;

            if (loopListView.ItemPrefabDataList.Count == 0)
            {
                Debug.LogError("ItemPrefabDataList is null!");
                return null;
            }
            

            //鍒涘缓瀵瑰簲item棰勫埗浣?
            LoopGridViewItem item = gridView.NewListViewItem(loopListView.ItemPrefabDataList[0].mItemPrefab.name);
            //鑾峰彇Item涓婄殑鑴氭湰缁勪欢
            IZMUIViewListItem itemScript = item.GetComponent<IZMUIViewListItem>();
           

            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //item鑴氭湰鍒濆鍖栨帴鍙?
                itemScript.InitListItem();
            }

            //璁剧疆Item鑴氭湰鏁版嵁
            itemScript.SetListItemShowData(index, itemData);
            return item;
        }


        public void OnRelease()
        {
           var itemScriptArr = loopListView.ContainerTrans.GetComponentsInChildren<IZMUIViewListItem>(true);
           foreach (var item in itemScriptArr)
           {
               item.OnRelease();
           }
        }
    }
}
