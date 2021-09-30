using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class PlaneProfile
    {
        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public string AnalysisTemplateName { get; set; }
    }

    public class AnalysisData
    {
        public string TemplateName { get; set; }

        public List<Template> Templates { get; set; }
    }

    public class Template
    {
        public int PopoutId { get; set; }

        public string PopoutName { get; set; }

        public List<string> ImagePaths { get; set; }
    }
}
