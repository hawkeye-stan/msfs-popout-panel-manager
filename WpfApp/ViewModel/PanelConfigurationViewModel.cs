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

            ConfigurePanelPixelCommand = new DelegateCommand<object>(OnDataItemIncDec, (obj) => NumericDataPointTextBox != null && ProfileData.HasActiveProfile && !ProfileData.ActiveProfile.IsLocked).ObservesProperty(() => SelectedPanelConfigItem).ObservesProperty(() => ProfileData.ActiveProfile.IsLocked);

            ConfigurePanelCommand = new DelegateCommand<object>(OnConfigurePanel, (obj) => true);
        }

        public DelegateCommand LockPanelsCommand { get; private set; }

        public DelegateCommand<object> PanelConfigUpdatedCommand { get; private set; }

        public DelegateCommand<object> ConfigurePanelPixelCommand { get; private set; }

        public DelegateCommand<object> ConfigurePanelCommand { get; private set; }

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
                _orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(panelConfigItem.PanelHandle, panelConfigItem.PanelConfigProperty);
        }

        private void OnDataItemIncDec(object commandParameter)
        {
            _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, SelectedPanelConfigItem.PanelConfigProperty, Convert.ToInt32(commandParameter));

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (NumericDataPointTextBox != null)
                    NumericDataPointTextBox.Focus();
            });
        }

        private void OnConfigurePanel(object commandParameter)
        {
            if (SelectedPanelConfigItem.PanelHandle == IntPtr.Zero)
                return;

            var keyAction = commandParameter as KeyAction;

            switch (keyAction.Action)
            {
                case "Up":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Top, -1 * keyAction.Multiplier);
                    break;
                case "Down":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Top, 1 * keyAction.Multiplier);
                    break;
                case "Left":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Left, -1 * keyAction.Multiplier);
                    break;
                case "Right":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Left, 1 * keyAction.Multiplier);
                    break;
                case "Control-Up":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Height, -1 * keyAction.Multiplier);
                    break;
                case "Control-Down":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Height, 1 * keyAction.Multiplier);
                    break;
                case "Control-Left":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Width, -1 * keyAction.Multiplier);
                    break;
                case "Control-Right":
                    _orchestrator.PanelConfiguration.PanelConfigIncreaseDecrease(SelectedPanelConfigItem.PanelHandle, PanelConfigPropertyName.Width, 1 * keyAction.Multiplier);
                    break;
            }
        }
    }

    public class KeyAction
    {
        public KeyAction(string action, int multiplier)
        {
            Action = action;
            Multiplier = multiplier;
        }

        public KeyAction(string action)
        {
            Action = action;
            Multiplier = 1;
        }

        public string Action { get; set; }

        public int Multiplier { get; set; }
    }
}