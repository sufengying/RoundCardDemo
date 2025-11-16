using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ZMGCFrameWork.Battle
{
    public class ChacaterHeadUI : MonoBehaviour
    {
        //角色头像
        public Image characterIcon;
        //终极技能图标
        public Image UlitmateIcon;
        //血条
        public Slider HPSlider;
        //血条过度
        public Slider transitionSlider;
        //技能点
        public SkillPoint[] SkillPoints;
        //角色渲染体
        public CharacterRender characterRender;

        private int blockNum = 0;//格挡次数
        // 慢动作持续时间
        private float slowMotionDuration = 3f;
        // 慢动作速度
        private float slowMotionScale = 0.1f;

        // 存储原始尺寸和位置
        private Vector2 originalSize;
        private Vector2 originalPosition;
        private bool isOriginalSizeStored = false;

        private float transitionDuration = 0.5f; // 过渡动画持续时间
        private Tweener transitionTweener; // 用于存储当前的过渡动画
        private CancellationTokenSource _cancellationTokenSource; // 用于取消任务

        //手动初始化（我方角色的头像，大招图标，初始技能点，以及更新血条（传入角色渲染体，调用更新血条方法））
        public void Initialize(CharacterLogic characterLogic)
        {
            if (characterLogic == null)
            {
                Debug.LogError("ChacaterHeadUI Initialize: Could not find CharacterLogic component within the 'ChacaterHeadUI' GameObject.");
                return;
            }
            if (characterLogic.team != Team.Player)
            {
                Debug.LogError("只有玩家阵营角色才能需要创建角色头像");
                return;
            }
            if (characterLogic.characterRender == null)
            {
                Debug.LogError("该逻辑体未绑定角色渲染体");
                return;
            }

            characterIcon.sprite = characterLogic.m_characterConfig.characterIcon;
            UlitmateIcon.sprite = characterLogic.m_characterConfig.UlitmateIcon;
            HPSlider.value = 1f;
            transitionSlider.value = 1f;
            BindCharacterRender(characterLogic.characterRender);
            UpdateHPSlider(new int[] { 0 }, new float[] { 0f }, null, null, false);
            UpdateSkillPoints(characterLogic.currentSkillPoints);
        }
        private void BindCharacterRender(CharacterRender characterRender)
        {
            characterRender.chacaterHeadUI = this;
            this.characterRender = characterRender;
        }

        #region 更新血条与跳出血量数字
        /// <summary>
        /// 更新血条，传入攻击前摇，血量，技能伤害，血量数字预制体（异步加载）
        /// </summary>
        /// <param name="attackPreDelay">攻击前摇数组</param>
        /// <param name="value">血量</param>
        /// <param name="skillDamageArray">伤害数字数组</param>
        /// <param name="dameagePrefab">血量数字预制体</param>
        /// <param name="playHitEffect">是否播放受击效果</param>
        public async void UpdateHPSlider(int[] attackPreDelay, float[] value, float[] skillDamageArray = null, GameObject dameagePrefab = null, bool playHitEffect = true)
        {

            blockNum = 0;
            // 创建新的取消令牌
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            for (int i = 0; i < value.Length; i++)
            {
                if (HPSlider.value <= 0)
                {
                    return;
                }
                await Task.Delay(attackPreDelay[i], token);

                if (characterRender.characterLogic.aiState.GetState() == AIStateEnum.block)
                {
                    ++blockNum;
                    characterRender.PlayParticleSystem();
                    characterRender.characterLogic.AddSkillPoints(1);
                    if (blockNum == attackPreDelay.Length)
                    {
                        EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setBlockSkillCanvasGroup", true);
                        StartCoroutine(SlowMotionCoroutine());
                    }
                    continue;
                }

                if (playHitEffect)
                {
                    characterRender.OnEnterHit();
                    characterRender.OnEnterAnimation("hit");
                }

                if (skillDamageArray != null && dameagePrefab != null)
                {
                    UpdateDamageNumber(skillDamageArray[i], dameagePrefab);
                }
                HPSlider.value -= value[i];
                UpdateTransitionSlider(value[i]);
            }
        }

        IEnumerator SlowMotionCoroutine()
        {
            // 开始慢动作
            Time.timeScale = slowMotionScale;

            // 等待实际时间（不受timeScale影响）
            yield return new WaitForSecondsRealtime(slowMotionDuration);
            EventCenter.Instance.TriggerAction("BattleSkillButtonUI_setBlockSkillCanvasGroup", false);
            // 恢复正常速度
            Time.timeScale = 1f;
        }
        /// <summary>
        /// 更新血条过渡动画
        /// </summary>
        /// <param name="value"></param>
        private void UpdateTransitionSlider(float value)
        {
            if (transitionTweener != null && transitionTweener.IsActive())
            {
                transitionTweener.Kill();
            }

            transitionTweener = transitionSlider.DOValue(HPSlider.value , transitionDuration)
                .SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// 更新血量数字,调用角色渲染体更新血量数字方法
        /// </summary>
        /// <param name="damage">数字</param>
        /// <param name="dameagePrefab">血量数字预制体</param>
        public void UpdateDamageNumber(float damage, GameObject dameagePrefab)
        {
            if (characterRender != null)
            {
                characterRender.UpdateDamageNumber(damage, dameagePrefab);
            }
        }
        #endregion

        #region 更新技能点
        /// <summary>
        /// 更新技能点
        /// </summary>
        /// <param name="currentSkillPoints"></param>
        public void UpdateSkillPoints(int currentSkillPoints)
        {
            if (SkillPoints == null || SkillPoints.Length == 0)
            {
                Debug.LogError("SkillPoints array is not set up properly");
                return;
            }

            for (int i = 0; i < SkillPoints.Length; i++)
            {
                if (SkillPoints[i] != null)
                {
                    // 如果当前索引小于当前技能点数，则显示该技能点
                    SkillPoints[i].SetActive(i <= currentSkillPoints - 1);
                }
            }
        }
        #endregion

        #region 角色头像选中时扩展
        /// <summary>
        /// 扩大图片（向左）
        /// </summary>
        /// <param name="widthIncrease">宽度增加</param>
        /// <param name="heightIncrease">高度增加</param>
        public void ExpandImageLeft(float widthIncrease, float heightIncrease)
        {
            if (characterIcon == null)
            {
                Debug.LogError("Character icon is not set");
                return;
            }

            RectTransform rectTransform = characterIcon.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("RectTransform component not found on character icon");
                return;
            }

            // 如果是第一次调用，保存原始尺寸和位置
            if (!isOriginalSizeStored)
            {
                originalSize = rectTransform.sizeDelta;
                originalPosition = rectTransform.anchoredPosition;
                isOriginalSizeStored = true;
            }

            // 获取当前尺寸
            Vector2 currentSize = rectTransform.sizeDelta;

            // 计算新的尺寸
            float newWidth = currentSize.x + widthIncrease;
            float newHeight = currentSize.y + heightIncrease;

            // 设置新的尺寸
            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

            // 调整锚点位置，使图片向左扩展
            rectTransform.anchoredPosition = new Vector2(
                rectTransform.anchoredPosition.x - widthIncrease / 2,
                rectTransform.anchoredPosition.y
            );
        }

        /// <summary>
        /// 恢复图片原始大小
        /// </summary>
        public void RestoreOriginalSize()
        {
            if (characterIcon == null)
            {
                Debug.LogError("Character icon is not set");
                return;
            }

            RectTransform rectTransform = characterIcon.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("RectTransform component not found on character icon");
                return;
            }

            if (!isOriginalSizeStored)
            {
                Debug.LogWarning("Original size was not stored, cannot restore");
                return;
            }

            // 恢复原始尺寸和位置
            rectTransform.sizeDelta = originalSize;
            rectTransform.anchoredPosition = originalPosition;
        }
        #endregion
        public void OnDestroy()
        {
            // 取消并清理异步任务
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // 清理 DOTween 动画
            if (transitionTweener != null)
            {
                transitionTweener.Kill();
                transitionTweener = null;
            }

            // 清理 UI 组件引用
            if (characterRender != null)
            {
                characterRender.chacaterHeadUI = null;
                characterRender = null;
            }

            // 清理技能点数组
            if (SkillPoints != null)
            {
                for (int i = 0; i < SkillPoints.Length; i++)
                {
                    if (SkillPoints[i] != null)
                    {
                        SkillPoints[i] = null;
                    }
                }
                SkillPoints = null;
            }

            // 清理 UI 组件
            if (characterIcon != null)
            {
                characterIcon.sprite = null;
                characterIcon = null;
            }
            if (UlitmateIcon != null)
            {
                UlitmateIcon.sprite = null;
                UlitmateIcon = null;
            }
            if (HPSlider != null)
            {
                HPSlider.onValueChanged.RemoveAllListeners();
                HPSlider = null;
            }
            if (transitionSlider != null)
            {
                transitionSlider.onValueChanged.RemoveAllListeners();
                transitionSlider = null;
            }

            // 清理原始尺寸和位置数据
            originalSize = Vector2.zero;
            originalPosition = Vector2.zero;
            isOriginalSizeStored = false;
        }
    }
}