using System;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class FloatingPanel : ObservableObject
    {
        public FloatingPanel()
        {
            PropertyChanged += FloatingPanel_PropertyChanged;
        }

        private void FloatingPanel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var arg = e as PropertyChangedExtendedEventArgs;

            if (arg?.PropertyName != nameof(IsEnabled) || IsEnabled)
                return;

            KeyboardBinding = null;
            IsHiddenOnStart = false;
        }

        public bool IsEnabled { get; set; }

        public string KeyboardBinding { get; set; }

        public bool IsHiddenOnStart { get; set; }

        [JsonIgnore]
        public bool IsDetectingKeystroke { get; set; }


        [JsonIgnore]
        public bool HasKeyboardBinding => !string.IsNullOrEmpty(KeyboardBinding);
    }
}
