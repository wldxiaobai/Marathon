Shader "Custom/RiverFlow"
{
    Properties
    {
        _MainTex ("River Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Range(0.1, 5.0)) = 1.0
        _FlowIntensity ("Flow Intensity (U Offset)", Range(0.0, 2.0)) = 1.0
        _NormalMap ("Normal Map (Optional)", 2D) = "bump" {}
        _Distortion ("Distortion Strength", Range(0.0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off ZWrite Off ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            float _FlowSpeed;
            float _FlowIntensity;
            float _Distortion;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 使用两个时刻的采样以避免纹理接缝处的色块
                float time1 = _Time.y * _FlowSpeed * _FlowIntensity;
                float time2 = frac(time1 + 0.5);  // 延迟0.5个周期
                time1 = frac(time1);

                // 采样法线贴图用于扰动效果
                float2 distortedUV = i.uv;
                if (_Distortion > 0.0)
                {
                    float2 normalUV = TRANSFORM_TEX(i.uv, _NormalMap);
                    float4 normalSample = tex2D(_NormalMap, normalUV + float2(_Time.y * _FlowSpeed * 0.3, 0));
                    // 使用法线的R通道作为扰动
                    distortedUV.x += (normalSample.r - 0.5) * _Distortion;
                }

                // 确保UV在0-1范围内
                distortedUV = frac(distortedUV);

                // 采样两个不同时刻的流动纹理
                float2 flowUV1 = float2(frac(distortedUV.x + time1), distortedUV.y);
                float2 flowUV2 = float2(frac(distortedUV.x + time2), distortedUV.y);

                fixed4 col1 = tex2D(_MainTex, flowUV1);
                fixed4 col2 = tex2D(_MainTex, flowUV2);

                // 平滑混合因子：使用正弦波实现更平滑的过渡
                float blendFactor = 0.5 + 0.5 * sin(time1 * 6.28318);  // 2π
                fixed4 col = lerp(col1, col2, blendFactor);

                // 保留原始Alpha通道，防止透明底转白
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
