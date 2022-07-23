using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PanelConfigurationViewModel : ObservableObject
    {
        private MainOrchestrator _orchestrator;

        public PanelConfigurationViewModel(MainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;

            LockPanelsCommand = new DelegateCommand(OnLockPanelsUpdated);

            PanelConfigUpdatedCommand = new DelegateCommand<object>(OnPanelConfigUpdated);

            MinusTenPixelCommand = new DelegateCommand<object>(OnDataItemIncDec, (obj) => NumericDataPointTextBox != null && ProfileData.HasActiveProfile && !ProfileData.ActiveProfile.IsLocked).ObservesProperty(() => SelectedPanelConfigItem).ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);

            MinusOnePixelCommand = new DelegateCommand<object>(OnDataItemIncDec, (obj) => NumericDataPointTextBox != null && ProfileData.HasActiveProfile && !ProfileData.ActiveProfile.IsLocked).ObservesProperty(() => SelectedPanelConfigItem).ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);

            PlusOnePixelCommand = new DelegateCommand<object>(OnDataItemIncDec, (obj) => NumericDataPointTextBox != null && ProfileData.HasActiveProfile && !ProfileData.ActiveProfile.IsLocked).ObservesProperty(() => SelectedPanelConfigItem).ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);

            PlusTenPixelCommand = new DelegateCommand<object>(OnDataItemIncDec, (obj) => NumericDataPointTextBox != null && ProfileData.HasActiveProfile && !ProfileData.ActiveProfile.IsLocked).ObservesProperty(() => SelectedPanelConfigItem).ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);
        }

        public DelegateCommand LockPanelsCommand { get; private set; }

        public DelegateCommand<object> PanelConfigUpdatedCommand { get; private set; }

        public DelegateCommand<object> MinusTenPixelCommand { get; private set; }

        public DelegateCommand<object> MinusOnePixelCommand { get; private set; }

        public DelegateCommand<object> PlusOnePixelCommand { get; private set; }

        public DelegateCommand<object> PlusTenPixelCommand { get; private set; }

        public ProfileData ProfileData { get { return _orchestrator.ProfileData; } }

        public PanelConfigItem SelectedPanelConfigItem { get; set; }

        public TextBox NumericDataPointTextBox { get; set; }

        private void OnLockPanelsUpdated()
        {
            if (ProfileData.ActiveProfile == null)
                return;

            if (!ProfileData.ActiveProfile.IsLocked)
            {
                _orchestrator.PanelConfiguration.LockStatusUpdated();
            }
            else if (DialogHelper.ConfirmDialog("Confirm Unlock Panels", "Are you sure you want to unlock all panels to make changes?"))
            {
                _orchestrator.PanelConfiguration.LockStatusUpdated();
            }
        }

        private void OnPanelConfigUpdated(object commandParameter)
        {
            var panelConfigItem = commandParameter as PanelConfigItem;
            if (panelConfigItem != null)
                _orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(panelConfigItem.PanelIndex, panelConfigItem.PanelConfigProperty);
        }

        private void OnDataItemIncDec(object commandParameter)
        {
            _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelIndex, SelectedPanelConfigItem.PanelConfigProperty, Convert.ToInt32(commandParameter));

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (NumericDataPointTextBox != null)
                    NumericDataPointTextBox.Focus();
            });
        }
    }
}