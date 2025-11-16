Shader "Custom/PincushionDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _FOV ("Field of View", Range(0.1, 100.0)) = 1.0
        _AspectRatio ("Aspect Ratio", Float) = 1.777777
        _Center ("Distortion Center", Vector) = (0.5, 0.5, 0, 0)
        _ColorTint ("Color Tint", Color) = (1,0,0,1)
        _ColorIntensity ("Color Intensity", Range(0, 1)) = 0.5
        _FOVThreshold ("FOV Threshold", Range(0.1, 2.0)) = 1.5
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskScale ("Mask Scale", Range(0.01, 5.0)) = 1.0
        _MaskSoftness ("Mask Softness", Range(0, 1)) = 0.1
        _MaskTransition ("Mask Transition", Range(0.1, 3.0)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DistortionStrength;
            float _FOV;
            float _AspectRatio;
            float2 _Center;
            float4 _ColorTint;
            float _ColorIntensity;
            float _FOVThreshold;
            sampler2D _MaskTex;
            float _MaskScale;
            float _MaskSoftness;
            float _MaskTransition;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算UV坐标和偏移量
                float2 UV_normalized = i.uv - 0.5;
                
                // 应用宽高比
                float2 uv = float2(UV_normalized.x * _AspectRatio, UV_normalized.y);
                
                // 计算到中心的距离
                float dist = length(uv);
                
                // 计算FOV和畸变强度
                float fovFactor = 1.0 / (_DistortionStrength * 0.1 + 1.0);
                
                // 计算畸变因子
                float dist_factor = dist * _FOV * fovFactor;
                
                // 应用畸变
                float2 UV_distorted = UV_normalized / (1.0 + dist_factor);
                float2 UV_final = UV_distorted + _Center;
                
                // 采样纹理
                fixed4 col = tex2D(_MainTex, UV_final);
                
                // 计算颜色混合因子
                float colorBlend = saturate((_FOV - 0.1) / 1.9);
                colorBlend = pow(colorBlend, 2.0);
                
                // 混合原始颜色和色调
                float3 tintedColor = lerp(col.rgb, _ColorTint.rgb, colorBlend * _ColorIntensity);
                
                // 计算遮罩效果
                if (_FOV > _FOVThreshold)
                {
                    // 计算遮罩缩放因子
                    float scaleFactor = smoothstep(_FOVThreshold, _FOVThreshold + _MaskTransition, _FOV);
                    float currentScale = lerp(0.01, _MaskScale, scaleFactor);
                    
                    // 计算遮罩UV，考虑缩放
                    float2 maskUV = (i.uv - _Center) / currentScale + 0.5;
                    
                    // 检查UV是否在有效范围内
                    if (maskUV.x >= 0 && maskUV.x <= 1 && maskUV.y >= 0 && maskUV.y <= 1)
                    {
                        // 采样遮罩纹理
                        fixed4 maskColor = tex2D(_MaskTex, maskUV);
                        float maskValue = maskColor.r; // 使用红色通道作为遮罩值
                        
                        // 添加软边缘效果
                        float softEdge = smoothstep(1.0, 1.0 - _MaskSoftness, maskValue);
                        
                        // 如果遮罩值大于阈值，显示黑色（中间部分）
                        if (softEdge > 0.5)
                        {
                            return fixed4(0, 0, 0, 1);
                        }
                    }
                }
                
                // 返回最终颜色，保持原始alpha值
                return fixed4(tintedColor, col.a);
            }
            ENDCG
        }
    }
} 