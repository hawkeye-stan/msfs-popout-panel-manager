using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using PropertyChanged;
using System;
using System.Windows;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    [SuppressPropertyChangedWarnings]
    public class ApplicationViewModel : BaseViewModel
    {
        private readonly AppOrchestrator _appOrchestrator;

        public WindowState InitialWindowState { get; private set; }

        public ApplicationViewModel(SharedStorage sharedStorage, AppOrchestrator appOrchestrator) : base(sharedStorage)
        {
            _appOrchestrator = appOrchestrator;
        }

        public void Initialize()
        {
            // Set title bar color
            WindowActionManager.SetWindowTitleBarColor(ApplicationHandle, "303030");

            _appOrchestrator.Initialize();
            AppSettingData.OnAlwaysOnTopChanged += (_, e) => WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, e);

            // Set window state
            if (AppSettingData.ApplicationSetting.GeneralSetting.StartMinimized)
                InitialWindowState = WindowState.Minimized;

            // Set Always on Top
            if (AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop);
        }

        public void WindowClosing()
        {
            _appOrchestrator.ApplicationClose();

            if (Application.Current != null)
                Environment.Exit(0);
        }
    }
}
