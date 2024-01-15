using Dialog.AppSetting.Models;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dialog.AppSetting.ViewModels
{
    public class AppSettingViewModel : BindableBase, IDialogAware
    {
        public string Title => "App Setting Dialog";
        private ISettingManager _settingManager;
        private LogWrite _logWrite = LogWrite.Instance;
        private VisionProRecipe _selectedRecipe;
        private string _barcodeColor;
        private string _boxColor;

        public event Action<IDialogResult> RequestClose;
        public ISettingManager SettingManager { get => _settingManager; set => SetProperty(ref _settingManager, value); }
        public ObservableCollection<KeyValuePair<string, VisionProRecipe>> VisionProRecipe { get; set; } = new ObservableCollection<KeyValuePair<string, VisionProRecipe>>();
        public VisionProRecipe SelectedRecipe { get => _selectedRecipe; set => SetProperty(ref _selectedRecipe, value); }
        public string BarcodeColor { get => _barcodeColor; set => SetProperty(ref _barcodeColor, value); }
        public string BoxColor { get => _boxColor; set => SetProperty(ref _boxColor, value); }

        public DelegateCommand<object> BtnSelectImageSavePathCommand => new DelegateCommand<object>(OnSelectPath);
        public DelegateCommand<VisionProRecipe> RecipeChanagedCommand => new DelegateCommand<VisionProRecipe>(OnSetROIColors);
        public DelegateCommand<VisionProRecipe> BarcodeColorChangedCommand => new DelegateCommand<VisionProRecipe>(OnSetBarcodeColor);
        public DelegateCommand<VisionProRecipe> BoxColorChangedCommand => new DelegateCommand<VisionProRecipe>(OnSetBoxColor);

        public AppSettingViewModel(ISettingManager sm)
        {
            _settingManager = sm;
        }

        public bool CanCloseDialog()
        {
            var isSave = MessageBox.Show("Do you want to save it?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (isSave)
            {
                case MessageBoxResult.Yes:
                    SettingManager.Save();
                    return true;

                case MessageBoxResult.No:
                    SettingManager.Load();
                    return true;

                case MessageBoxResult.Cancel:
                default:
                    return false;
            }

        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            VisionProRecipe.Clear();
            foreach (var recipe in SettingManager.AppSetting.VisionProSetting.Recipes)
            {
                VisionProRecipe.Add(recipe);
            }
            if(VisionProRecipe.Count != 0)
            {
                SelectedRecipe = VisionProRecipe.First().Value;
                BarcodeColor = SelectedRecipe.BarcodeColor;
                BoxColor = SelectedRecipe.BoxColor;
            }

            _logWrite?.Info("Complete App Setting dialog init");
        }

        private void OnSetBoxColor(VisionProRecipe recipe) => recipe.BoxColor = BoxColor;
        private void OnSetBarcodeColor(VisionProRecipe recipe) => recipe.BarcodeColor = BarcodeColor;

        private void OnSetROIColors(VisionProRecipe recipe)
        {
            BarcodeColor = recipe.BarcodeColor;
            BoxColor = recipe.BoxColor;
        }
        
        private void OnSelectPath(object type)
        {
            PathType pathType = (PathType)type;
            string path = null;
            switch (pathType)
            {
                case PathType.InspectionImageSavePath:
                    path = GetFolderPath();
                    if (path != null)
                        SettingManager.AppSetting.ImageSetting.InspectionImageSavePath = path;
                    return;

                case PathType.LiveImageSavePath:
                    path = GetFolderPath();
                    if(path != null)
                        SettingManager.AppSetting.ImageSetting.LiveImageSavePath = path;
                    return;

                case PathType.DataSavePath:
                    path = GetFolderPath();
                    if(path !=  null)
                        SettingManager.AppSetting.DataSetting.SavePath = path;
                    return;

                case PathType.DataSendPath: 
                    path = GetFolderPath();
                    if (path != null)
                        SettingManager.AppSetting.DataSetting.SendPath = path;
                    return;

                case PathType.VisionProPatMaxToolPath:
                    path = GetFilePath(".vpp");

                    if (path != null)
                        SelectedRecipe.PatMaxToolPath = path;
                    return;

                case PathType.VisionProAffineToolPath:
                    path = GetFilePath(".vpp");
                    if (path != null)
                        SelectedRecipe.AffineToolPath = path;
                    return;

                case PathType.VisionProIDToolPath:
                    path = GetFilePath(".vpp");
                    if (path != null)
                        SelectedRecipe.IDToolPath = path;
                    return;
            }
        }

        private string GetFolderPath()
        {
            var selectFileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (selectFileDialog.ShowDialog() == CommonFileDialogResult.Ok) 
                return selectFileDialog.FileName;
            else 
                return null;
        }

        private string GetFilePath(string filter)
        {
            OpenFileDialog dig = new OpenFileDialog();
            dig.DefaultExt = "filter";
            dig.Filter = $"{filter}(*{filter})|*{filter}";
            bool? result = dig.ShowDialog();

            if (result == true) return dig.FileName;
            else return null;
        }
    }
}
