using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class DynamicLodPreference
    {
        public DynamicLodPreference()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }
  
            Loaded += (_, _) =>
            {
                InitializeComponent();
                var dataContext = DataContext as ApplicationViewModel;
            };
        }

        private void TxtBox_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(int.TryParse(e.Text, out _) || (e.Text.Trim() == "-"));
        }
    }

  

    public class AglValidationRule : ValidationRule
    {
        private const int MIN = -2000;
        private const int MAX = 99999;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var agl = 0;

            try
            {
                var valueString = value as string;
                if (valueString?.Length > 0)
                    agl = int.Parse(valueString);
            }
            catch
            {
                return new ValidationResult(false, "AGL must be a number.");
            }

            if (agl is < MIN or > MAX)
            {
                return new ValidationResult(false,
                    $"AGL must be in the range: {MIN} - {MAX}.");
            }
            return ValidationResult.ValidResult;
        }
    }

    public class LodValidationRule : ValidationRule
    {
        private const int MIN = 0;
        private const int MAX = 400;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var lod = 0;

            try
            {
                var valueString = value as string;
                if (valueString?.Length > 0)
                    lod = int.Parse(valueString);
            }
            catch
            {
                return new ValidationResult(false, "LOD must be a number.");
            }

            if (lod is < MIN or > MAX)
            {
                return new ValidationResult(false,
                    $"LOD must be in the range: {MIN} - {MAX}.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
