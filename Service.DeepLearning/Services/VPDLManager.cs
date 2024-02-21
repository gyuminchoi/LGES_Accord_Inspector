using Service.DeepLearning.Models;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using System.Collections.Generic;
using System.Drawing;
using ViDi2;

namespace Service.DeepLearning.Services
{
    public class VPDLManager : IVPDLManager
    {
        private VPDLSetting _setting;
        private LogWrite _logWrite = LogWrite.Instance;
        private ViDi2.Runtime.Local.Control _control;
        private ViDi2.Runtime.IWorkspace _workspace;
        private IStream _stream;
        private VPDLDrawManager _drawManager = new VPDLDrawManager();
        public void Dispose()
        {
            _workspace?.Close();
            _control?.Dispose();
        }

        public void Initialize(ISettingManager sm)
        {
            _setting = sm.AppSetting.VPDLSetting;

            try
            {
                _control = new ViDi2.Runtime.Local.Control();
                _control.InitializeComputeDevices(GpuMode.SingleDevicePerTool, new List<int>() { });
                _workspace = _control.Workspaces.Add(_setting.Workspacename, _setting.WorkspacePath);
                _stream = _workspace.Streams[_setting.StreamName];
            }
            catch (ViDi2.Exception err)
            {
                _logWrite?.Error(err);
            }
            catch (System.Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public VPDLResult VPDLInspect(Bitmap bmp)
        {
            using(IImage img = new FormsImage(bmp))
            {
                using(ISample sample = _stream.Process(img))
                {
                    bool isPass = GetRedToolResult(sample, _setting.ToolName);
                    IRedMarking redMarking = sample.Markings[_setting.ToolName] as IRedMarking;
                    
                    Bitmap cloneBmp = bmp.Clone() as Bitmap;
                    DrawOverlayRedTool(redMarking, isPass, false, 10, 10, ref cloneBmp);

                    VPDLResult result = new VPDLResult()
                    {
                        OriginBmp = bmp,
                        OverlayBmp = cloneBmp,
                        IsPass = isPass
                    };

                    return result;
                }
            }
        }

        public bool GetRedToolResult(ISample sample, string toolName)
        {
            IRedMarking redMarking = sample.Markings[toolName] as IRedMarking;

            return (redMarking.Views[0].Regions.Count > 0) ? false : true;
        }

        public void DrawOverlayRedTool(IRedMarking redMarking, bool isPass, bool isDrawBorder, int ngPenSize, int borderPenSize, ref Bitmap bmp)
        {
            _drawManager.DrawLinesFromIRedView(ref bmp, redMarking, ngPenSize);

            if (isDrawBorder)
            {
                Color color;

                // Count로 OK NG 판단.
                if (!isPass) { color = Color.Red; } // NG 
                else { color = Color.Green; } // OK
                _drawManager.DrawingBorderFromIRedView(ref bmp, color, redMarking, borderPenSize);
            }
        }
    }
}
