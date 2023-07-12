using MSFSPopoutPanelManager.DomainModel.Setting;

namespace MSFSPopoutPanelManager.DomainModel.DataFile
{
    public class AppSetttingFile
    {
        public AppSetttingFile()
        {
            FileVersion = "4.0";
            ApplicationSetting = new ApplicationSetting();
        }

        public string FileVersion { get; set; }

        public ApplicationSetting ApplicationSetting { get; set; }
    }


}
