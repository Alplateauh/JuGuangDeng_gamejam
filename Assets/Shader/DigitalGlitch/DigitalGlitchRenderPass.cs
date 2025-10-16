using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DigitalGlitchRenderPass : ScriptableRenderPass
{
    private const string PROFILER_TAG = "Digital Glitch";
    private Material _material;
    private DigitalGlitchVolume _volume;
    private int _tempTextureId = Shader.PropertyToID("_TempDigitalGlitchTexture");
    
    // Digital Glitch 资源
    private Texture2D _noiseTexture;
    private RenderTexture _trashFrame1;
    private RenderTexture _trashFrame2;
    private float _lastUpdateTime;
    
    private DigitalGlitchRendererFeature.Settings _settings;
    
    public DigitalGlitchRenderPass(DigitalGlitchRendererFeature.Settings settings)
    {
        _settings = settings;
        renderPassEvent = settings.renderPassEvent;
        
        if (settings.shader != null)
        {
            _material = CoreUtils.CreateEngineMaterial(settings.shader);
        }
        
        CreateResources();
    }
    
    private void CreateResources()
    {
        // 创建噪声纹理
        _noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false)
        {
            hideFlags = HideFlags.DontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        
        UpdateNoiseTexture();
    }
    
    private void UpdateNoiseTexture()
    {
        if (_noiseTexture == null) return;
        
        var color = RandomColor();
        for (var y = 0; y < _noiseTexture.height; y++)
        {
            for (var x = 0; x < _noiseTexture.width; x++)
            {
                if (Random.value > 0.89f) color = RandomColor();
                _noiseTexture.SetPixel(x, y, color);
            }
        }
        _noiseTexture.Apply();
    }
    
    private Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, Random.value);
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null) return;
        
        var stack = VolumeManager.instance.stack;
        _volume = stack.GetComponent<DigitalGlitchVolume>();
        if (_volume == null || !_volume.IsActive()) return;
        
        // 在 Execute 方法中获取 cameraColorTarget
        var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        
        CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);
        
        using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
        {
            UpdateGlitchResources(renderingData, cameraColorTarget);
            SetupMaterialProperties(renderingData, cameraColorTarget);
            
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
    
    private void UpdateGlitchResources(RenderingData renderingData, RenderTargetIdentifier source)
    {
        if (_volume == null) return;
        
        float currentTime = Time.time;
        float updateInterval = Mathf.Lerp(0.5f, 0.1f, _volume.updateFrequency.value);
        
        // 定期更新噪声纹理
        if (currentTime - _lastUpdateTime > updateInterval && 
            Random.value > Mathf.Lerp(0.9f, 0.5f, _volume.intensity.value))
        {
            UpdateNoiseTexture();
            _lastUpdateTime = currentTime;
        }
        
        // 创建或调整 trash frames
        var cameraData = renderingData.cameraData;
        int width = cameraData.cameraTargetDescriptor.width;
        int height = cameraData.cameraTargetDescriptor.height;
        
        if (_trashFrame1 == null || _trashFrame1.width != width || _trashFrame1.height != height)
        {
            DisposeTextures();
            _trashFrame1 = new RenderTexture(width, height, 0) { hideFlags = HideFlags.DontSave };
            _trashFrame2 = new RenderTexture(width, height, 0) { hideFlags = HideFlags.DontSave };
            
            // 初始化 trash frames
            CommandBuffer initCmd = CommandBufferPool.Get();
            initCmd.Blit(source, _trashFrame1);
            initCmd.Blit(source, _trashFrame2);
            Graphics.ExecuteCommandBuffer(initCmd);
            CommandBufferPool.Release(initCmd);
        }
    }
    
    private void SetupMaterialProperties(RenderingData renderingData, RenderTargetIdentifier source)
    {
        if (_material == null || _volume == null) return;
        
        // 更新 trash frames
        if (_trashFrame1 != null && _trashFrame2 != null)
        {
            var fcount = Time.frameCount;
            if (fcount % 13 == 0) 
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                cmd.Blit(source, _trashFrame1);
                Graphics.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            if (fcount % 73 == 0) 
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                cmd.Blit(source, _trashFrame2);
                Graphics.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
        
        // 设置 shader 属性
        _material.SetFloat("_Intensity", _volume.intensity.value);
        _material.SetTexture("_NoiseTex", _noiseTexture);
        
        // 使用 Volume 中指定的噪声纹理（如果有）
        if (_volume.noiseTexture.value != null)
        {
            _material.SetTexture("_NoiseTex", _volume.noiseTexture.value);
        }
        
        var trashFrame = Random.value > 0.5f ? _trashFrame1 : _trashFrame2;
        _material.SetTexture("_TrashTex", trashFrame);
    }
    
    private void DisposeTextures()
    {
        if (_trashFrame1 != null)
        {
            _trashFrame1.Release();
            _trashFrame1 = null;
        }
        if (_trashFrame2 != null)
        {
            _trashFrame2.Release();
            _trashFrame2 = null;
        }
    }
    
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
    }
    
    public void Dispose()
    {
        DisposeTextures();
        
        if (_noiseTexture != null)
        {
            Object.DestroyImmediate(_noiseTexture);
            _noiseTexture = null;
        }
        
        if (_material != null)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }
    }
}