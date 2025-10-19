using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom Effects/Twist Glitch")]
public class TwistGlitchVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Effect Intensity")]
    [Tooltip("Overall intensity of the twist glitch effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Header("Position Settings")]
    [Tooltip("Center position of the twist effect (0-1)")]
    public Vector2Parameter centerPosition = new Vector2Parameter(new Vector2(0.5f, 0.5f));
    
    [Tooltip("Rotation angle of the effect area in degrees")]
    public ClampedFloatParameter rotationAngle = new ClampedFloatParameter(0f, 0f, 360f);
    
    [Tooltip("Shape of the effect area")]
    public EffectShapeParameter effectShape = new EffectShapeParameter(EffectShape.Circular);
    
    [Header("Distortion Settings")]
    [Tooltip("Strength of the distortion effect")]
    public ClampedFloatParameter distortionStrength = new ClampedFloatParameter(0.1f, 0f, 1f);
    
    [Tooltip("Frequency of the distortion waves")]
    public ClampedFloatParameter distortionFrequency = new ClampedFloatParameter(3.0f, 0.1f, 10f);
    
    [Tooltip("Speed of the distortion animation")]
    public ClampedFloatParameter distortionSpeed = new ClampedFloatParameter(1.0f, 0f, 5f);
    
    [Header("Size Settings")]
    [Tooltip("Softness of the effect edge")]
    public ClampedFloatParameter vignetteSoftness = new ClampedFloatParameter(0.3f, 0f, 1f);
    
    [Tooltip("Radius/size of the effect area")]
    public ClampedFloatParameter vignetteRadius = new ClampedFloatParameter(1.0f, 0f, 2f);
    
    [Tooltip("Aspect ratio of the effect area (width/height)")]
    public ClampedFloatParameter aspectRatio = new ClampedFloatParameter(1.0f, 0.1f, 3.0f);
    
    [Header("Noise Settings")]
    [Tooltip("Custom noise texture for distortion patterns")]
    public TextureParameter noiseTexture = new TextureParameter(null);
    
    [Tooltip("Scale of the noise pattern")]
    public ClampedFloatParameter noiseScale = new ClampedFloatParameter(0.05f, 0.001f, 0.1f);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}

// 效果形状枚举
public enum EffectShape
{
    Circular,
    Elliptical,
    Rectangular
}

// 自定义参数类型
[System.Serializable]
public sealed class EffectShapeParameter : VolumeParameter<EffectShape>
{
    public EffectShapeParameter(EffectShape value, bool overrideState = false) : base(value, overrideState) { }
}