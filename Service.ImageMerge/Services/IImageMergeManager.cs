using Service.Camera.Models;
using Service.ImageMerge.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ImageMerge.Services
{
    public interface IImageMergeManager : IDisposable
    {
        bool IsRun { get; set; }
        ConcurrentQueue<MergeBitmap> TopMergeBitmapQueue { get; set; }
        ConcurrentQueue<MergeBitmap> BotMergeBitmapQueue { get; set; }
        void Initialize(ICameraManager cm);
        void Start();
        void Stop();
    }
}
