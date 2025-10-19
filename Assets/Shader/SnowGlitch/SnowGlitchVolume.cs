using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom Effects/Snow Glitch Effect")]
public class SnowGlitchVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Snow Settings")]
    public ClampedFloatParameter snowIntensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter snowSize = new ClampedFloatParameter(0.05f, 0.01f, 0.2f);
    public ClampedFloatParameter snowDensity = new ClampedFloatParameter(0.3f, 0f, 1f);
    
    [Header("Noise Settings")]
    public ClampedFloatParameter flickerSpeed = new ClampedFloatParameter(1f, 0f, 5f);
    public ClampedFloatParameter staticIntensity = new ClampedFloatParameter(0.2f, 0f, 1f);

    public bool IsActive() => snowIntensity.value > 0f;
    public bool IsTileCompatible() => false;
}