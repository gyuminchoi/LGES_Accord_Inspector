using Cognex.VisionPro;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.Database.Services;
using Service.ImageMerge.Models;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.VisionPro.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Service.VisionPro.Services
{
    
    public class VisionProInspector : IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;

        private ISQLiteManager _sqliteManager;
        private Thread _inspectThread = new Thread(() => { });
        private CogPMAlignTool _patMaxTool;
        private CogAffineTransformTool _affineTool;
        private CogIDTool _idTool;
        private ConcurrentQueue<MergeBitmap> _mergeBmpQueue;
        private VisionProRecipe _loadedRecipe;

        public ConcurrentQueue<VisionProResult> VisionProResultQueue { get; set; } = new ConcurrentQueue<VisionProResult>();
        public bool IsRun { get; set; } = false;

        public VisionProInspector(ConcurrentQueue<MergeBitmap> mergeBmpQueue, ISQLiteManager sqliteManager)
        {
            _mergeBmpQueue = mergeBmpQueue;
            _sqliteManager = sqliteManager;
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
                if(VisionProResultQueue.Count > 0)
                {
                    for(int i = 0; i < VisionProResultQueue.Count; i++)
                    {
                        VisionProResultQueue.TryDequeue(out VisionProResult result);
                        result.Dispose();
                    }
                }

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

                if (VisionProResultQueue.Count > 0)
                {
                    for (int i = 0; i < VisionProResultQueue.Count; i++)
                    {
                        VisionProResultQueue.TryDequeue(out VisionProResult result);
                        result.Dispose();
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public VisionProResult BCRInspection(MergeBitmap mergeBmp) 
        {
            var vpResult = new VisionProResult() { BoxDatas = new List<Box>() };

            using (var cogBmp = new CogImage8Grey(mergeBmp.Bmp))
            {
                PMAlignToolRun(_patMaxTool, cogBmp);

                // 0개라면 다음 검사 안함.
                for (int i = 0; i < _patMaxTool.Results.Count; i++)
                {
                    AffineToolRun(_affineTool, _patMaxTool, i, cogBmp);
                    Box box = new Box(
                        cropBmp: _affineTool.OutputImage.ToBitmap(),
                        x: _affineTool.Region.CenterX,
                        y: _affineTool.Region.CenterY,
                        width: _affineTool.Region.SideXLength,
                        height: _affineTool.Region.SideYLength);
                    vpResult.BoxDatas.Add(box);

                    IDToolRun(_idTool, _affineTool.OutputImage);

                    vpResult.BoxDatas[i].Barcodes = new List<Barcode>();
                    for (int j = 0; j < _idTool.Results.Count; j++)
                    {
                        Barcode barcode = new Barcode(
                            x: _idTool.Results[j].CenterX,
                            y: _idTool.Results[j].CenterY,
                            code: _idTool.Results[j].DecodedData.DecodedString);

                        vpResult.BoxDatas[i].Barcodes.Add(barcode);
                    }

                    if (vpResult.BoxDatas[i].Barcodes.Count >= 2)
                        box.Barcodes.Sort((barcode1, barcode2) => barcode1.Code.Length.CompareTo(barcode2.Code.Length));
                }
                
                vpResult.Bmp = cogBmp.ToBitmap();

                // PM Align 검사 결과 Fail이라면
                bool boxInspectionResult = BoxInspection(_patMaxTool, _loadedRecipe);
                if (!boxInspectionResult)
                {
                    vpResult.IsPass = false;
                    return vpResult;
                }

                vpResult.IsPass = BarcodeInspection(vpResult, _loadedRecipe);

                //TODO :Test 
                vpResult.IsPass = true;

                return vpResult;
            }
        }


        private void InspectionProcess()
        {
            int errCount = 0;
            try
            {
                while (IsRun)
                {
                    try
                    {
                        MergeBitmap mergeBmp = null;
                        if (!_mergeBmpQueue.TryDequeue(out mergeBmp))
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        var vpResult = new VisionProResult() { BoxDatas = new List<Box>() };

                        using (var cogBmp = new CogImage8Grey(mergeBmp.Bmp))
                        {
                            PMAlignToolRun(_patMaxTool, cogBmp);

                            if (_patMaxTool.Results.Count > 0)
                            {
                                // PM Align 검사 결과 Pass 라면
                                for (int i = 0; i < _patMaxTool.Results.Count; i++)
                                {
                                    AffineToolRun(_affineTool, _patMaxTool, i, cogBmp);

                                    Box box = new Box(
                                        cropBmp: _affineTool.OutputImage.ToBitmap(),
                                        x: _affineTool.Region.CenterX,
                                        y: _affineTool.Region.CenterY,
                                        width: _affineTool.Region.SideXLength,
                                        height: _affineTool.Region.SideYLength);

                                    vpResult.BoxDatas.Add(box);

                                    IDToolRun(_idTool, _affineTool.OutputImage);

                                    vpResult.BoxDatas[i].Barcodes = new List<Barcode>();
                                    for (int j = 0; j < _idTool.Results.Count; j++)
                                    {
                                        Barcode barcode = new Barcode(
                                            x: _idTool.Results[j].CenterX,
                                            y: _idTool.Results[j].CenterY,
                                            code: _idTool.Results[j].DecodedData.DecodedString);

                                        vpResult.BoxDatas[i].Barcodes.Add(barcode);
                                    }

                                    if (vpResult.BoxDatas[i].Barcodes.Count == 2)
                                        box.Barcodes.Sort((barcode1, barcode2) => barcode1.Code.Length.CompareTo(barcode2.Code.Length));
                                }
                            }

                            vpResult.InspectionTime = DateTime.Now;
                            vpResult.Bmp = cogBmp.ToBitmap();

                            // PM Align 검사 결과 Fail이라면
                            bool boxInspectionResult = BoxInspection(_patMaxTool, _loadedRecipe);
                            if (!boxInspectionResult)
                            {
                                vpResult.IsPass = false;
                                mergeBmp.Dispose();

                                VisionProResultQueue.Enqueue(vpResult);
                                continue;
                            }

                            //vpResult.IsPass = BarcodeInspection(vpResult, _loadedRecipe);
                            //vpResult.IsPass = true;
                            mergeBmp.Dispose();
                            VisionProResultQueue.Enqueue(vpResult);
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

        private bool BoxInspection(CogPMAlignTool pmAlignTool, VisionProRecipe recipe)
        {
            if(pmAlignTool.Results.Count == recipe.SideBoxCount || pmAlignTool.Results.Count == recipe.FrontBoxCount)
                return true;
            else 
                return false;
        }

        private bool BarcodeInspection(VisionProResult result, VisionProRecipe recipe)
        {
            DateTime date = DateTime.Now.AddDays(recipe.DateRange * -1);

            foreach (var box in result.BoxDatas)
            {
                if (box.Barcodes.Count != recipe.BarcodeCount) return false;

                int count = _sqliteManager.SearchCount(box.Barcodes[0].Code, box.Barcodes[1].Code, DateTime.Now, date);
                if (count > 0) return false;
            }

            return true;
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
