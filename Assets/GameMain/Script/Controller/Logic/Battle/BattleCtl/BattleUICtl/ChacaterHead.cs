using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ZMGCFrameWork.Battle
{
    public class ChacaterHead : MonoBehaviour
    {
        public List<RectTransform> rectTransformList;
        public GridLayoutGroup gridLayoutGroup;

        public List<ChacaterHeadUI> chacaterHeadUIList = new List<ChacaterHeadUI>();
        public ChacaterHeadUI currentFocusedChacaterHeadUI;


        public void Initialize(List<CharacterLogic> playerLogicList, CharacterLogic currentFocusedPlayer)
        {

            foreach (CharacterLogic characterLogic in playerLogicList)
            {
                GameObject obj = ResourcesMgr.Instance.LoadGameObject(TestAssetsPath.chacaterHeadUI, rectTransformList[characterLogic.posID]);
                ChacaterHeadUI chacaterHeadUI = obj.GetComponent<ChacaterHeadUI>();
                chacaterHeadUI.Initialize(characterLogic);
                chacaterHeadUIList.Add(chacaterHeadUI);
            }
            SetSelected(currentFocusedPlayer);

        }

        //控制当前镜头角色头像的选中特效（放大图片）
        public void SetSelected(CharacterLogic currentFocusedPlayer)
        {

            // 如果之前有选中的角色，先恢复其原始大小
            if (currentFocusedChacaterHeadUI != null)
            {

                currentFocusedChacaterHeadUI.RestoreOriginalSize();
            }

            foreach (ChacaterHeadUI chacaterHeadUI in chacaterHeadUIList)
            {
                if (chacaterHeadUI.characterRender.characterLogic == currentFocusedPlayer)
                {

                    chacaterHeadUI.ExpandImageLeft(50, 50);
                    currentFocusedChacaterHeadUI = chacaterHeadUI;
                }
            }
        }

        public void OnDestroy()
        {
            // 清理角色头像UI列表
            if (chacaterHeadUIList != null)
            {
                for (int i = 0; i < chacaterHeadUIList.Count; i++)
                {
                    if (chacaterHeadUIList[i] != null)
                    {
                        chacaterHeadUIList[i].OnDestroy();
                        chacaterHeadUIList[i] = null;
                    }
                }
                chacaterHeadUIList.Clear();
                chacaterHeadUIList = null;
            }

            // 清理当前焦点角色头像UI
            if (currentFocusedChacaterHeadUI != null)
            {
                currentFocusedChacaterHeadUI.OnDestroy();
                currentFocusedChacaterHeadUI = null;
            }

            // 清理布局组件
            if (gridLayoutGroup != null)
            {
                gridLayoutGroup = null;
            }

            // 清理RectTransform列表
            if (rectTransformList != null)
            {
                for (int i = 0; i < rectTransformList.Count; i++)
                {
                    rectTransformList[i] = null;
                }
                rectTransformList.Clear();
                rectTransformList = null;
            }
        }
    }
}