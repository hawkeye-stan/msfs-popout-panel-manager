using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Windows.Input;
using MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PopOutPanelConfigCardViewModel : BaseViewModel
    {
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;

        public PanelConfig DataItem { get; set; }

        public ICommand MovePanelUpCommand { get; set; }

        public ICommand MovePanelDownCommand { get; set; }

        public ICommand DeletePanelCommand { get; set; }

        public ICommand TouchEnabledCommand { get; set; }

        public ICommand MoveResizePanelCommand { get; set; }

        public ICommand DetectFloatPanelKeyBindingCommand { get; set; }

        public DelegateCommand<string> PanelAttributeUpdatedCommand { get; set; }

        public PopOutPanelConfigCardViewModel(SharedStorage sharedStorage, PanelSourceOrchestrator panelSourceOrchestrator, PanelConfigurationOrchestrator panelConfigurationOrchestrator) : base(sharedStorage)
        {
            _panelSourceOrchestrator = panelSourceOrchestrator;
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;

            DataItem = new PanelConfig();

            MovePanelUpCommand = new DelegateCommand(OnMovePanelUp, () => ActiveProfile != null && ActiveProfile.PanelConfigs.IndexOf(DataItem) > 0)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count);

            MovePanelDownCommand = new DelegateCommand(OnMovePanelDown, () => ActiveProfile != null && ActiveProfile.PanelConfigs.IndexOf(DataItem) < ActiveProfile.PanelConfigs.Count - 1)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs)
                                                                                .ObservesProperty(() => ActiveProfile.PanelConfigs.Count);


            MoveResizePanelCommand = new DelegateCommand(OnMoveResizePanel, () => ActiveProfile != null
                                                                                && DataItem != null
                                                                                && (ActiveProfile.CurrentMoveResizePanelId == DataItem.Id || ActiveProfile.CurrentMoveResizePanelId == Guid.Empty)
                                                                                && FlightSimData.IsInCockpit
                                                                                && DataItem.IsPopOutSuccess != null
                                                                                && DataItem.PanelHandle != IntPtr.Zero
                                                                                && !DataItem.FullScreen)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.CurrentMoveResizePanelId)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => DataItem.IsPopOutSuccess)
                                                                                .ObservesProperty(() => DataItem.PanelHandle)
                                                                                .ObservesProperty(() => DataItem.FullScreen);

            TouchEnabledCommand = new DelegateCommand(OnTouchEnabled, () => ActiveProfile != null)
                                                                                .ObservesProperty(() => ActiveProfile);

            DeletePanelCommand = new DelegateCommand(OnDeletePanel);


            PanelAttributeUpdatedCommand = new DelegateCommand<string>(OnPanelAttributeUpdated);

            DetectFloatPanelKeyBindingCommand = new DelegateCommand(OnDetectFloatPanelKeyBinding);
        }
        
        private void OnMovePanelUp()
        {
            var index = ActiveProfile.PanelConfigs.IndexOf(DataItem);
            ActiveProfile.PanelConfigs.Insert(index - 1, DataItem);
            ActiveProfile.PanelConfigs.RemoveAt(index + 1);
        }

        private void OnMovePanelDown()
        {
            var index = ActiveProfile.PanelConfigs.IndexOf(DataItem);
            ActiveProfile.PanelConfigs.Insert(index + 2, DataItem);
            ActiveProfile.PanelConfigs.RemoveAt(index);
        }

        private async void OnDeletePanel()
        {
            var result = await DialogHost.Show(new ConfirmationDialog("Are you sure you want to delete the panel?", "Delete"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
                _panelSourceOrchestrator.RemovePanelSource(DataItem);
        }

        private void OnTouchEnabled()
        {
            if (DataItem != null)
                _panelConfigurationOrchestrator.PanelConfigPropertyUpdated(DataItem.PanelHandle, PanelConfigPropertyName.TouchEnabled);
        }

        private void OnPanelAttributeUpdated(string commandParameter)
        {
            if (DataItem != null && commandParameter != null)
                _panelConfigurationOrchestrator.PanelConfigPropertyUpdated(DataItem.PanelHandle, (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), commandParameter));
        }

        private void OnMoveResizePanel()
        {
            if (DataItem.IsEditingPanel)
            {
                ActiveProfile.CurrentMoveResizePanelId = DataItem.Id;

                InputHookManager.StartKeyboardHook();
                InputHookManager.OnKeyUp -= HandleKeyUpEvent;
                InputHookManager.OnKeyUp += HandleKeyUpEvent;
            }
            else
            {
                ActiveProfile.CurrentMoveResizePanelId = Guid.Empty;
                InputHookManager.OnKeyUp -= HandleKeyUpEvent;
                InputHookManager.EndKeyboardHook();
            }
        }

        private void HandleKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            PanelConfigPropertyName panelConfigPropertyName = PanelConfigPropertyName.None;

            if (e.IsHoldControl)
                e.KeyCode = $"CTRL+{e.KeyCode}";

            if (e.IsHoldShift)
                e.KeyCode = $"SHFT+{e.KeyCode}";

            switch (e.KeyCode.ToUpper())
            {
                case "UP":
                    panelConfigPropertyName = PanelConfigPropertyName.Top;
                    DataItem.Top -= 10;
                    break;
                case "DOWN":
                    panelConfigPropertyName = PanelConfigPropertyName.Top;
                    DataItem.Top += 10;
                    break;
                case "LEFT":
                    panelConfigPropertyName = PanelConfigPropertyName.Left;
                    DataItem.Left -= 10;
                    break;
                case "RIGHT":
                    panelConfigPropertyName = PanelConfigPropertyName.Left;
                    DataItem.Left += 10;
                    break;
                case "SHFT+UP":
                    panelConfigPropertyName = PanelConfigPropertyName.Top;
                    DataItem.Top -= 1;
                    break;
                case "SHFT+DOWN":
                    panelConfigPropertyName = PanelConfigPropertyName.Top;
                    DataItem.Top += 1;
                    break;
                case "SHFT+LEFT":
                    panelConfigPropertyName = PanelConfigPropertyName.Left;
                    DataItem.Left -= 1;
                    break;
                case "SHFT+RIGHT":
                    panelConfigPropertyName = PanelConfigPropertyName.Left;
                    DataItem.Left += 1;
                    break;
                case "CTRL+UP":
                    panelConfigPropertyName = PanelConfigPropertyName.Height;
                    DataItem.Height -= 10;
                    break;
                case "CTRL+DOWN":
                    panelConfigPropertyName = PanelConfigPropertyName.Height;
                    DataItem.Height += 10;
                    break;
                case "CTRL+LEFT":
                    panelConfigPropertyName = PanelConfigPropertyName.Width;
                    DataItem.Width -= 10;
                    break;
                case "CTRL+RIGHT":
                    panelConfigPropertyName = PanelConfigPropertyName.Width;
                    DataItem.Width += 10;
                    break;
                case "SHFT+CTRL+UP":
                    panelConfigPropertyName = PanelConfigPropertyName.Height;
                    DataItem.Height -= 1;
                    break;
                case "SHFT+CTRL+DOWN":
                    panelConfigPropertyName = PanelConfigPropertyName.Height;
                    DataItem.Height += 1;
                    break;
                case "SHFT+CTRL+LEFT":
                    panelConfigPropertyName = PanelConfigPropertyName.Width;
                    DataItem.Width -= 1;
                    break;
                case "SHFT+CTRL+RIGHT":
                    panelConfigPropertyName = PanelConfigPropertyName.Width;
                    DataItem.Width += 1;
                    break;
            }

            if (panelConfigPropertyName != PanelConfigPropertyName.None)
                _panelConfigurationOrchestrator.PanelConfigPropertyUpdated(DataItem.PanelHandle, panelConfigPropertyName);
        }

        private void OnDetectFloatPanelKeyBinding()
        {
            _panelConfigurationOrchestrator.StartDetectKeystroke(DataItem.Id);
        }
    }
}
