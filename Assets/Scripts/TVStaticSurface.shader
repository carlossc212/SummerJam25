Shader "Custom/TVStaticSurface"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _StaticColor ("Static Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _StaticIntensity ("Static Intensity", Range(0, 1)) = 0.2
        _StaticSpeed ("Static Speed", Range(0.1, 10)) = 3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        fixed4 _BaseColor;
        fixed4 _StaticColor;
        float _Glossiness;
        float _Metallic;
        float _StaticIntensity;
        float _StaticSpeed;

        struct Input
        {
            float2 uv_MainTex;
        };

        float random(float2 st)
        {
            return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;

            // Textura base + color
            fixed4 baseTex = tex2D(_MainTex, uv) * _BaseColor;

            // Ruido animado
            float noise = random(float2(uv.x, uv.y + _Time.y * _StaticSpeed));
            float3 staticEffect = _StaticColor.rgb * noise;

            // Combinar base y ruido
            float3 finalColor = lerp(baseTex.rgb, staticEffect, _StaticIntensity);

            o.Albedo = finalColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
