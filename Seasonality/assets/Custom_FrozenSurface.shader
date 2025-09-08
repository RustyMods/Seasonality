// Frozen Ice Surface Shader
// Solid ice surface for walkable terrain
Shader "Custom/FrozenIceSurface"
{
    Properties
    {
        [Header(Ice Surface Properties)]
        _UVScale("UV Scale", Float) = 0.1
        _IceColorBase("Base Ice Color", Color) = (0.7, 0.8, 0.9, 1)
        _IceColorDeep("Deep Ice Color", Color) = (0.4, 0.5, 0.7, 1)
        _IceColorCracks("Crack Color", Color) = (0.2, 0.3, 0.5, 1)
        _IceColorFrost("Frost Color", Color) = (0.9, 0.95, 1.0, 1)
        
        [Header(Normal Maps)]
        _NormalIce("Ice Surface Normal", 2D) = "bump" {}
        _NormalCracks("Crack Normal", 2D) = "bump" {}
        _NormalFrost("Frost Normal", 2D) = "bump" {}
        _NormalIceScale("Ice Normal Scale", Float) = 1
        _NormalCrackScale("Crack Normal Scale", Float) = 2
        _NormalFrostScale("Frost Normal Scale", Float) = 0.5
        
        [Header(Pattern Animation)]
        _FrostSpeed("Frost Movement Speed", Float) = 0.005
        _CrackAnimSpeed("Crack Animation Speed", Float) = 0.002
        _IceShiftSpeed("Ice Shift Speed", Float) = 0.001
        
        [Header(Depth and Thickness)]
        _DepthFade("Depth Fade Distance", Float) = 3.0
        _DepthPower("Depth Fade Power", Float) = 2.0
        _IceThickness("Ice Thickness Effect", Float) = 0.8
        _UnderIceColor("Under Ice Color", Color) = (0.1, 0.2, 0.3, 1)
        
        [Header(Crack Pattern)]
        _CrackTexture("Crack Pattern", 2D) = "black" {}
        _CrackIntensity("Crack Intensity", Range(0, 1)) = 0.6
        _CrackScale("Crack Scale", Float) = 1.5
        _CrackDepth("Crack Depth", Range(0, 1)) = 0.3
        _CrackSoftness("Crack Edge Softness", Range(0.01, 1)) = 0.07
        
        [Header(Frost Pattern)]
        _FrostTexture("Frost Pattern", 2D) = "white" {}
        _FrostCoverage("Frost Coverage", Range(0, 1)) = 0.4
        _FrostScale("Frost Scale", Float) = 2.0
        _FrostThreshold("Frost Threshold", Range(0, 1)) = 0.92
        _FrostSoftness("Frost Softness", Range(0.01, 1)) = 0.2
        
        [Header(Ice Chunk Variation)]
        _ChunkScale("Ice Chunk Scale", Float) = 0.3
        _ChunkVariation("Chunk Color Variation", Range(0, 1)) = 0.2
        _ChunkBumpiness("Chunk Surface Bumpiness", Range(0, 1)) = 0.1
        
        [Header(Refraction)]
        _RefractionStrength("Refraction Strength", Range(0, 0.5)) = 0.05
        _RefractionDepthAtten("Refraction Depth Attenuation", Float) = 1.0
        _RefractionDistortion("Refraction Distortion", Range(0, 1)) = 0.3
        
        [Header(Specular Properties)]
        _SpecularIce("Ice Specular", Range(0, 1)) = 0.8
        _SpecularCracks("Crack Specular", Range(0, 1)) = 0.513
        _SpecularFrost("Frost Specular", Range(0, 1)) = 0.51
        _SmoothnessIce("Ice Smoothness", Range(0, 1)) = 1.0
        _SmoothnessCracks("Crack Smoothness", Range(0, 1)) = 0.303
        _SmoothnessFrost("Frost Smoothness", Range(0, 1)) = 0.69
        
        [Header(Ambient Effects)]
        _AmbientReflection("Ambient Reflection", Range(0, 1)) = 0.3
        _FresnelPower("Fresnel Power", Range(0.1, 5)) = 1.5
        _FresnelIntensity("Fresnel Intensity", Range(0, 1)) = 0.4
        
        [HideInInspector] __dirty("", Int) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-100" "IgnoreProjector"="True" }
        LOD 200
        
        GrabPass { "_BackgroundTexture" }
        
        CGPROGRAM
        #pragma surface surf StandardSpecular alpha:fade vertex:vert
        #pragma target 3.0
        
        #include "UnityStandardUtils.cginc"
        
        struct Input
        {
            float2 uv_NormalIce;
            float2 uv_NormalCracks;
            float2 uv_NormalFrost;
            float4 screenPos;
            float3 worldPos;
            float3 viewDir;
            float4 color : COLOR;
        };
        
        // Textures
        sampler2D _NormalIce;
        sampler2D _NormalCracks;
        sampler2D _NormalFrost;
        sampler2D _CrackTexture;
        sampler2D _FrostTexture;
        sampler2D _BackgroundTexture;
        sampler2D _CameraDepthTexture;
        
        // Surface Properties
        fixed4 _IceColorBase;
        fixed4 _IceColorDeep;
        fixed4 _IceColorCracks;
        fixed4 _IceColorFrost;
        fixed4 _UnderIceColor;
        float _UVScale;
        
        // Normal Properties
        float _NormalIceScale;
        float _NormalCrackScale;
        float _NormalFrostScale;
        
        // Animation
        float _FrostSpeed;
        float _CrackAnimSpeed;
        float _IceShiftSpeed;
        
        // Depth
        float _DepthFade;
        float _DepthPower;
        float _IceThickness;
        
        // Crack Properties
        float _CrackIntensity;
        float _CrackScale;
        float _CrackDepth;
        float _CrackSoftness;
        
        // Frost Properties
        float _FrostCoverage;
        float _FrostScale;
        float _FrostThreshold;
        float _FrostSoftness;
        
        // Ice Chunk Properties
        float _ChunkScale;
        float _ChunkVariation;
        float _ChunkBumpiness;
        
        // Refraction
        float _RefractionStrength;
        float _RefractionDepthAtten;
        float _RefractionDistortion;
        
        // Specular
        float _SpecularIce;
        float _SpecularCracks;
        float _SpecularFrost;
        float _SmoothnessIce;
        float _SmoothnessCracks;
        float _SmoothnessFrost;
        
        // Ambient
        float _AmbientReflection;
        float _FresnelPower;
        float _FresnelIntensity;
        
        // Optimized noise functions for ice patterns
        float hash(float2 p)
        {
            return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
        }
        
        float noise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            
            float a = hash(i);
            float b = hash(i + float2(1.0, 0.0));
            float c = hash(i + float2(0.0, 1.0));
            float d = hash(i + float2(1.0, 1.0));
            
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }
        
        float voronoi(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            
            float minDist = 1.0;
            for(int y = -1; y <= 1; y++)
            {
                for(int x = -1; x <= 1; x++)
                {
                    float2 neighbor = float2(x, y);
                    float2 pointPos = hash(i + neighbor) * 0.5 + 0.5;
                    float2 diff = neighbor + pointPos - f;
                    minDist = min(minDist, dot(diff, diff));
                }
            }
            return minDist;
        }
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldPos = worldPos;
            
            // Subtle vertex displacement for ice chunk variation
            float chunkNoise = noise(worldPos.xz * _ChunkScale + _Time.y * _IceShiftSpeed);
            float chunkHeight = (chunkNoise - 0.5) * _ChunkBumpiness * 0.1;
            v.vertex.y += chunkHeight;
        }
        
        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Calculate UVs with proper scaling
            float2 worldUV = IN.worldPos.xz * _UVScale;
            float2 iceUV = worldUV + _Time.y * _IceShiftSpeed * float2(0.1, 0.2);
            float2 crackUV = worldUV * _CrackScale + _Time.y * _CrackAnimSpeed * float2(-0.3, 0.5);
            float2 frostUV = worldUV * _FrostScale + _Time.y * _FrostSpeed * float2(0.7, -0.4);
            
            // Sample normal maps
            float3 iceNormal = UnpackScaleNormal(tex2D(_NormalIce, iceUV), _NormalIceScale);
            float3 crackNormal = UnpackScaleNormal(tex2D(_NormalCracks, crackUV), _NormalCrackScale);
            float3 frostNormal = UnpackScaleNormal(tex2D(_NormalFrost, frostUV), _NormalFrostScale);
            
            // Calculate depth
            float4 screenPos = IN.screenPos;
            float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, screenPos));
            float surfaceDepth = screenPos.w;
            float depthDiff = abs(sceneDepth - surfaceDepth);
            
            // Depth-based effects
            float depthFade = saturate(pow(depthDiff / _DepthFade, _DepthPower));
            float thicknessFactor = saturate(depthDiff / _IceThickness);
            
            // Ice chunk variation using Voronoi patterns
            float chunkPattern = voronoi(worldUV * _ChunkScale);
            float chunkVariation = noise(worldUV * _ChunkScale * 0.5 + _Time.y * _IceShiftSpeed);
            
            // Crack calculation
            float crackNoise = tex2D(_CrackTexture, crackUV).r + tex2D(_CrackTexture, crackUV).g + tex2D(_CrackTexture, crackUV).b;
            float crackPattern = voronoi(worldUV * _CrackScale * 0.3);
            float crackMask = smoothstep(_CrackIntensity - _CrackSoftness, _CrackIntensity + _CrackSoftness, 
                                        crackNoise * crackPattern);
            
            // Frost calculation
            float frostNoise = tex2D(_FrostTexture, frostUV).r;
            float frostPattern = noise(worldUV * _FrostScale * 0.5 + _Time.y * _FrostSpeed * 10);
            float frostMask = smoothstep(_FrostThreshold - _FrostSoftness, _FrostThreshold + _FrostSoftness, 
                                        frostNoise * frostPattern * _FrostCoverage);
            
            // Blend normals based on surface features
            float3 baseNormal = iceNormal;
            baseNormal = lerp(baseNormal, crackNormal, crackMask * _CrackDepth);
            baseNormal = lerp(baseNormal, frostNormal, frostMask);
            o.Normal = baseNormal;
            
            // Color blending with chunk variation
            fixed3 baseIceColor = lerp(_IceColorBase.rgb, _IceColorDeep.rgb, thicknessFactor);
            baseIceColor = lerp(baseIceColor, _IceColorDeep.rgb, chunkVariation * _ChunkVariation);
            
            fixed3 crackColor = _IceColorCracks.rgb;
            fixed3 frostColor = _IceColorFrost.rgb;
            
            // Combine all surface features
            fixed3 surfaceColor = baseIceColor;
            surfaceColor = lerp(surfaceColor, crackColor, crackMask);
            surfaceColor = lerp(surfaceColor, frostColor, frostMask);
            
            // Under-ice color effect for depth
            surfaceColor = lerp(_UnderIceColor.rgb, surfaceColor, thicknessFactor);
            
            // Refraction effect (subtle for solid ice)
            float2 refractionUV = (screenPos.xy / screenPos.w) + 
                                 baseNormal.xy * _RefractionStrength * _RefractionDistortion * 
                                 saturate(depthDiff * _RefractionDepthAtten);
            fixed3 refraction = tex2D(_BackgroundTexture, refractionUV).rgb;
            
            // Fresnel effect
            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _FresnelPower);
            
            // Blend refraction with surface color
            o.Albedo = lerp(surfaceColor, refraction, fresnel * _FresnelIntensity * 0.1);
            
            // Add ambient reflection
            o.Albedo += _AmbientReflection * fresnel * 0.2;
            
            // Specular properties based on surface features
            float specular = _SpecularIce;
            specular = lerp(specular, _SpecularCracks, crackMask);
            specular = lerp(specular, _SpecularFrost, frostMask);
            o.Specular = specular;
            
            float smoothness = _SmoothnessIce;
            smoothness = lerp(smoothness, _SmoothnessCracks, crackMask);
            smoothness = lerp(smoothness, _SmoothnessFrost, frostMask);
            o.Smoothness = smoothness;
            
            // Alpha - solid ice should be mostly opaque
            o.Alpha = saturate(0.8 + thicknessFactor * 0.2);
        }
        
        ENDCG
    }
    
    FallBack "Transparent/Diffuse"
}