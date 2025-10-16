using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SnowGlitchRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    public Settings settings = new Settings();
    private SnowGlitchRenderPass _renderPass;

    public override void Create()
    {
        if (settings.shader == null)
        {
            settings.shader = Shader.Find("Hidden/Custom/Snow");
        }
        
        _renderPass = new SnowGlitchRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled && 
            settings.shader != null && 
            _renderPass != null)
        {
            // 只设置渲染事件，不在这里访问 cameraColorTarget
            _renderPass.renderPassEvent = settings.renderPassEvent;
            renderer.EnqueuePass(_renderPass);
        }
    }
}