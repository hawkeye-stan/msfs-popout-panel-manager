﻿using System;
using System.Linq;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class SwitchWindowViewModel : BaseViewModel
    {
        public DelegateCommand<string> ButtonCommand { get; private set; }

        public Guid PanelId { get; set; }

        public PanelConfig PanelConfig => ActiveProfile.PanelConfigs.FirstOrDefault(p => p.Id == PanelId);


        public SwitchWindowViewModel(SharedStorage sharedStorage) : base(sharedStorage)
        {
            ButtonCommand = new DelegateCommand<string>(OnButtonActivated);
        }

        private void OnButtonActivated(string commandParameter)
        {
            switch (commandParameter)
            {
                case "VerticalPanel":
                    PInvoke.SwitchToThisWindow(PInvoke.GetWindowHandle("737 Instruments Vertical"), true);
                    break;
                case "OverheadPanel":
                    PInvoke.SwitchToThisWindow(PInvoke.GetWindowHandle("737 Instruments Overhead"), true);
                    break;
            }
        }
    }
}
