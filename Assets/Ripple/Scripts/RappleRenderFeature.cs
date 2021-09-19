using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class RappleRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {        
        //��������صĲ���д��RT��
        public Material AddMat;

        //���ڻ���������ɢЧ��
        public Material RippleMat;

        //�����Ⱦˮ�õĲ���
        public Material WaterMaterial;

        //���������Ⱦ��RT
        public RenderTexture InteractiveRT;

        //����Ƶ��
        public int sampleTextureCount;

        //��������Ƿ���Խ���
        public bool isPointerInteractive = false;

        //��꽻��ʱ���ƵĴ�С
        [Range(0, 1.0f)]
        public float drawRadius = 0.2f;
        //����˥��
        [Range(0, 1.0f)]
        public float waveAttenuation = 0.99f;

        //���Ƹ�pass��ʲôʱ������
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public int RenderTextureSize = 512;

    }


    public Settings setting = new Settings();

    

    class CustomRenderPass : ScriptableRenderPass
    {
        public Settings setting;

        //����tag Unity����Ⱦ���и�tag��Shader
        public ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
        //public ShaderTagId shaderTag = new ShaderTagId("Ripple");

        public FilteringSettings filteringSetting;

        RenderTexture prevRT;
        RenderTexture currentRT;
        RenderTexture tempRT;

        //����������
        private int m_sampleCounter;

        //��ʼ��RT����
        private void InitRT()
        {
            currentRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
            prevRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
            tempRT = new RenderTexture(setting.RenderTextureSize, setting.RenderTextureSize, 0, RenderTextureFormat.RFloat);
        }
        //����RT����
        private void ExchangeRT(ref RenderTexture a,ref RenderTexture b)
        {
            RenderTexture rt = a;
            a = b;
            b = rt;
        }
        //���캯��
        public CustomRenderPass(Settings setting)
        {
            //��ȡsetting����
            this.setting = setting;
            //��Ⱦ����
            RenderQueueRange queue = new RenderQueueRange();
            ////��������
            filteringSetting = new FilteringSettings(queue);
            //����������
            CameraRaycast.drawRadius = setting.drawRadius;
            //��ʼ��RT
            InitRT();
            //��ʼ������Ƶ��
            m_sampleCounter = setting.sampleTextureCount;
            
        }



        //Scriptable Render Pass����ͨ��Configure(...)�������ض�����ȾĿ��, ��Ȼ, ����ض����Ч��ֻ���ڸ�Pass����ʱ��Ч

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

            //���Ʋ�������
            if (setting.sampleTextureCount <= 1 || m_sampleCounter == setting.sampleTextureCount)
            {
                //��������Ƿ���Խ���
                if (setting.isPointerInteractive == true && CameraRaycast.isRaycast == true)
                    setting.AddMat.SetVector("_PositionPoint", CameraRaycast.currentPos);

                setting.AddMat.SetFloat("_isRenderMousePointer", setting.isPointerInteractive == true ? 1.0f : 0f);

                //�����彻��RT����Shader
                setting.AddMat.SetTexture("_InteractiveTex", setting.InteractiveRT);
                //������һ֡��RT
                setting.AddMat.SetTexture("_CurrentRT", currentRT);
                //������������tempRT
                cmd.Blit(null, tempRT, setting.AddMat);
                //��tempRT���ø�currentRT������currentRTΪ���µ�RT
                ExchangeRT(ref tempRT, ref currentRT);
                //�����������
                m_sampleCounter = 0;
            }
            else
            {
                m_sampleCounter++;
            }

            //��������
            //������һ֡RT
            setting.RippleMat.SetTexture("_PrevRT", prevRT);
            //����֮ǰ����õ����µ�RT
            setting.RippleMat.SetTexture("_CurrentRT", currentRT);
            //����˥��ϵ��
            setting.RippleMat.SetFloat("_Attenuation", setting.waveAttenuation);

            //���������tempRT
            cmd.Blit(null, tempRT, setting.RippleMat);
            //��������Ƹ�prevRT
            cmd.Blit(tempRT, prevRT);

            //��������RT��current���մ洢������������֮���RT��pre���մ洢��������֮ǰ��RT��
            //������һ����Ⱦʱ,��Ϊ��һ֡�������м���
            ExchangeRT(ref prevRT, ref currentRT);

            //Ϊ��Ⱦˮ�Ĳ���������ͼ�Ͳ���
            //setting.WaterMaterial.SetTexture("_SourceTexture", currentRT);

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //�������д��Ⱦ�߼�
            var drawFrame = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            setting.WaterMaterial.SetTexture("_SourceTexture", currentRT);
            //�趨��Ⱦ������pass����
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


