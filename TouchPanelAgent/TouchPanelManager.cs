using System.Collections.Generic;

namespace MSFSPopoutPanelManager.TouchPanelAgent
{
    public class TouchPanelManager
    {
        public static List<PlaneProfileInfo> GetPlaneProfiles()
        {
            return ConfigurationReader.GetPlaneProfilesConfiguration();
        }
    }
}
