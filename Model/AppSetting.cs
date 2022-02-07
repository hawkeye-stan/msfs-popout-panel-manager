using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace MSFSPopoutPanelManager.Model
{
    public class AppSetting : INotifyPropertyChanged
    {
        private const string APP_SETTING_DATA_FILENAME = "appsettingdata.json";

        private bool _saveEnabled;

        // Using PropertyChanged.Fody
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs<bool>> AlwaysOnTopChanged;
        public event EventHandler<EventArgs<bool>> AutoPopOutPanelsChanged;

        public AppSetting()
        {
            // Set defaults
            MinimizeToTray = false;
            AlwaysOnTop = true;
            UseAutoPanning = true;
            StartMinimized = false;
            IncludeBuiltInPanel = false;
            AutoPopOutPanels = false;
            AutoPopOutPanelsWaitDelay = new AutoPopOutPanelsWaitDelay();
        }

        public void Load()
        {
            var appSetting = ReadAppSetting();
            this.MinimizeToTray = appSetting.MinimizeToTray;
            this.AlwaysOnTop = appSetting.AlwaysOnTop;
            this.UseAutoPanning = appSetting.UseAutoPanning;
            this.StartMinimized = appSetting.StartMinimized;
            this.IncludeBuiltInPanel = appSetting.IncludeBuiltInPanel;
            this.AutoPopOutPanels = appSetting.AutoPopOutPanels;
            this.AutoPopOutPanelsWaitDelay = appSetting.AutoPopOutPanelsWaitDelay;

            _saveEnabled = true;
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            // Automatic save data
            if (_saveEnabled && propertyName != "AutoStart" && before != after)
                WriteAppSetting(this);

            switch (propertyName)
            {
                case "AlwaysOnTop":
                    AlwaysOnTopChanged?.Invoke(this, new EventArgs<bool>((bool)after));
                    break;
                case "AutoPopOutPanels":
                    AutoPopOutPanelsChanged?.Invoke(this, new EventArgs<bool>((bool)after));
                    break;
            }
        }

        //[OnDeserialized]
        //private void OnDeserialized(StreamingContext context)
        //{
        //    // Allow save data
        //    _saveEnabled = true;
        //}

        public bool MinimizeToTray { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool UseAutoPanning { get; set; }

        public bool StartMinimized { get; set; }

        public bool IncludeBuiltInPanel { get; set; }

        public bool AutoPopOutPanels { get; set; }

        public AutoPopOutPanelsWaitDelay AutoPopOutPanelsWaitDelay { get; set; }

        [JsonIgnore]
        public bool AutoStart
        {
            get
            {
                return AppAutoStart.CheckIsAutoStart();
            }
            set 
            {
                if (value)
                    AppAutoStart.Activate();
                else
                    AppAutoStart.Deactivate();
            }
        }

        public AppSetting ReadAppSetting()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME)))
                {
                    return JsonConvert.DeserializeObject<AppSetting>(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                // if file does not exist, write default data
                var appSetting = new AppSetting();
                WriteAppSetting(appSetting);
                return appSetting;
            }
        }

        public void WriteAppSetting(AppSetting appSetting)
        {
            try
            {
                var userProfilePath = FileIo.GetUserDataFilePath();
                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, APP_SETTING_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, appSetting);
                }
            }
            catch
            {
                Logger.LogStatus($"Unable to write app setting data file: {APP_SETTING_DATA_FILENAME}", StatusMessageType.Error);
            }
        }
    }

    public class AutoPopOutPanelsWaitDelay
    {
        public AutoPopOutPanelsWaitDelay()
        {
            ReadyToFlyButton = 4;
            InitialCockpitView = 2;
            InstrumentationPowerOn = 2;
        }

        public int ReadyToFlyButton { get; set; }

        public int InitialCockpitView { get; set; }

        public int InstrumentationPowerOn { get; set; }
    }
}
