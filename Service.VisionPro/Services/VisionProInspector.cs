using Cognex.VisionPro;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.ImageMerge.Models;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.VisionPro.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Service.VisionPro.Services
{
    public class VisionProInspector : IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;
        
        private Thread _inspectThread = new Thread(() => { });
        private CogPMAlignTool _patMaxTool;
        private CogAffineTransformTool _affineTool;
        private CogIDTool _idTool;
        private ConcurrentQueue<MergeBitmap> _mergeBmpQueue;
        private VisionProRecipe _loadedRecipe;

        public ConcurrentQueue<VisionProResult> VisionProResultQueue { get; set; } = new ConcurrentQueue<VisionProResult>();
        public bool IsRun { get; set; } = false;

        public VisionProInspector(ConcurrentQueue<MergeBitmap> mergeBmpQueue)
        {
            _mergeBmpQueue = mergeBmpQueue;
        }

        public void RecipeLoad(VisionProRecipe recipe)
        {
            try
            {
                DisposeTools();
                _patMaxTool = LoadFile(recipe.PatMaxToolPath) as CogPMAlignTool;
                _affineTool = LoadFile(recipe.AffineToolPath) as CogAffineTransformTool;
                _idTool = LoadFile(recipe.IDToolPath) as CogIDTool;
                _loadedRecipe = recipe;
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
                Dispose();
            }
        }

        public void Start()
        {
            try
            {
                IsRun = true;

                _inspectThread = new Thread(new ThreadStart(InspectionProcess));
                _inspectThread.Name = "VisionPro Inspection Thread";
                _inspectThread.Start();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
                Stop();
                IsRun = false;
            }
        }

        public void Stop()
        {
            try
            {
                if (!_inspectThread.IsAlive)
                    return;

                IsRun = false;

                if (_inspectThread.Join(1000))
                    _inspectThread.Abort();

                if(VisionProResultQueue.Count > 0)
                {
                    for (int i = 0; i < VisionProResultQueue.Count; i++)
                    {
                        VisionProResult result = null;
                        VisionProResultQueue.TryDequeue(out result);
                        result.Dispose();
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private void InspectionProcess()
        {
            int errCount = 0;
            try
            {
                MergeBitmap mergeBmp = null;
                while (IsRun)
                {
                    try
                    {
                        if (!_mergeBmpQueue.TryDequeue(out mergeBmp))
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        VisionProResult result = new VisionProResult(new List<Box>());
                        //result.OriginBmp = mergeBmp;
                        using (var cogBmp = new CogImage8Grey(mergeBmp.Bmp))
                        {
                            
                            PMAlignToolRun(_patMaxTool, cogBmp);

                            for (int i = 0; i < _patMaxTool.Results.Count; i++)
                            {
                                AffineToolRun(_affineTool, _patMaxTool, i, cogBmp);

                                Box box = new Box(
                                    //cropBmp: _affineTool.OutputImage,
                                    x: _affineTool.Region.CenterX,
                                    y: _affineTool.Region.CenterY,
                                    width: _affineTool.Region.SideXLength,
                                    height: _affineTool.Region.SideYLength);
                                    result.BoxDatas.Add(box);

                                IDToolRun(_idTool, _affineTool.OutputImage);

                                result.BoxDatas[i].Barcodes = new List<Barcode>();
                                for (int j = 0; j < _idTool.Results.Count; j++)
                                {
                                    Barcode barcode = new Barcode(
                                        x: _idTool.Results[j].CenterX, 
                                        y: _idTool.Results[j].CenterY, 
                                        code: _idTool.Results[j].DecodedData.DecodedString);
                                        result.BoxDatas[i].Barcodes.Add(barcode);
                                }
                            }
                            SetPassOrFail(result, _loadedRecipe);
                            //TODO :Test 
                            result.OriginBmp = cogBmp.ToBitmap();
                            VisionProResultQueue.Enqueue(result);
                            mergeBmp.Dispose();
                        }
                        errCount = 0;
                        Thread.Sleep(10);
                    }
                    catch (Exception err)
                    {
                        _logWrite?.Error(err, false, true);

                        if (errCount > 100)
                        {
                            _logWrite?.Error(err);
                            break;
                        }
                        errCount++;
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        private void SetPassOrFail(VisionProResult result, VisionProRecipe recipe)
        {
            bool isPass = true;

            if(result.BoxDatas.Count != recipe.FrontBoxCount || result.BoxDatas.Count != recipe.SideBoxCount) isPass = false;

            foreach (var box in result.BoxDatas)
            {
                if(box.Barcodes.Count != recipe.BarcodeCount) isPass = false;
            }
            //TODO : 검사 이력 중 같은 바코드 번호가 존재하는 지 검사
            result.IsPass = isPass;
        }

        private object LoadFile(string path)
        {
            if (!File.Exists(path))
                throw new VisionProException(EVisionProError.FileDoesNotExist, path);

            return CogSerializer.LoadObjectFromFile(path);
        }

        private void PMAlignToolRun(CogPMAlignTool baseTool, CogImage8Grey cogBmp)
        {
            baseTool.InputImage = cogBmp;
            baseTool.Run();
        }

        private void AffineToolRun(CogAffineTransformTool baseTool, CogPMAlignTool inputTool, int index, CogImage8Grey cogBmp)
        {
            baseTool.InputImage = cogBmp;
            baseTool.Region.CenterX = inputTool.Results[index].GetPose().TranslationX;
            baseTool.Region.CenterY = inputTool.Results[index].GetPose().TranslationY;
            baseTool.Region.SideXLength = inputTool.Pattern.GetTrainedPatternImage().Width;
            baseTool.Region.SideYLength = inputTool.Pattern.GetTrainedPatternImage().Height;
            baseTool.Run();
        }

        public void IDToolRun(CogIDTool baseTool, ICogImage cogBmp)
        {
            baseTool.InputImage = cogBmp;
            baseTool.Run();
        }

        private void DisposeTools()
        {
            if (_patMaxTool != null) 
            {
                _patMaxTool.Dispose();
                _patMaxTool= null;
            }
            
            if (_idTool != null)
            {
                _idTool.Dispose();
                _idTool = null;
            }

            if (_affineTool != null)
            {
                _affineTool.Dispose();
                _affineTool = null;
            }
        }

        public void Dispose()
        {
            try 
            {
                Stop();
                DisposeTools();
            } 
            catch { }
        }

        
    }
}
