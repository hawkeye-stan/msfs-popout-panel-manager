using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Linq;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PopOutPanelCardViewModel : BaseViewModel
    {
        public PanelConfig DataItem { get; set; }

        public ICommand MovePanelUpCommand { get; set; }

        public ICommand MovePanelDownCommand { get; set; }

        public ICommand DeletePanelCommand { get; set; }

        public ICommand AddPanelSourceLocationCommand { get; set; }

        public ICommand TouchEnabledCommand { get; set; }

        public ICommand MoveResizePanelCommand { get; set; }

        public DelegateCommand<string> PanelAttributeUpdatedCommand { get; set; }

        public PopOutPanelCardViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            DataItem = new PanelConfig();

            MovePanelUpCommand = new DelegateCommand(OnMovePanelUp, () => ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelConfigs.IndexOf(DataItem) > 0)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count);

            MovePanelDownCommand = new DelegateCommand(OnMovePanelDown, () => ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelConfigs.IndexOf(DataItem) < ProfileData.ActiveProfile.PanelConfigs.Count - 1)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count);

            AddPanelSourceLocationCommand = new DelegateCommand(OnAddPanelSourceLocation, () => ProfileData.ActiveProfile != null && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            MoveResizePanelCommand = new DelegateCommand(OnMoveResizePanel, () => ProfileData.ActiveProfile != null
                                                                                && DataItem != null
                                                                                && (ProfileData.ActiveProfile.CurrentMoveResizePanelId == DataItem.Id || ProfileData.ActiveProfile.CurrentMoveResizePanelId == Guid.Empty)
                                                                                && FlightSimData.IsInCockpit
                                                                                && DataItem.IsPopOutSuccess != null
                                                                                && DataItem.PanelHandle != IntPtr.Zero
                                                                                && !DataItem.FullScreen)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile.CurrentMoveResizePanelId)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => DataItem.IsPopOutSuccess)
                                                                                .ObservesProperty(() => DataItem.PanelHandle)
                                                                                .ObservesProperty(() => DataItem.FullScreen);

            TouchEnabledCommand = new DelegateCommand(OnTouchEnabled, () => ProfileData.ActiveProfile != null)
                                                                                .ObservesProperty(() => ProfileData.ActiveProfile);

            DeletePanelCommand = new DelegateCommand(OnDeletePanel);
            PanelAttributeUpdatedCommand = new DelegateCommand<string>(OnPanelAttributeUpdated);

            IsEnabledEditPanelSource = true;
        }

        public int DataItemIndex => ProfileData.ActiveProfile.PanelConfigs.IndexOf(DataItem);

        public int DataItemsMaxIndex => ProfileData.ActiveProfile.PanelConfigs.Count - 1;

        public bool IsEnabledEditPanelSource { get; set; }

        public bool IsErrorPanel => DataItem.IsPopOutSuccess != null && !(bool)DataItem.IsPopOutSuccess;

        private void OnMovePanelUp()
        {
            var index = ProfileData.ActiveProfile.PanelConfigs.IndexOf(DataItem);
            ProfileData.ActiveProfile.PanelConfigs.Insert(index - 1, DataItem);
            ProfileData.ActiveProfile.PanelConfigs.RemoveAt(index + 1);
        }

        private void OnMovePanelDown()
        {
            var index = ProfileData.ActiveProfile.PanelConfigs.IndexOf(DataItem);
            ProfileData.ActiveProfile.PanelConfigs.Insert(index + 2, DataItem);
            ProfileData.ActiveProfile.PanelConfigs.RemoveAt(index);
        }

        private async void OnDeletePanel()
        {
            var result = await DialogHost.Show(new ConfirmationDialog("Are you sure you want to delete the panel?", "Delete"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
                Orchestrator.PanelSource.RemovePanelSource(DataItem);

            if (ProfileData.ActiveProfile.PanelConfigs.Where(p => p.PanelType == PanelType.CustomPopout).Count() == 0 && ProfileData.ActiveProfile.IsEditingPanelSource)
                ProfileData.ActiveProfile.IsEditingPanelSource = false;
        }

        private void OnTouchEnabled()
        {
            if (DataItem != null)
                Orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(DataItem.PanelHandle, PanelConfigPropertyName.TouchEnabled);
        }

        private void OnPanelAttributeUpdated(string? commandParameter)
        {
            if (DataItem != null && commandParameter != null)
                Orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(DataItem.PanelHandle, (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), commandParameter));
        }

        private async void OnAddPanelSourceLocation()
        {
            Orchestrator.PanelSource.StartPanelSelectionEvent();

            DataItem.IsSelectedPanelSource = true;
            if (!ProfileData.ActiveProfile.IsEditingPanelSource)
                await Orchestrator.PanelSource.StartEditPanelSources();

            Orchestrator.PanelSource.StartPanelSelection(DataItem);
        }

        private void OnMoveResizePanel()
        {
            if (DataItem.IsEditingPanel)
            {
                ProfileData.ActiveProfile.CurrentMoveResizePanelId = DataItem.Id;
                InputHookManager.StartKeyboardHook();
                InputHookManager.OnKeyUp -= HandleKeyUpEvent;
                InputHookManager.OnKeyUp += HandleKeyUpEvent;
            }
            else
            {
                ProfileData.ActiveProfile.CurrentMoveResizePanelId = Guid.Empty;
                InputHookManager.OnKeyUp -= HandleKeyUpEvent;
                InputHookManager.EndKeyboardHook();
            }
        }

        private void HandleKeyUpEvent(object? sender, KeyUpEventArgs e)
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
                Orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(DataItem.PanelHandle, panelConfigPropertyName);
        }
    }
}
