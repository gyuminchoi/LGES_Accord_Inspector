using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Models
{
    public interface ICamera : IDisposable
    { 
        CameraConfig CamConfig { get; set; }
        ConcurrentQueue<byte[]> RawDatas { get; set; }
        event EventHandler<string> ReceiveRawDataEnqueueComplete;
        //ConcurrentQueue<Bitmap> ReceiveImageDatas { get; set; }
        //event EventHandler<string> ReceiveImageDataEnqueueComplete;
        int MaxEnqueueCount { get; set; }
        void Open(bool isReConnect);
        void Close();
        void UpdateCameraDatas();
        void SetParameter(ECameraGetSetType camGetSetType, object command, object value);
        object GetParameter(ECameraGetSetType camGetSetType, object command);
        void AcqStart();
        void AcqStop();
        void GrabStart();
        void GrabStop();
        void SWTriggerExecute(ICamera cam);
        void ContinueSWTrigExecute();
        void StopContinueTrigExecute();
        ulong GetPacketLossCount();

    }
}
