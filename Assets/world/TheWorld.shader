Shader "Hidden/TheWorld"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
        _Radius("Radius",float) = 0.0
        _CenterX("x",float) = 0.5
        _CenterY("y",float) = 0.5
        _MaskSoftness("Mask Softness", Range(0, 1)) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            float _Radius;
            float _CenterX;
            float _CenterY;
            float _MaskSoftness;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
    
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                // 计算遮罩纹理的UV，考虑屏幕宽高比
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 maskUV = float2(
                    (i.uv.x - 0.5f - _CenterX) * aspect / _Radius + 0.5f,
                    (i.uv.y - 0.5f - _CenterY) / _Radius + 0.5f
                );
                
                // 检查UV是否在有效范围内
                if (maskUV.x < 0 || maskUV.x > 1 || maskUV.y < 0 || maskUV.y > 1)
                {
                    return fixed4(0, 0, 0, 1);
                }
                
                // 采样遮罩纹理
                fixed4 maskColor = tex2D(_MaskTex, maskUV);
                float maskValue = maskColor.r; // 使用红色通道作为遮罩值
                
                // 添加软边缘效果
                float softEdge = smoothstep(1.0, 1.0 - _MaskSoftness, maskValue);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 使用遮罩值来决定是否显示
                return lerp(fixed4(0,0,0,1), col, softEdge);
            }
            ENDCG
        }
    }
}