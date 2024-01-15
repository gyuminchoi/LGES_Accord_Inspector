//using Service.Camera.Models;
//using Service.Preprocessing.Models;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Service.Preprocessing.Services
//{
//    public interface IPreprocessingManager : IDisposable
//    {
//        bool IsRun { get; set; }
//        Dictionary<string, ReceivedBitmap> ReceivedBitmapDic { get; set; }
//        ConcurrentQueue<Bitmap> TopMergeBitmapQueue { get; set; }
//        ConcurrentQueue<Bitmap> BotMergeBitmapQueue { get; set; }
//        void Initialize(ICameraManager cm);
//        void Start();
//        void Stop();
//    }
//}
