using MSFSPopoutPanelManager.DomainModel.Profile;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.DomainModel.DataFile
{
    public class UserProfileFile
    {
        public string FileVersion { get; set; } = "4.0";

        public IList<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    }
}
