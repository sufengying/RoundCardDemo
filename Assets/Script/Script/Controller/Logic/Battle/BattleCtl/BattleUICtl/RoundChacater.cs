using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace ZMGCFrameWork.Battle
{
    public class RoundChacater : MonoBehaviour
    {
        public List<RectTransform> rectTransformList;
        public GridLayoutGroup gridlayoutGroup;
        // 使用字典通过角色ID快速查找UI元素，提高效率和健壮性
        private Dictionary<int, ChacaterRoundInfo> playerInfoUIMap = new Dictionary<int, ChacaterRoundInfo>();
        // 当前选中的角色UI元素
        private ChacaterRoundInfo currentSelectedUI;

        public void Initialize(List<CharacterLogic> SortList)
        {
            if (rectTransformList == null || rectTransformList.Count == 0)
            {
                Debug.LogError("RoundChacater_rectTransformList为空或者数量为零");
                return;
            }
            if (SortList == null)
            {
                Debug.LogError("RoundChacater_SortList为空");
                return;
            }

            // 清理旧的UI（如果可能重入）
            CleanupPlayerUIElements();

            for (int i = 0; i < SortList.Count; i++)
            {
                // 健壮性检查
                if (i >= rectTransformList.Count)
                {
                    Debug.LogError($"RoundChacater_rectTransformList数量不足");
                    return;
                }
                if (SortList[i] == null)
                {
                    Debug.LogWarning($"RoundChacater_SortList[i]为空");
                    return;
                }
                if (rectTransformList[i] == null)
                {
                    Debug.LogWarning($"RoundChacater_rectTransformList[i]为空");
                    return;
                }

                // 加载UI元素实例
                ChacaterRoundInfo playerInfoUI = UIMgr.Instance.LoadPlayerBattleUI(rectTransformList[i]);
                if (playerInfoUI == null)
                {
                    Debug.LogError($"RoundChacater_OnCreate_LoadPlayerBattleUI失败{i}");
                    return;
                }

                var characterLogic = SortList[i];
                int characterID = characterLogic.InstanceID;

                // 关联UI与角色ID
                playerInfoUI.AssociateWithCharacterID(characterID);
                
                playerInfoUI.gameObject.name = $"{characterLogic.name}";
                if (characterLogic.m_characterConfig.characterIcon != null)
                {
                    playerInfoUI.playerIcon.sprite = characterLogic.m_characterConfig.characterIcon;
                }

                // 添加到字典进行管理
                if (!playerInfoUIMap.ContainsKey(characterID))
                {
                    playerInfoUIMap.Add(characterID, playerInfoUI);
                }
                else
                {
                    Debug.LogWarning($"RoundChacater_Duplicate Character ID {characterID} found. Overwriting UI element in map.");
                    if (playerInfoUIMap.ContainsKey(characterID) && playerInfoUIMap[characterID] != null)
                    {
                        //销毁旧的UI元素
                        UIMgr.Instance.UnloadPlayerBattleUI(playerInfoUIMap[characterID]);
                    }
                    playerInfoUIMap[characterID] = playerInfoUI;
                }
            }
            // 设置初始选中效果
            if (SortList.Count > 0 && SortList[0] != null)
            {
                if (playerInfoUIMap.TryGetValue(SortList[0].InstanceID, out var firstUI))
                {
                    firstUI.PlaySelectedEffect();
                    currentSelectedUI = firstUI;
                    currentSelectedUI.SetShadowAlpha(true);
                }
            }
        }
        private void CleanupPlayerUIElements()
        {
            if (playerInfoUIMap != null)
            {
                foreach (var kvp in playerInfoUIMap)
                {
                    if (kvp.Value != null)
                    {
                        DOTween.Kill(kvp.Value.transform);
                        DOTween.Kill(kvp.Value.canvasGroup);
                        UIMgr.Instance.UnloadPlayerBattleUI(kvp.Value);
                    }
                }
                playerInfoUIMap.Clear();
            }
            currentSelectedUI = null;
        }

        public void UpdateRoundList(CharacterLogic characterLogicLast, List<CharacterLogic> newSortList)
        {
            if (rectTransformList == null || newSortList == null)
            {
                Debug.LogError("BattleUICtl UpdateList: Dependencies not initialized properly (roundChacater, rectTransformList, or newSortList).");
                return;
            }

            // 1. 取消之前的选中动画
            if (currentSelectedUI != null)
            {
                currentSelectedUI.StopSelectedEffect();
                currentSelectedUI.SetShadowAlpha(false);
                currentSelectedUI = null;
            }

            // 2. 处理刚行动结束的角色
            if (characterLogicLast != null)
            {
                if (playerInfoUIMap.TryGetValue(characterLogicLast.InstanceID, out var lastActorUI))
                {
                    // 找到刚行动结束的角色在新排序列表中的索引（位置）
                    int targetIndex = newSortList.FindIndex(characterLogic => characterLogic != null && characterLogic.InstanceID == characterLogicLast.InstanceID);

                    if (targetIndex != -1 && targetIndex < rectTransformList.Count && rectTransformList[targetIndex] != null)
                    {
                        // 将刚行动结束的角色UI元素设置到对应的新父物体下
                        lastActorUI.transform.SetParent(rectTransformList[targetIndex], true);
                        // 淡出UI
                        lastActorUI.FadeOutUI();
                        // 更新UI上的角色行动值
                        //
                    }
                    else
                    {
                        Debug.LogWarning($"BattleUICtl UpdateList: Could not find valid target slot index ({targetIndex}) for last actor InstanceID {characterLogicLast.InstanceID} in newSortList or rectTransformList.");
                        lastActorUI.gameObject.SetActive(false);
                    }
                }
            }

            // 3. 处理列表中的其他角色
            for (int i = 0; i < newSortList.Count; i++)
            {
                if (i >= rectTransformList.Count)
                {
                    Debug.LogError($"BattleUICtl UpdateList: Not enough UI slots in RoundChacater ({rectTransformList.Count}) for character index {i}.");
                    return;
                }
                if (newSortList[i] == null)
                {
                    Debug.LogWarning($"BattleUICtl UpdateList: Character data at index {i} is null or has no Player.");
                    return;
                }
                if (rectTransformList[i] == null)
                {
                    Debug.LogWarning($"BattleUICtl UpdateList: UI Slot RectTransform at index {i} is null.");
                    return;
                }

                var currentCharacter = newSortList[i];
                int currentCharacterID = currentCharacter.InstanceID;

                if (characterLogicLast != null && currentCharacterID == characterLogicLast.InstanceID)
                {
                    continue;
                }

                if (playerInfoUIMap.TryGetValue(currentCharacterID, out var currentItemUI))
                {
                    //currentItemUI.playerActionValue.text = currentCharacter.action.ActionValue.ToString();
                    currentItemUI.transform.SetParent(rectTransformList[i], true);
                    currentItemUI.Move();
                }
            }

            // 4. 重新设置当前选中的 UI（列表第一位）
            if (newSortList.Count > 0 && newSortList[0] != null)
            {
                if (playerInfoUIMap.TryGetValue(newSortList[0].InstanceID, out var newSelectedUI))
                {
                    currentSelectedUI = newSelectedUI;
                    currentSelectedUI.PlaySelectedEffect();
                    currentSelectedUI.SetShadowAlpha(true);
                }
            }
        }

        public void RemoveCharacterUI(CharacterLogic characterLogic)
        {
            if(playerInfoUIMap.TryGetValue(characterLogic.InstanceID,out var characterUI))
            {
                characterUI.FadeOutUI_NoDelay();
                playerInfoUIMap.Remove(characterLogic.InstanceID);
            }
        }

        public void OnDestroy()
        {
            // 清理角色信息列表
            if (playerInfoUIMap != null)
            {
                foreach (var kvp in playerInfoUIMap)
                {
                    if (kvp.Value != null)
                    {
                        DOTween.Kill(kvp.Value.transform);
                        DOTween.Kill(kvp.Value.canvasGroup);
                        UIMgr.Instance.UnloadPlayerBattleUI(kvp.Value);
                    }
                }
                playerInfoUIMap.Clear();
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

            // 清理布局组件
            if (gridlayoutGroup != null)
            {
                gridlayoutGroup = null;
            }

            // 清理当前选中的UI
            if (currentSelectedUI != null)
            {
                currentSelectedUI = null;
            }
        }
    }
}
