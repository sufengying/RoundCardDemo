using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Threading;


namespace ZMGCFrameWork.Battle
{
    public class EnemyHUD : MonoBehaviour
    {
        private CharacterRender characterRender;
        public Slider hpSlider;
        public Slider transitionSlider;
     
        public CanvasGroup canvasGroup;
        private float transitionDuration = 0.5f; // 过渡动画持续时间
        private Tweener transitionTweener; // 用于存储当前的过渡动画
        private CancellationTokenSource _cancellationTokenSource;//取消令牌

        public void Initialize(CharacterRender characterRender)
        {
            this.characterRender = characterRender;
            // 初始化时设置两个slider的值相同
            hpSlider.value = 1f;
            transitionSlider.value = 1f;
            canvasGroup=GetComponent<CanvasGroup>();
            UpdateHPSlider(new int[] { 0 }, new float[] { 0f }, null, null, false);
        }

        #region 更新血条与跳出血量数字
        /// <summary>
        /// 更新血条,血条直接跳转到对应的值
        /// </summary>
        /// <param name="attackPreDelay">攻击前摇数组</param>
        /// <param name="value">血量</param>
        /// <param name="skillDamageArray">伤害数字数组</param>
        /// <param name="dameagePrefab">血量数字预制体</param>
        public async void UpdateHPSlider(int[] attackPreDelay, float[] value, float[] skillDamageArray = null, GameObject dameagePrefab = null, bool isPlayHitEffect = true)
        {
            if(hpSlider.value<=0)
            {
                return;
            }
            // 创建新的取消令牌
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // 如果存在正在进行的过渡动画，先停止它
            if (transitionTweener != null && transitionTweener.IsActive())
            {
                transitionTweener.Kill();
            }

            for (int i = 0; i < value.Length; i++)
            {
                await Task.Delay(attackPreDelay[i], token);
                
                if (isPlayHitEffect)
                {
                    characterRender.OnEnterHit();
                    characterRender.OnEnterAnimation("hit");
                }

                if (skillDamageArray != null && dameagePrefab != null)
                {
                    UpdateDamageNumber(skillDamageArray[i], dameagePrefab);
                }
                // 先更新红色血条
                hpSlider.value -= value[i];
                // 调用过渡动画
                UpdateTransitionSlider(value[i]);
                if(hpSlider.value<=0)
                {
                    canvasGroup.alpha = 0;
                }else
                {
                    canvasGroup.alpha = 1;
                }
            }
        }
        /// <summary>
        /// 更新过渡条动画,过渡条是平滑的
        /// </summary>
        /// <param name="value">血量</param>
        public void UpdateTransitionSlider(float value)
        {
            // 如果存在正在进行的过渡动画，先停止它
            if (transitionTweener != null && transitionTweener.IsActive())
            {
                transitionTweener.Kill();
            }

            // 创建新的过渡动画
            transitionTweener = transitionSlider.DOValue(hpSlider.value, transitionDuration)
                .SetEase(Ease.OutQuad);
        }
        /// <summary>
        /// 更新血量数字
        /// </summary>
        /// <param name="damage">伤害数字</param>
        /// <param name="dameagePrefab">血量数字预制体</param>
        public void UpdateDamageNumber(float damage, GameObject dameagePrefab)
        {
            if (characterRender != null)
            {
                characterRender.UpdateDamageNumber(damage, dameagePrefab);
            }
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

            // 清理角色渲染体引用
            if (characterRender != null)
            {
                characterRender.enemyHUD = null;
                characterRender = null;
            }

            // 清理 UI 组件
            if (hpSlider != null)
            {
                hpSlider.onValueChanged.RemoveAllListeners();
                hpSlider = null;
            }
            if (transitionSlider != null)
            {
                transitionSlider.onValueChanged.RemoveAllListeners();
                transitionSlider = null;
            }


        }

    }
}