Shader "Hidden/Custom/TwistGlitch"
{
    Properties
    {
        _MainTex  ("-", 2D) = "" {}
        _NoiseTex ("-", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _NoiseTex;
    
    float _Intensity;
    float _DistortionStrength;
    float _DistortionFrequency;
    float _DistortionSpeed;
    float _VignetteSoftness;
    float _VignetteRadius;
    float _NoiseScale;
    
    // 位置控制参数
    float2 _CenterPosition;
    float _AspectRatio;
    float _RotationAngle;

    // 简单的噪声函数
    float noise(float2 uv)
    {
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }
    
    // 平滑噪声
    float smoothNoise(float2 uv)
    {
        float2 i = floor(uv);
        float2 f = frac(uv);
        
        float a = noise(i);
        float b = noise(i + float2(1.0, 0.0));
        float c = noise(i + float2(0.0, 1.0));
        float d = noise(i + float2(1.0, 1.0));
        
        float2 u = f * f * (3.0 - 2.0 * f);
        
        return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
    }
    
    // 旋转UV坐标
    float2 rotateUV(float2 uv, float2 center, float angle)
    {
        float2 centeredUV = uv - center;
        float s = sin(angle);
        float c = cos(angle);
        float2x2 rotationMatrix = float2x2(c, -s, s, c);
        return mul(rotationMatrix, centeredUV) + center;
    }
    
    // 计算扭曲遮罩
    float TwistGlitchMask(float2 uv)
    {
        // 应用旋转
        float2 rotatedUV = rotateUV(uv, _CenterPosition, _RotationAngle);
        
        // 将UV坐标从[0,1]转换到基于中心位置的坐标
        float2 centeredUV = (rotatedUV - _CenterPosition) * float2(_AspectRatio, 1.0);
        
        // 计算到中心的距离
        float distanceFromCenter = length(centeredUV);
        
        // 反转晕影：中心区域为1，边缘为0
        float vignette = 1.0 - smoothstep(_VignetteRadius - _VignetteSoftness, 
                                         _VignetteRadius + _VignetteSoftness, 
                                         distanceFromCenter);
        
        return vignette;
    }

    // 椭圆遮罩
    float EllipticalMask(float2 uv)
    {
        float2 centeredUV = (uv - _CenterPosition) * float2(_AspectRatio, 1.0);
        float distanceFromCenter = length(centeredUV);
        
        return 1.0 - smoothstep(_VignetteRadius - _VignetteSoftness, 
                               _VignetteRadius + _VignetteSoftness, 
                               distanceFromCenter);
    }

    // 矩形遮罩
    float RectangularMask(float2 uv)
    {
        float2 centeredUV = (uv - _CenterPosition) * float2(_AspectRatio, 1.0);
        float2 absUV = abs(centeredUV);
        
        // 计算矩形边界
        float2 edgeDist = smoothstep(_VignetteRadius - _VignetteSoftness, 
                                   _VignetteRadius + _VignetteSoftness, 
                                   absUV);
        
        return 1.0 - max(edgeDist.x, edgeDist.y);
    }

    float4 frag(v2f_img i) : SV_Target 
    {
        // 计算扭曲遮罩
        float mask = TwistGlitchMask(i.uv);
        
        // 如果没有遮罩（外部区域），直接返回原图
        if (mask < 0.001)
        {
            return tex2D(_MainTex, i.uv);
        }
        
        // 计算扭曲效果
        float2 distortedUV = i.uv;
        
        // 添加基于时间的动画
        float time = _Time.y * _DistortionSpeed;
        
        // 使用噪声创建有机的扭曲效果
        float noise1 = smoothNoise(i.uv * _DistortionFrequency + time) * 2.0 - 1.0;
        float noise2 = smoothNoise(i.uv * _DistortionFrequency * 1.7 - time * 0.7) * 2.0 - 1.0;
        
        // 基于极坐标的扭曲（相对于效果中心）
        float2 effectCenteredUV = (i.uv - _CenterPosition) * float2(_AspectRatio, 1.0);
        float angle = atan2(effectCenteredUV.y, effectCenteredUV.x);
        float radius = length(effectCenteredUV);
        
        // 径向扭曲
        float radialDistortion = sin(radius * _DistortionFrequency * 10.0 + time) * 0.1;
        
        // 角度扭曲
        float angularDistortion = sin(radius * _DistortionFrequency * 8.0 - time * 1.3) * 0.05;
        
        // 向中心点的扭曲
        float2 toCenter = normalize(_CenterPosition - i.uv);
        float centerPull = smoothstep(0.0, _VignetteRadius, radius) * 0.1;
        
        // 组合扭曲效果
        float2 distortionOffset = float2(
            noise1 * _NoiseScale + radialDistortion * mask + toCenter.x * centerPull,
            noise2 * _NoiseScale + angularDistortion * mask + toCenter.y * centerPull
        );
        
        // 应用扭曲，强度受遮罩和全局强度影响
        distortedUV += distortionOffset * _DistortionStrength * mask * _Intensity;
        
        // 采样扭曲后的纹理
        float4 color = tex2D(_MainTex, distortedUV);
        
        return color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}