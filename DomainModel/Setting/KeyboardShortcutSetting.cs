using System.Collections.Generic;
using System.Linq;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class KeyboardShortcutSetting : ObservableObject
    {
        private string _startPopOutKeyBindingLegacyConversion;

        public bool IsEnabled { get; set; } = true;

        public string StartPopOutKeyBinding
        {
            get => _startPopOutKeyBindingLegacyConversion;
            set
            {
                // Convert legacy start pop out keyboard binding to new keyboard bindings (v4.0.3 and earlier)

                if (string.IsNullOrEmpty(value)) 
                    return;

                var keys = new List<string>() { "LeftCtrl", "LeftShift", value.ToUpper() };
                keys = keys.OrderBy(x => x).ToList();
                PopOutKeyboardBinding = string.Join("|", keys);

                _startPopOutKeyBindingLegacyConversion = null;
            }
        }

        public string PopOutKeyboardBinding { get; set; }

        [JsonIgnore]
        public bool IsDetectingKeystroke { get; set; }
    }
}
