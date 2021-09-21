using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class UserData
    {
        public UserData()
        {
            Profiles = new List<Profile>();
        }

        public List<Profile> Profiles { get; set; }
    }

    public class Profile
    {
        public Profile()
        {
            PopoutNames = new Dictionary<string, Rect>();
        }

        public string Name { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HidePanelTitleBar { get; set; }

        public Dictionary<string, Rect> PopoutNames;
    }
}
