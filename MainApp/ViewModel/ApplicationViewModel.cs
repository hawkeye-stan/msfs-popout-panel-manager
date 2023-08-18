using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    [SuppressPropertyChangedWarnings]
    public class ApplicationViewModel : BaseViewModel
    {
        public IntPtr ApplicationHandle
        {
            get => Orchestrator.ApplicationHandle;
            set => Orchestrator.ApplicationHandle = value;
        }

        public Window ApplicationWindow { set => Orchestrator.ApplicationWindow = value; }

        public WindowState InitialWindowState { get; private set; }

        public ApplicationViewModel(MainOrchestrator orchestrator) : base(orchestrator) { }

        public void Initialize()
        {
            // Set title bar color
            WindowActionManager.SetWindowTitleBarColor(ApplicationHandle, "303030");

            Orchestrator.Initialize();
            Orchestrator.AppSettingData.AlwaysOnTopChanged += (sender, e) => WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, e);

            // Set window state
            if (Orchestrator.AppSettingData.ApplicationSetting.GeneralSetting.StartMinimized)
                InitialWindowState = WindowState.Minimized;

            // Set Always on Top
            if (Orchestrator.AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, Orchestrator.AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop);


        }

        public void WindowClosing()
        {
            Orchestrator.ApplicationClose();

            if (Application.Current != null)
                Environment.Exit(0);
        }
    }
}
