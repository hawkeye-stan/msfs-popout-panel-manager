using MSFSPopoutPanelManager.DomainModel.Profile;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.DomainModel.DataFile
{
    public class UserProfileFile
    {
        public UserProfileFile()
        {
            FileVersion = "4.0";
            Profiles = new List<UserProfile>();
        }

        public string FileVersion { get; set; }

        public IList<UserProfile> Profiles { get; set; }
    }
}
