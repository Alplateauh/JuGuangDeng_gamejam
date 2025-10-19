using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SnowGlitchRenderPass : ScriptableRenderPass
{
    private const string PROFILER_TAG = "Snow Glitch";
    private Material _material;
    private SnowGlitchVolume _volume;
    private int _tempTextureId = Shader.PropertyToID("_TempSnowGlitchTexture");
    
    // Shader Property IDs
    private static readonly int SnowIntensityID = Shader.PropertyToID("_SnowIntensity");
    private static readonly int SnowSizeID = Shader.PropertyToID("_SnowSize");
    private static readonly int SnowDensityID = Shader.PropertyToID("_SnowDensity");
    private static readonly int FlickerSpeedID = Shader.PropertyToID("_FlickerSpeed");
    private static readonly int StaticIntensityID = Shader.PropertyToID("_StaticIntensity");
    
    private SnowGlitchRendererFeature.Settings _settings;

    public SnowGlitchRenderPass(SnowGlitchRendererFeature.Settings settings)
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
        _volume = stack.GetComponent<SnowGlitchVolume>();
        if (_volume == null || !_volume.IsActive()) return;

        // 在 Execute 方法中获取 cameraColorTarget
        var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        
        CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);
        
        using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
        {
            SetupMaterialProperties();
            
            // 创建临时渲染纹理
            cmd.GetTemporaryRT(_tempTextureId, renderingData.cameraData.cameraTargetDescriptor);
            
            // 使用材质进行 Blit 操作
            cmd.Blit(cameraColorTarget, _tempTextureId, _material, 0);
            cmd.Blit(_tempTextureId, cameraColorTarget);
            
            // 释放临时纹理
            cmd.ReleaseTemporaryRT(_tempTextureId);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void SetupMaterialProperties()
    {
        if (_material == null || _volume == null) return;
        
        // 设置 Volume 参数到材质
        _material.SetFloat(SnowIntensityID, _volume.snowIntensity.value);
        _material.SetFloat(SnowSizeID, _volume.snowSize.value);
        _material.SetFloat(SnowDensityID, _volume.snowDensity.value);
        _material.SetFloat(FlickerSpeedID, _volume.flickerSpeed.value);
        _material.SetFloat(StaticIntensityID, _volume.staticIntensity.value);
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