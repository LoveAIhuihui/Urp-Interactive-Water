using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class RappleRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {        
        //将交互相关的操作写入RT中
        public Material AddMat;

        //用于绘制涟漪扩散效果
        public Material RippleMat;

        //最后渲染水用的材质
        public Material WaterMaterial;

        //交互相机渲染的RT
        public RenderTexture InteractiveRT;

        //采样频率
        public int sampleTextureCount;

        //控制鼠标是否可以交互
        public bool isPointerInteractive = false;

        //鼠标交互时绘制的大小
        [Range(0, 1.0f)]
        public float drawRadius = 0.2f;
        //波浪衰减
        [Range(0, 1.0f)]
        public float waveAttenuation = 0.99f;

        //控制该pass在什么时候运行
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public int RenderTextureSize = 512;

    }


    public Settings setting = new Settings();

    

    class CustomRenderPass : ScriptableRenderPass
    {
        public Settings setting;

        //设置tag Unity会渲染带有该tag的Shader
        public ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
        //public ShaderTagId shaderTag = new ShaderTagId("Ripple");

        public FilteringSettings filteringSetting;

        RenderTexture prevRT;
        RenderTexture currentRT;
        RenderTexture tempRT;

        //采样计数器
        private int m_sampleCounter;

        //初始化RT方法
        private void InitRT()
        {
            currentRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
            prevRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
            tempRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
        }
        //交换RT方法
        private void ExchangeRT(ref RenderTexture a,ref RenderTexture b)
        {
            RenderTexture rt = a;
            a = b;
            b = rt;
        }
        //构造函数
        public CustomRenderPass(Settings setting)
        {
            //获取setting设置
            this.setting = setting;
            //渲染队列
            RenderQueueRange queue = new RenderQueueRange();
            ////过滤设置
            filteringSetting = new FilteringSettings(queue);
            //鼠标控制涟漪
            CameraRaycast.drawRadius = setting.drawRadius;
            //初始化RT
            InitRT();
            //初始化采样频率
            m_sampleCounter = setting.sampleTextureCount;
            
        }



        //Scriptable Render Pass可以通过Configure(...)函数来重定向渲染目标, 当然, 这个重定向的效果只会在该Pass运行时生效

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

            //控制采样次数
            if (setting.sampleTextureCount <= 1 || m_sampleCounter == setting.sampleTextureCount)
            {
                //控制鼠标是否可以交互
                if (setting.isPointerInteractive == true && CameraRaycast.isRaycast == true)
                    setting.AddMat.SetVector("_PositionPoint", CameraRaycast.currentPos);

                setting.AddMat.SetFloat("_isRenderMousePointer", setting.isPointerInteractive == true ? 1.0f : 0f);

                //将物体交互RT传入Shader
                setting.AddMat.SetTexture("_InteractiveTex", setting.InteractiveRT);
                //传入上一帧的RT
                setting.AddMat.SetTexture("_CurrentRT", currentRT);
                //将处理结果传入tempRT
                cmd.Blit(null, tempRT, setting.AddMat);
                //将tempRT引用给currentRT，保持currentRT为最新的RT
                ExchangeRT(ref tempRT, ref currentRT);
                //清零采样计数
                m_sampleCounter = 0;
            }
            else
            {
                m_sampleCounter++;
            }

            //涟漪计算
            //传入上一帧RT
            setting.RippleMat.SetTexture("_PrevRT", prevRT);
            //传入之前处理好的最新的RT
            setting.RippleMat.SetTexture("_CurrentRT", currentRT);
            //传入衰减系数
            setting.RippleMat.SetFloat("_Attenuation", setting.waveAttenuation);

            //将结果传入tempRT
            cmd.Blit(null, tempRT, setting.RippleMat);
            //将结果复制给prevRT
            cmd.Blit(tempRT, prevRT);

            //交换两个RT，current最终存储的是涟漪计算之后的RT，pre最终存储涟漪计算之前的RT，
            //用于下一次渲染时,作为上一帧参数进行计算
            ExchangeRT(ref prevRT, ref currentRT);

            //为渲染水的材质设置贴图和参数
            //setting.WaterMaterial.SetTexture("_SourceTexture", currentRT);

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //在这里编写渲染逻辑
            var drawFrame = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            setting.WaterMaterial.SetTexture("_SourceTexture", currentRT);
            //设定渲染参数和pass索引
            drawFrame.overrideMaterial = setting.WaterMaterial;
            drawFrame.overrideMaterialPassIndex = 0;

            context.DrawRenderers(renderingData.cullResults, ref drawFrame, ref filteringSetting);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            //tempRT.Release();
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(setting); 
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = setting.Event;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


