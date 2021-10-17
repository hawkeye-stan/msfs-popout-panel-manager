using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class PlaneProfile
    {
        public PlaneProfile()
        {
            ProfileId = 1000;
            IsUserProfile = false;
            AnalysisTemplateName = "none";
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public string AnalysisTemplateName { get; set; }

        public bool IsUserProfile { get; set; }
    }

    public class AnalysisData
    {
        public AnalysisData()
        {
            Templates = new List<Template>();
            IsUserTemplate = false;
        }

        public string TemplateName { get; set; }

        public bool IsUserTemplate { get; set; }

        public List<Template> Templates { get; set; }

        public string TemplateImagePath
        {
            get
            {
                return TemplateName.Replace(" ", "_").Replace("/", "_").Replace(@"\", "_");
            }
        }
    }

    public class Template
    {
        public Template()
        {
            ImagePaths = new List<string>();
        }

        public int PopoutId { get; set; }

        public string PopoutName { get; set; }

        public List<string> ImagePaths { get; set; }
    }
}
