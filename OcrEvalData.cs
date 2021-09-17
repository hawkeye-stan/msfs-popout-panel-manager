using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class OcrEvalData
    {
        public string Profile { get; set; }

        public bool DefaultProfile { get; set; }

        // scale the image so OCR can better recognize the text
        public double OCRImageScale { get; set; }

        public List<PopoutEvalData> EvalData { get; set; }
    }

    public class PopoutEvalData
    {
        public string PopoutName { get; set; }

        public List<string> Data { get; set; }
    }
}
