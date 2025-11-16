using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZMGCFrameWork.Battle;


public class ChacaterRoundInfo : MonoBehaviour
{
    public Image playerIcon;
    
    public CanvasGroup shadowCanvasGroup;
    // 存储该UI元素代表的角色ID
    public int CharacterID { get; private set; } = -1; // 默认为无效ID

    public RectTransform playerRectTransform;
    public CanvasGroup canvasGroup;
    private Tween selectedTween; // 用于记录当前选中动画
    private Vector2 originalPosition; // 存储原始位置
    private Sequence currentSequence; // 用于管理动画序列

    private void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        playerRectTransform = GetComponent<RectTransform>();
        originalPosition = playerRectTransform.anchoredPosition; // 保存原始位置
    }

    private void OnDisable()
    {
        CleanupAnimations();
    }

    public void SetShadowAlpha(bool isShow)
    {
        if(isShow)
        {
            shadowCanvasGroup.alpha = 1;
        }
        else
        {
            shadowCanvasGroup.alpha = 0;
        }
    }   

    /// <summary>
    /// 将该UI元素与特定角色的ID关联
    /// </summary>
    /// <param name="characterID">角色的唯一ID</param>
    public void AssociateWithCharacterID(int characterID)
    {
        CharacterID = characterID;
    }

    // 淡入动画
    public void FadeInUI()
    {
        CleanupAnimations();
        currentSequence = DOTween.Sequence();

        // 重置位置到原始位置
        currentSequence.Append(playerRectTransform.DOAnchorPos(new Vector2(0, 0), 0.2f).SetEase(Ease.OutQuad));

        // 淡入动画
        currentSequence.Join(canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad));

        // 设置完成回调
        currentSequence.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        });
    }

    // 淡出动画
    public void FadeOutUI()
    {
        CleanupAnimations();
        currentSequence = DOTween.Sequence();

        // 保存当前位置
        originalPosition = playerRectTransform.anchoredPosition;

        // 淡出动画
        currentSequence.Append(canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

        // 设置完成回调
        currentSequence.OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            // 重置缩放
            playerRectTransform.localScale = Vector3.one;
            // 延迟一小段时间后淡入
            DOVirtual.DelayedCall(0.2f, FadeInUI);
        });
    }

    public void FadeOutUI_NoDelay()
    {
        CleanupAnimations();
        currentSequence = DOTween.Sequence();
        currentSequence.Append(canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
        currentSequence.OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });
    }

    // 位移动画
    public void Move()
    {
        CleanupAnimations();
        currentSequence = DOTween.Sequence();

        // 添加位移动画到序列中
        currentSequence.Append(
            playerRectTransform.DOAnchorPos(new Vector2(0, 0), 0.5f)
                .SetEase(Ease.OutBounce)
        );
    }

    // 播放选中效果
    public void PlaySelectedEffect()
    {
        // 清理之前的选中动画
        if (selectedTween != null)
        {
            selectedTween.Kill();
            selectedTween = null;
        }

        // 创建新的选中动画
        selectedTween = playerRectTransform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
    }

    // 停止选中效果
    public void StopSelectedEffect()
    {
        if (selectedTween != null)
        {
            selectedTween.Kill();
            selectedTween = null;
        }
        playerRectTransform.localScale = Vector3.one;
    }

    // 清理所有动画
    private void CleanupAnimations()
    {
        if (currentSequence != null)
        {
            currentSequence.Kill();
            currentSequence = null;
        }
        if (selectedTween != null)
        {
            selectedTween.Kill();
            selectedTween = null;
        }
    }

    public void OnDestroy()
    {
        // 清理所有动画
        CleanupAnimations();

        // 清理 UI 组件引用
        if (playerIcon != null)
        {
            playerIcon.sprite = null;
            playerIcon = null;
        }



        if (playerRectTransform != null)
        {
            playerRectTransform = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup = null;
        }

        // 重置位置数据
        originalPosition = Vector2.zero;

        // 重置角色ID
        CharacterID = -1;


    }
}

