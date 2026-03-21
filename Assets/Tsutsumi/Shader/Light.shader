Shader "Unlit/Light"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineWidth ("Shine Width", Range(0.01, 1)) = 0.1
        _ShineSpeed ("Shine Speed", Range(0.1, 10)) = 1
        _ShineInterval ("Shine Interval", Float) = 2 
        _ShineAngle ("Shine Angle", Range(-90,90)) = 0
        _ShineIntensity ("Shine Intensity", Range(0,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShineColor;
            float _ShineWidth;
            float _ShineSpeed;
            float _ShineInterval;
            float _ShineAngle;
            float _ShineCount; // 追加
            float _ShineIntensity;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float angle = radians(_ShineAngle);
                float2 center = float2(0.5, 0.5);
                float2 uv_rot;
                uv_rot.x = cos(angle) * (uv.x - center.x) - sin(angle) * (uv.y - center.y) + center.x;
                uv_rot.y = sin(angle) * (uv.x - center.x) + cos(angle) * (uv.y - center.y) + center.y;
                float cycleTime = fmod(_Time.y, _ShineInterval);
                float shineDuration = _ShineInterval;
                float shine = 0.0;
                float shinePos = cycleTime * _ShineSpeed - _ShineWidth;
                if ((shinePos - _ShineWidth) < 1.0)
                {
                    shine += smoothstep(shinePos - _ShineWidth, shinePos, uv_rot.x) *
                                            (1.0 - smoothstep(shinePos, shinePos + _ShineWidth, uv_rot.x));
                }
                shine = saturate(shine) * _ShineIntensity;
                fixed4 texColor = tex2D(_MainTex, uv);
                fixed4 finalColor = texColor + _ShineColor * shine;
                finalColor.a = texColor.a;
                return finalColor;
            }
            ENDCG
        }
    }
}

