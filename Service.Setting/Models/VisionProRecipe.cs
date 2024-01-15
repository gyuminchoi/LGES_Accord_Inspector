using Prism.Mvvm;

namespace Service.Setting.Models
{
    public class VisionProRecipe : BindableBase
    {
        private string _patMaxToolPath;
        private string _affineToolPath;
        private string _idToolPath;
        private int _frontBoxCount;
        private int _sideBoxCount;
        private int _barcodeCount;
        private int _penSize;
        private int _barcodeWidth;
        private int _barcodeHeight;
        private string _barcodeColor;
        private string _boxColor;
        

        public string PatMaxToolPath { get => _patMaxToolPath; set => SetProperty(ref _patMaxToolPath, value); }
        public string AffineToolPath { get => _affineToolPath; set => SetProperty(ref _affineToolPath, value); }
        public string IDToolPath { get => _idToolPath; set => SetProperty(ref _idToolPath, value); }
        public int FrontBoxCount { get => _frontBoxCount; set => SetProperty(ref _frontBoxCount, value); }
        public int SideBoxCount { get => _sideBoxCount; set => SetProperty(ref _sideBoxCount, value); }
        public int BarcodeCount { get => _barcodeCount; set => SetProperty(ref _barcodeCount, value); }
        public int PenSize { get => _penSize; set => SetProperty(ref _penSize, value); }
        public int BarcodeWidth { get => _barcodeWidth; set => SetProperty(ref _barcodeWidth, value); }
        public int BarcodeHeight { get => _barcodeHeight; set => SetProperty(ref _barcodeHeight, value); }
        public string BarcodeColor { get => _barcodeColor; set => SetProperty(ref _barcodeColor, value); }
        public string BoxColor { get => _boxColor; set => SetProperty(ref _boxColor, value); }


        public VisionProRecipe(string patMaxPath, string affinePath, string idPath, int frontBoxCount, int sideBoxCount, int barcodeCount, int penSize, int barcodeWidth, int barcodeHeight, string barcodeColor, string boxColor)
        {
            PatMaxToolPath = patMaxPath;
            AffineToolPath = affinePath;
            IDToolPath = idPath;
            FrontBoxCount = frontBoxCount;
            SideBoxCount = sideBoxCount;
            BarcodeCount = barcodeCount;
            PenSize = penSize;
            BarcodeWidth = barcodeWidth;
            BarcodeHeight = barcodeHeight;
            BarcodeColor = barcodeColor;
            BoxColor = boxColor;
        }
    }
}
