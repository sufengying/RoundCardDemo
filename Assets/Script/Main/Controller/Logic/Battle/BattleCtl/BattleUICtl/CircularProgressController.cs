using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ZMGCFrameWork.Battle
{
    [RequireComponent(typeof(Image))]
    public class CircularProgressController : MonoBehaviour
    {
        private Image progressImage;
        private Material progressMaterial;

        [Header("Progress Settings")]
        [Range(0, 1)] public float progress = 0f;
        [Range(0, 0.5f)] public float width = 0.1f;
        [Range(0, 360)] public float startAngle = 0f;
        public bool clockwise = true;

        [Header("Animation Settings")]
        public float animationDuration = 1f;
        private float targetProgress = 0f;
        private bool isAnimating = false;

        private Tween progressTween;
        private System.Action onComplete;

        private void Awake()
        {
            progressImage = GetComponent<Image>();

            // 创建材质实例
            progressMaterial = new Material(Shader.Find("Custom/UI/CircularProgressUI"));
            progressImage.material = progressMaterial;

            // 设置初始属性
            UpdateMaterialProperties();
        }

        private void Update()
        {
            if (isAnimating)
            {
                // 使用 Lerp 平滑过渡
                progress = Mathf.Lerp(progress, targetProgress, Time.deltaTime / animationDuration);
                UpdateMaterialProperties();

                // 当接近目标值时停止动画
                if (Mathf.Abs(progress - targetProgress) < 0.001f)
                {
                    progress = targetProgress;
                    UpdateMaterialProperties();
                    isAnimating = false;
                }
            }
        }

        private void UpdateMaterialProperties()
        {
            if (progressMaterial != null)
            {
                progressMaterial.SetFloat("_Progress", progress);
                progressMaterial.SetFloat("_Width", width);
                progressMaterial.SetFloat("_StartAngle", startAngle);
                progressMaterial.SetFloat("_Clockwise", clockwise ? 1f : 0f);
            }
        }

        // 直接设置进度
        public void SetProgress(float newProgress)
        {
            progress = Mathf.Clamp01(newProgress);
            UpdateMaterialProperties();
        }

        // 使用动画设置进度
        public void SetProgressWithAnimation(float newProgress)
        {
            targetProgress = Mathf.Clamp01(newProgress);
            isAnimating = true;
        }

        // 在编辑器中预览效果
        private void OnValidate()
        {
            if (progressMaterial != null)
            {
                UpdateMaterialProperties();
            }
        }



        /// <summary>
        /// 在指定时间内完成进度条
        /// </summary>
        /// <param name="duration">完成时间（秒）</param>
        public void CompleteInDuration(float duration)
        {
            if (duration <= 0)
            {
                // 如果时间小于等于0，直接设置为100%
                SetProgress(1f);
                return;
            }

            // 设置动画持续时间为指定时间
            animationDuration = duration;
            // 设置目标进度为100%
            SetProgressWithAnimation(1f);
        }

        private void OnDestroy()
        {
            // 清理材质
            if (progressMaterial != null)
            {
                Destroy(progressMaterial);
                progressMaterial = null;
            }

            // 清理UI组件引用
            if (progressImage != null)
            {
                progressImage.material = null;
                progressImage = null;
            }

            // 清理动画
            if (progressTween != null)
            {
                progressTween.Kill();
                progressTween = null;
            }

            // 清理事件监听
            if (onComplete != null)
            {
                onComplete = null;
            }
        }
    }
}
