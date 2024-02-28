using System.Globalization;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class ConfirmationViewModel
    {
        public string Title
        {
            get
            {
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                return $"Confirm {textInfo.ToTitleCase(ConfirmButtonText.ToLower())}";
            }
        }

        public string Content { get; set; }

        public string ConfirmButtonText { get; set; }

        public ConfirmationViewModel(string content, string confirmButtonText)
        {
            Content = content;
            ConfirmButtonText = confirmButtonText.ToUpper();
        }
    }
}
