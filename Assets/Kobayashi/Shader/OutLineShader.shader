Shader "Unlit/OutLineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutLineColor("OutLineColor", Color) = (1,1,1,1)
        _OutLineSize("OutLineSize", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "OUTLINE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _OutLineColor;
            float _OutLineSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float outline = 0.0;

                float2 texelSize = _OutLineSize * float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                //ピクセルを16方向にずらす
                float2 offsets[16] = {
                    float2(1, 0), // 0°
                    float2(0.9239, 0.3827), // 22.5°
                    float2(0.7071, 0.7071), // 45°
                    float2(0.3827, 0.9239), // 67.5°
                    float2(0, 1), // 90°
                    float2(-0.3827, 0.9239), // 112.5°
                    float2(-0.7071, 0.7071), // 135°
                    float2(-0.9239, 0.3827), // 157.5°
                    float2(-1, 0), // 180°
                    float2(-0.9239, -0.3827), // 202.5°
                    float2(-0.7071, -0.7071), // 225°
                    float2(-0.3827, -0.9239), // 247.5°
                    float2(0, -1), // 270°
                    float2(0.3827, -0.9239), // 292.5°
                    float2(0.7071, -0.7071), // 315°
                    float2(0.9239, -0.3827) // 337.5°
                };
                for (int j = 0; j < 16; j++)
                {
                    float2 offsetUV = uv + offsets[j] * texelSize;
                    float sampleAlpha = tex2D(_MainTex, offsetUV).a;

                    // 境界判定：自分が透明で周囲が不透明
                    outline += sampleAlpha;
                }

                outline = saturate(outline); // 0〜1に制限
                if (outline<=0.2f) discard;
                return _OutLineColor * outline;
            }
            ENDCG
        }
    }
}