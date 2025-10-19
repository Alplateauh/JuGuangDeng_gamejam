using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom Effects/Digital Glitch")]
public class DigitalGlitchVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Digital Glitch Intensity")]
    [Tooltip("Overall intensity of the digital glitch effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Header("Advanced Settings")]
    [Tooltip("Noise texture for glitch patterns")]
    public TextureParameter noiseTexture = new TextureParameter(null);
    
    [Tooltip("Update frequency of glitch patterns")]
    public ClampedFloatParameter updateFrequency = new ClampedFloatParameter(0.1f, 0.01f, 1f);
    
    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}