Shader "Custom/FrozenWater"
{
    Properties
    {
        _IceAlbedo("Ice Albedo", 2D) = "white" {}
        _IceNormal("Ice Normal", 2D) = "bump" {}
        _FlowMap("Flow Map", 2D) = "black" {}
        _IceCracks("Ice Cracks", 2D) = "white" {}
        _FogMap("Fog Map", 2D) = "white" {}

        _IceTint("Ice Tint", Color) = (0.8, 0.9, 1.0, 1.0)
        _WaterDepthColor("Water Depth Color", Color) = (0.2, 0.4, 0.6, 1.0)
        _SubsurfaceColor("Subsurface Color", Color) = (0.3, 0.6, 0.9, 1)
        _SubsurfaceStrength("Subsurface Strength", Range(0, 1)) = 0.5
        _SubsurfaceFalloff("Subsurface Falloff", Float) = 2.0

        _NormalStrength("Normal Strength", Float) = 1
        _AnisoStrength("Anisotropic Highlight Strength", Range(0,1)) = 0.2

        _FlowStrength("Flow Strength", Range(0,1)) = 0.1
        _DistortionStrength("Distortion Strength", Range(0,1)) = 0.05
        _Glossiness("Glossiness", Range(0,1)) = 0.5
        _FlowSpeed("Flow Speed", Range(0,5)) = 1.0

        _CracksStrength("Ice Cracks Strength", Range(0,1)) = 1.0
        _CrackRefractionStrength("Crack Refraction Strength", Range(0,1)) = 0.1

        _FogStrength("Fog Strength", Range(0,1)) = 0.5
        _Reflectivity("Reflectivity", Range(0,1)) = 0.5

        _TessellationEdge("Tessellation Edge", Range(1,64)) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf StandardSpecular fullforwardshadows vertex:vert tessellate:tessEdge
        #pragma target 5.0

        sampler2D _IceAlbedo;
        sampler2D _IceNormal;
        sampler2D _FlowMap;
        sampler2D _IceCracks;
        sampler2D _FogMap;

        float4 _IceTint;
        float4 _WaterDepthColor;
        float4 _SubsurfaceColor;
        float _SubsurfaceStrength;
        float _SubsurfaceFalloff;

        float _NormalStrength;
        float _AnisoStrength;
        float _FlowStrength;
        float _DistortionStrength;
        float _Glossiness;
        float _FlowSpeed;
        float _CracksStrength;
        float _CrackRefractionStrength;
        float _FogStrength;
        float _Reflectivity;
        float _TessellationEdge;

        struct Input
        {
            float2 uv_IceAlbedo;
            float2 uv_IceNormal;
            float2 uv_FlowMap;
            float2 uv_IceCracks;
            float2 uv_FogMap;
            float3 worldPos;
            float3 viewDir;
        };

        void vert(inout appdata_full v) {}

        float tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return _TessellationEdge;
        }

        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float2 flow = tex2D(_FlowMap, IN.uv_FlowMap).rgb * 2 - 1;
            float2 animatedUV = IN.uv_IceAlbedo + flow * _FlowStrength * (_Time.y * _FlowSpeed);

            fixed4 baseAlbedo = tex2D(_IceAlbedo, animatedUV) * _IceTint;
            fixed3 baseNormal = UnpackNormal(tex2D(_IceNormal, animatedUV));
            baseNormal = normalize(lerp(float3(0, 0, 1), baseNormal, _NormalStrength));

            // Refracted UVs for cracks
            float2 refractUV = IN.uv_IceCracks + baseNormal.xy * _CrackRefractionStrength;
            fixed4 cracksRefracted = tex2D(_IceCracks, refractUV);
            float cracksMask = cracksRefracted.r + cracksRefracted.g + cracksRefracted.b;

            // Blend refracted cracks into subsurface
            baseAlbedo.rgb = lerp(baseAlbedo.rgb, cracksRefracted.rgb, cracksMask * _CracksStrength * 0.5);

            float2 fogUV = IN.uv_FogMap + flow * _FlowStrength * (_Time.y * _FlowSpeed);
            fixed fogSample = tex2D(_FogMap, fogUV).r;
            fixed4 fogColor = lerp(fixed4(0,0,0,0), fixed4(0.8,0.85,0.9,1), fogSample * _FogStrength);
            baseAlbedo.rgb = lerp(baseAlbedo.rgb, fogColor.rgb, fogColor.a);

            // Fake subsurface (based on view angle)
            float3 V = normalize(IN.viewDir);
            float NdotV = saturate(dot(baseNormal, V));
            float subsurfaceMask = pow(1.0 - NdotV, _SubsurfaceFalloff);
            baseAlbedo.rgb = lerp(baseAlbedo.rgb, _SubsurfaceColor.rgb, subsurfaceMask * _SubsurfaceStrength);

            o.Albedo = lerp(_WaterDepthColor.rgb, baseAlbedo.rgb, 0.8);
            o.Normal = baseNormal;

            float aniso = pow(1.0 - NdotV, 3.0);
            float anisoSpec = aniso * _AnisoStrength;

            float reflectControl = saturate((1.0 - cracksMask) * _Reflectivity);
            o.Smoothness = saturate(_Glossiness + reflectControl + anisoSpec);
            o.Specular = saturate(_Glossiness + reflectControl + anisoSpec);

            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
