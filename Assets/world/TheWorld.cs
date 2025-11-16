using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
class TheWorld : PostEffectsBase
{
    private static TheWorld instance;
    public static TheWorld Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<TheWorld>();
                if (instance == null)
                {
                    Debug.LogError("TheWorld component not found in scene!");
                }
            }
            return instance;
        }
    }

    public Vector2 center = new Vector2(0, 0f);  // 屏幕中心
    public Vector2 distortionCenter = new Vector2(0.5f, 0.5f);  // 畸变中心
    [Range(0, 2)] public float radius = 0f;           // 初始半径为0
    public Shader pincushionShader = null;
    private Material pincushionMaterial = null;
    public Texture2D testImage = null;                // 测试图片
    public Texture2D maskTexture = null;              // 遮罩纹理
    RenderTexture mySource;                          // 渲染纹理

    [Header("Distortion Settings")]
    [Range(0f, 1f)] public float distortionStrength = 0.1f;
    [Range(0f, 20.0f)] public float fov = 1.0f;
    public float aspectRatio = 1.777777f;
    public Color colorTint = new Color(1, 0, 0, 1);
    [Range(0f, 1f)] public float colorIntensity = 0.5f;
    [Range(0f, 20.0f)] public float fovThreshold = 1.5f;

    [Header("Mask Settings")]
    [Range(0.01f, 5.0f)] public float maskScale = 1.0f;        // 增大最大缩放范围
    [Range(0f, 1f)] public float maskSoftness = 0.1f;
    [Range(0.1f, 3.0f)] public float maskTransition = 0.5f;   // 增大过渡范围

    public Camera mainCamera;
    private bool isTimeStopped = false;
    private Tween fovTween;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }

        if (testImage)
        {
            mySource = new RenderTexture(testImage.width / 2, testImage.height / 2, 0);
            RenderTexture.active = mySource;
            Graphics.Blit(testImage, mySource);
        }
    }

    void Update()
    {
        // // 在编辑模式和运行时都能响应按键
        // if (Input.GetKeyDown(KeyCode.U))
        // {
        //     Debug.Log("Time Stop Triggered");
        //     ExecuteTimeStop(4f);
        // }
    }

    /// <summary>
    /// 执行时间暂停效果
    /// </summary>
    /// <param name="duration">FOV动画持续时间</param>
    /// <param name="delay">延迟执行时间</param>
    /// <param name="actionStart">开始时的回调</param>
    /// <param name="actionEnd">结束时的回调</param>
    public void ExecuteTimeStop(float duration, float delay = 0f, Action actionStart=null, Action actionEnd=null)
    {
        if (isTimeStopped) return;

        if (delay > 0)
        {
            // 使用DOTween创建延迟
            DOVirtual.DelayedCall(delay, () => {
                ExecuteTimeStopInternal(duration, actionStart, actionEnd);
            }, false);
        }
        else
        {
            ExecuteTimeStopInternal(duration, actionStart, actionEnd);
        }
    }

    private void ExecuteTimeStopInternal(float duration, Action actionStart, Action actionEnd)
    {
        actionStart?.Invoke();
        if (isTimeStopped) return;

       

        // 暂停时间
        Time.timeScale = 0f;
        isTimeStopped = true;

        // 捕获当前画面
        if (mainCamera != null)
        {
       
            // 创建临时渲染纹理
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = rt;
            mainCamera.Render();
            mainCamera.targetTexture = null;

            // 创建新的纹理
            if (testImage != null)
            {
                if (Application.isPlaying)
                    Destroy(testImage);
                else
                    DestroyImmediate(testImage);
            }
            testImage = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            
            // 读取渲染纹理到纹理2D
            RenderTexture.active = rt;
            testImage.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            testImage.Apply();
            RenderTexture.active = null;

            // 清理临时渲染纹理
            if (Application.isPlaying)
                Destroy(rt);
            else
                DestroyImmediate(rt);

            // 创建新的渲染纹理用于效果
            mySource = new RenderTexture(testImage.width / 2, testImage.height / 2, 0);
            RenderTexture.active = mySource;
            Graphics.Blit(testImage, mySource);
        }

        // 执行FOV动画
        if (fovTween != null)
        {
            fovTween.Kill();
        }

      

        // 创建FOV动画
        fovTween = DOTween.To(() => fov, x => fov = x, 20f, duration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() => {
                Time.timeScale = 1f;
                isTimeStopped = false;
                actionEnd?.Invoke();
                
            });
    }

    public void ResetFov()
    {
        testImage=null;
        fov = 0f;
        GameNode.Instance.mainCamera.depth=-1;
    }

    

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // 设置畸变材质参数
        pincushionMaterial.SetFloat("_DistortionStrength", distortionStrength);
        pincushionMaterial.SetFloat("_FOV", fov);
        pincushionMaterial.SetFloat("_AspectRatio", aspectRatio);
        pincushionMaterial.SetVector("_Center", distortionCenter);
        pincushionMaterial.SetColor("_ColorTint", colorTint);
        pincushionMaterial.SetFloat("_ColorIntensity", colorIntensity);
        pincushionMaterial.SetFloat("_FOVThreshold", fovThreshold);
        
        // 设置遮罩参数
        pincushionMaterial.SetFloat("_MaskScale", maskScale);
        pincushionMaterial.SetFloat("_MaskSoftness", maskSoftness);
        pincushionMaterial.SetFloat("_MaskTransition", maskTransition);
        
        // 设置遮罩纹理
        if (maskTexture != null)
        {
            pincushionMaterial.SetTexture("_MaskTex", maskTexture);
        }

        // 直接应用效果
        if (testImage)
        {
            Graphics.Blit(mySource, destination, pincushionMaterial);
        }
        else
        {
            Graphics.Blit(source, destination, pincushionMaterial);
        }
    }

    void OnDestroy()
    {
        // 清理资源
        if (testImage != null)
        {
            if (Application.isPlaying)
                Destroy(testImage);
            else
                DestroyImmediate(testImage);
        }
        if (mySource != null)
        {
            mySource.Release();
        }
        if (fovTween != null)
        {
            fovTween.Kill();
        }
    }

    public override bool CheckResources()
    {
        CheckSupport(true);
        pincushionMaterial = CheckShaderAndCreateMaterial(pincushionShader, pincushionMaterial);
        if (!isSupported)
            ReportAutoDisable();
        return isSupported;
    }
}
