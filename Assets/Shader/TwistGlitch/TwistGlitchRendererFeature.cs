using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TwistGlitchRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }
    
    public Settings settings = new Settings();
    private TwistGlitchRenderPass _renderPass;
    
    public override void Create()
    {
        if (settings.shader == null)
        {
            settings.shader = Shader.Find("Hidden/Custom/TwistGlitch");
        }
        
        _renderPass = new TwistGlitchRenderPass(settings);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled && 
            settings.shader != null && 
            _renderPass != null)
        {
            _renderPass.renderPassEvent = settings.renderPassEvent;
            renderer.EnqueuePass(_renderPass);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        _renderPass?.Dispose();
    }
}