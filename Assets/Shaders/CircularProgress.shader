Shader "Custom/UI/CircularProgressUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR]_Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Width ("Width", Range(0, 0.5)) = 0.1
        _StartAngle ("Start Angle", Range(0, 360)) = 0
        _Clockwise ("Clockwise", Float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _Progress;
            float _Width;
            float _StartAngle;
            float _Clockwise;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // 计算UV中心点
                float2 center = float2(0.5, 0.5);
                float2 uv = IN.texcoord - center;
                
                // 计算当前点的角度
                float angle = atan2(uv.y, uv.x) * 180 / 3.14159;
                angle = (angle + 360) % 360;
                
                // 调整起始角度
                angle = (angle - _StartAngle + 360) % 360;
                
                // 根据方向调整角度
                if (_Clockwise < 0.5)
                {
                    angle = 360 - angle;
                }
                
                // 计算进度
                float progress = _Progress * 360;
                
                // 计算到中心的距离
                float dist = length(uv);
                
                // 计算环形区域
                float ring = smoothstep(0.5 - _Width, 0.5 - _Width + 0.001, dist) * 
                           smoothstep(0.5 + _Width, 0.5 + _Width - 0.001, dist);
                
                // 计算角度遮罩
                float angleMask = step(angle, progress);
                
                // 合并遮罩
                float mask = ring * angleMask;
                
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) * mask;
                
                return color;
            }
            ENDCG
        }
    }
} 