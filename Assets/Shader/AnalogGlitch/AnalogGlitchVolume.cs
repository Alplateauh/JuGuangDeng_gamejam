using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom Effects/Analog Glitch")]
public class AnalogGlitchVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Scan Line Jitter")]
    public ClampedFloatParameter scanLineJitter = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Header("Vertical Jump")]
    public ClampedFloatParameter verticalJump = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Header("Horizontal Shake")]
    public ClampedFloatParameter horizontalShake = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Header("Color Drift")]
    public ClampedFloatParameter colorDrift = new ClampedFloatParameter(0f, 0f, 1f);
    
    public bool IsActive() => scanLineJitter.value > 0f || verticalJump.value > 0f || 
                              horizontalShake.value > 0f || colorDrift.value > 0f;
    
    public bool IsTileCompatible() => false;
}