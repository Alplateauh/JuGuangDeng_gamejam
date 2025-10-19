using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnalogGlitchRenderPass : ScriptableRenderPass
{
    private const string PROFILER_TAG = "Analog Glitch";
    private AnalogGlitchVolume volume;
    private Material material;
    private float verticalJumpTime;
    
    public AnalogGlitchRenderPass(AnalogGlitchRendererFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        
        if (settings.shader != null)
        {
            material = CoreUtils.CreateEngineMaterial(settings.shader);
        }
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null) return;
        
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<AnalogGlitchVolume>();
        if (volume == null || !volume.IsActive()) return;
        
        CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);
        
        try
        {
            SetupMaterialProperties();
            
            var source = renderingData.cameraData.renderer.cameraColorTarget;
            var tempTarget = Shader.PropertyToID("_TempAnalogGlitch");
            cmd.GetTemporaryRT(tempTarget, renderingData.cameraData.cameraTargetDescriptor);
            
            // Blit with material
            cmd.Blit(source, tempTarget, material);
            cmd.Blit(tempTarget, source);
            
            cmd.ReleaseTemporaryRT(tempTarget);
            
            context.ExecuteCommandBuffer(cmd);
        }
        finally
        {
            cmd.Release();
        }
    }
    
    private void SetupMaterialProperties()
    {
        if (material == null || volume == null) return;
        
        verticalJumpTime += Time.deltaTime * volume.verticalJump.value * 11.3f;
        
        // Scan line jitter
        var sl_thresh = Mathf.Clamp01(1.0f - volume.scanLineJitter.value * 1.2f);
        var sl_disp = 0.002f + Mathf.Pow(volume.scanLineJitter.value, 3) * 0.05f;
        material.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
        
        // Vertical jump
        var vj = new Vector2(volume.verticalJump.value, verticalJumpTime);
        material.SetVector("_VerticalJump", vj);
        
        // Horizontal shake
        material.SetFloat("_HorizontalShake", volume.horizontalShake.value * 0.2f);
        
        // Color drift
        var cd = new Vector2(volume.colorDrift.value * 0.04f, Time.time * 606.11f);
        material.SetVector("_ColorDrift", cd);
    }
    
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
    }
}