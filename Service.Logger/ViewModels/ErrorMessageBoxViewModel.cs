using Prism.Mvvm;

namespace Service.Logger.ViewModels
{
    public class ErrorMessageBoxViewModel : BindableBase
    {
        private string _errorTitle;
        private string _errorMessage;

        public string ErrorTitle { get => _errorTitle; set => SetProperty(ref _errorTitle, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        public void SetMessage(string errTitle, string errMessage)
        {
            ErrorTitle = errTitle;
            ErrorMessage = errMessage;
        }
    }
}
