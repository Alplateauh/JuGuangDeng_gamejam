using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TwistGlitchRenderPass : ScriptableRenderPass
{
    private const string PROFILER_TAG = "Twist Glitch";
    private Material _material;
    private TwistGlitchVolume _volume;
    private int _tempTextureId = Shader.PropertyToID("_TempTwistGlitchTexture");
    
    private TwistGlitchRendererFeature.Settings _settings;
    
    public TwistGlitchRenderPass(TwistGlitchRendererFeature.Settings settings)
    {
        _settings = settings;
        renderPassEvent = settings.renderPassEvent;
        
        if (settings.shader != null)
        {
            _material = CoreUtils.CreateEngineMaterial(settings.shader);
        }
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null) return;
        
        var stack = VolumeManager.instance.stack;
        _volume = stack.GetComponent<TwistGlitchVolume>();
        if (_volume == null || !_volume.IsActive()) return;
        
        // 在 Execute 方法中获取 cameraColorTarget
        var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        
        CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);
        
        using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
        {
            SetupMaterialProperties(renderingData);
            
            // 创建临时渲染纹理并应用效果
            cmd.GetTemporaryRT(_tempTextureId, renderingData.cameraData.cameraTargetDescriptor);
            
            // 使用材质进行 Blit
            cmd.Blit(cameraColorTarget, _tempTextureId, _material, 0);
            cmd.Blit(_tempTextureId, cameraColorTarget);
            
            cmd.ReleaseTemporaryRT(_tempTextureId);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    
    private void SetupMaterialProperties(RenderingData renderingData)
    {
        if (_material == null || _volume == null) return;
        
        // 设置基础 shader 属性
        _material.SetFloat("_Intensity", _volume.intensity.value);
        _material.SetFloat("_DistortionStrength", _volume.distortionStrength.value);
        _material.SetFloat("_DistortionFrequency", _volume.distortionFrequency.value);
        _material.SetFloat("_DistortionSpeed", _volume.distortionSpeed.value);
        _material.SetFloat("_VignetteSoftness", _volume.vignetteSoftness.value);
        _material.SetFloat("_VignetteRadius", _volume.vignetteRadius.value);
        _material.SetFloat("_NoiseScale", _volume.noiseScale.value);
        
        // 设置位置相关属性
        _material.SetVector("_CenterPosition", _volume.centerPosition.value);
        _material.SetFloat("_AspectRatio", _volume.aspectRatio.value);
        _material.SetFloat("_RotationAngle", _volume.rotationAngle.value * Mathf.Deg2Rad); // 转换为弧度
        
        // 设置形状类型
        _material.SetFloat("_EffectShape", (float)_volume.effectShape.value);
        
        // 使用 Volume 中指定的噪声纹理（如果有）
        if (_volume.noiseTexture.value != null)
        {
            _material.SetTexture("_NoiseTex", _volume.noiseTexture.value);
        }
    }
    
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
    }
    
    public void Dispose()
    {
        if (_material != null)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }
    }
}