Shader "Hidden/Custom/Snow"
{
    Properties
    {
        _MainTex("-", 2D) = "" {}
    }
    
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    float4 _MainTex_TexelSize;

    // Snow Glitch Properties
    float _SnowIntensity;
    float _SnowSize;
    float _SnowDensity;
    float _FlickerSpeed;
    float _StaticIntensity;

    // 随机数生成
    float nrand(float2 uv)
    {
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // 雪花生成函数
    float snow(float2 uv, float time)
    {
        // 创建网格
        float2 grid = float2(_SnowSize, _SnowSize * _ScreenParams.x / _ScreenParams.y);
        float2 pos = uv * grid;
        float2 cell = floor(pos);
        float2 local = frac(pos);
        
        // 单元格随机值
        float random = nrand(cell + time);
        
        // 雪花中心点
        float2 center = 0.5 + 0.4 * sin(random * 6.2831 + float2(0, 1.57));
        float dist = length(local - center);
        float snowflake = 1.0 - smoothstep(0.0, 0.2, dist);
        
        // 密度控制
        snowflake *= step(random, _SnowDensity);
        
        return snowflake;
    }

    // 静态噪声
    float staticNoise(float2 uv, float time)
    {
        float2 flickerUV = uv + time * _FlickerSpeed * 0.1;
        float staticValue = nrand(flickerUV);
        return staticValue * _StaticIntensity;
    }

    Varyings vert(Attributes input)
    {
        Varyings output;
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        output.uv = input.uv;
        return output;
    }

    half4 frag(Varyings input) : SV_Target
    {
        float2 uv = input.uv;
        float time = _Time.y;
        
        // 原始颜色
        half4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        
        // 生成雪花
        float snowValue = 0.0;
        for (int layer = 0; layer < 3; layer++)
        {
            float layerScale = pow(2.0, layer);
            float layerTime = time * (0.5 + layer * 0.3);
            snowValue += snow(uv * layerScale, layerTime) * (1.0 / layerScale);
        }
        snowValue = saturate(snowValue * _SnowIntensity);
        
        // 生成静态噪声
        float staticValue = staticNoise(uv, time);
        
        // 组合效果
        half3 finalColor = originalColor.rgb;
        
        // 雪花效果 - 在雪花区域显示为白色/灰色
        if (snowValue > 0.1)
        {
            float brightness = 0.7 + 0.3 * nrand(uv + time);
            finalColor = lerp(originalColor.rgb, half3(brightness, brightness, brightness), snowValue);
        }
        
        // 添加静态噪声
        finalColor += staticValue * 0.5;
        
        // 闪烁效果
        float flicker = 0.8 + 0.2 * sin(time * _FlickerSpeed);
        finalColor *= flicker;
        
        // 颜色偏移
        float colorShift = sin(time * 10.0) * 0.02 * _StaticIntensity;
        half4 shiftedColor;
        shiftedColor.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(colorShift, 0)).r;
        shiftedColor.g = finalColor.g;
        shiftedColor.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(colorShift, 0)).b;
        shiftedColor.a = originalColor.a;
        
        // 最终雪花覆盖
        shiftedColor.rgb = lerp(shiftedColor.rgb, half3(1,1,1), snowValue * 0.3);
        
        return shiftedColor;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "Snow Glitch"
            ZTest Always
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}