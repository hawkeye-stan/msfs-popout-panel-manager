using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MSFSPopoutPanelManager.DomainModel.DynamicLod;
using MSFSPopoutPanelManager.MainApp.ViewModel;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class DynamicLodPreference
    {
        private ObservableLodConfigLinkedList _tlodConfigs;
        private ObservableLodConfigLinkedList _olodConfigs;

        public DynamicLodPreference()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }
            
            AddTlodConfigs = new ObservableCollection<TempLodConfig>() { new () };
            AddOlodConfigs = new ObservableCollection<TempLodConfig>() { new () };

            Loaded += (_, _) =>
            {
                InitializeComponent();
                var dataContext = DataContext as ApplicationViewModel;
                _tlodConfigs = dataContext?.AppSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs;
                _olodConfigs = dataContext?.AppSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs;
            };
        }
        
        public ObservableCollection<TempLodConfig> AddTlodConfigs { get; set; }

        public ObservableCollection<TempLodConfig> AddOlodConfigs { get; set; }

        private void AddTlod_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var textBox = sender as TextBox;

            var lodConfig = textBox?.DataContext as TempLodConfig;

            if (lodConfig?.Agl == null || lodConfig.Lod == null)
                return;

            if (UpdateTlodDuplicate(lodConfig))
            {
                RebindTLodGrid();
                return;
            }

            var targetLodConfig = _tlodConfigs.LastOrDefault(x => lodConfig.Agl >= x.Agl);
            var newLodConfig = new LodConfig { Agl = (int)lodConfig.Agl, Lod = (int)lodConfig.Lod };

            if (targetLodConfig == null)
                _tlodConfigs.AddFirst(newLodConfig);
            else
                _tlodConfigs.AddAfter(_tlodConfigs.Find(targetLodConfig), new LinkedListNode<LodConfig>(newLodConfig));

            RebindTLodGrid();
        }
        
        private void TLodDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;

            if(button?.DataContext is not LodConfig lodConfig) 
                return;

            _tlodConfigs.Remove(lodConfig);

            RebindTLodGrid();
        }

        private void RebindTLodGrid()
        {
            this.TlodGrid.ItemsSource = null;
            this.TlodGrid.ItemsSource = _tlodConfigs.ToList();

            AddTlodConfigs.Clear();
            AddTlodConfigs.Add(new TempLodConfig());
        }

        private bool UpdateTlodDuplicate(TempLodConfig lodConfig)
        {
            var tlodConfig = _tlodConfigs.FirstOrDefault(x => x.Agl == lodConfig.Agl);

            if(tlodConfig == null)
                return false;

            tlodConfig.Lod = Convert.ToInt32(lodConfig.Lod);
            return true;
        }

        private void AddOlod_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var textBox = sender as TextBox;

            var lodConfig = textBox?.DataContext as TempLodConfig;

            if (lodConfig?.Agl == null || lodConfig.Lod == null)
                return;

            if (UpdateOlodDuplicate(lodConfig))
            {
                RebindOLodGrid();
                return;
            }

            var targetLodConfig = _olodConfigs.LastOrDefault(x => lodConfig.Agl >= x.Agl);
            var newLodConfig = new LodConfig() { Agl = (int)lodConfig.Agl, Lod = (int)lodConfig.Lod };

            if (targetLodConfig == null)
                _olodConfigs.AddFirst(newLodConfig);
            else
                _olodConfigs.AddAfter(_olodConfigs.Find(targetLodConfig), new LinkedListNode<LodConfig>(newLodConfig));

            RebindOLodGrid();
        }

        private void OLodDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;

            if (button?.DataContext is not LodConfig lodConfig)
                return;

            _olodConfigs.Remove(lodConfig);

            RebindOLodGrid();
        }

        private void RebindOLodGrid()
        {
            this.OlodGrid.ItemsSource = null;
            this.OlodGrid.ItemsSource = _olodConfigs.ToList();

            AddOlodConfigs.Clear();
            AddOlodConfigs.Add(new TempLodConfig());
        }

        private bool UpdateOlodDuplicate(TempLodConfig lodConfig)
        {
            var olodConfig = _olodConfigs.FirstOrDefault(x => x.Agl == lodConfig.Agl);

            if (olodConfig == null)
                return false;

            olodConfig.Lod = Convert.ToInt32(lodConfig.Lod);
            return true;
        }
    }

    public class TempLodConfig
    {
        public int? Agl { get; set; }

        public int? Lod { get; set; }
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
