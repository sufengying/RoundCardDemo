using UnityEngine;
using DG.Tweening;

public class DissolveController : MonoBehaviour
{
    public Material originalMaterial;  // 原始材质
    public Material dissolveMaterial;  // 消融材质
    public Texture2D noiseTexture;     // 噪声纹理
    public float dissolveDuration = 2f; // 消融动画持续时间

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private bool isDissolving = false;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            // 保存原始材质
            originalMaterials = skinnedMeshRenderer.materials;
            
            // 创建消融材质数组
            dissolveMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // 为每个材质创建一个新的消融材质实例
                dissolveMaterials[i] = new Material(dissolveMaterial);
                
                // 复制原始材质的所有属性
                CopyMaterialProperties(originalMaterials[i], dissolveMaterials[i]);
            }
        }
    }

    // 复制材质属性
    private void CopyMaterialProperties(Material source, Material target)
    {
        // 复制主纹理
        if (source.HasProperty("_MainTex"))
        {
            target.SetTexture("_MainTex", source.GetTexture("_MainTex"));
        }

        // 复制法线贴图
        if (source.HasProperty("_BumpMap"))
        {
            target.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            if (source.HasProperty("_BumpScale"))
            {
                target.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            }
        }

        // 复制颜色
        if (source.HasProperty("_Color"))
        {
            target.SetColor("_Color", source.GetColor("_Color"));
        }

        // 复制高光颜色
        if (source.HasProperty("_SpecColor"))
        {
            target.SetColor("_SpecColor", source.GetColor("_SpecColor"));
        }

        // 复制高光强度
        if (source.HasProperty("_Shininess"))
        {
            target.SetFloat("_Shininess", source.GetFloat("_Shininess"));
        }

        // 复制其他常用属性
        string[] commonProperties = new string[] {
            "_Glossiness",
            "_Metallic",
            "_EmissionColor",
            "_EmissionMap",
            "_OcclusionMap",
            "_DetailMask",
            "_DetailAlbedoMap",
            "_DetailNormalMap"
        };

        foreach (string prop in commonProperties)
        {
            if (source.HasProperty(prop))
            {
                if (prop.EndsWith("Map"))
                {
                    target.SetTexture(prop, source.GetTexture(prop));
                }
                else if (prop.EndsWith("Color"))
                {
                    target.SetColor(prop, source.GetColor(prop));
                }
                else
                {
                    target.SetFloat(prop, source.GetFloat(prop));
                }
            }
        }
    }

    // 开始消融效果
    public void StartDissolve()
    {
        if (isDissolving || skinnedMeshRenderer == null) return;
        
        isDissolving = true;

        // 设置每个消融材质的属性
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            // 确保噪声纹理被正确设置
            dissolveMaterials[i].SetTexture("_NoiseMap", noiseTexture);
            
            // 设置初始消融阈值
            dissolveMaterials[i].SetFloat("_DissolveThreshold", 0);
        }

        // 应用消融材质
        skinnedMeshRenderer.materials = dissolveMaterials;

        // 使用DOTween创建消融动画
        DOTween.To(() => 0f,
            x => {
                // 更新所有材质的消融阈值
                foreach (Material mat in dissolveMaterials)
                {
                    mat.SetFloat("_DissolveThreshold", x);
                }
            },
            1f, dissolveDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                // 消融完成后可以在这里添加其他逻辑
                // 比如销毁物体
                // Destroy(gameObject);
            });
    }

    // 重置消融效果
    public void ResetDissolve()
    {
        if (skinnedMeshRenderer == null) return;
        
        isDissolving = false;
        skinnedMeshRenderer.materials = originalMaterials;
    }

    void OnDestroy()
    {
        // 清理创建的材质实例
        if (dissolveMaterials != null)
        {
            foreach (Material mat in dissolveMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }

    void Update()
    {
        // 按D键触发消融效果
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartDissolve();
        }
        
        // 按R键重置效果
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetDissolve();
        }
    }
} 