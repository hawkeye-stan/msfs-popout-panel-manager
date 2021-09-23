using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSFSPopoutPanelManager
{
    public class FileManager
    {
        private string _startupPath;

        public FileManager(string startupPath)
        {
            _startupPath = startupPath;
        }

        public UserData ReadUserData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(_startupPath + @"\config\userdata.json"))
                {
                    string json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<UserData>(json);
                }
            }
            catch
            {
                return null;
            }
        }

        public void WriteUserData(UserData userData)
        {
            using (StreamWriter file = File.CreateText(_startupPath + @"\config\userdata.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, userData);
            }
        }

        public List<OcrEvalData> ReadProfileData()
        {
            using (StreamReader reader = new StreamReader(_startupPath + @"\config\ocrdata.json"))
            {
                string json = reader.ReadToEnd();

                try
                {
                    return JsonConvert.DeserializeObject<List<OcrEvalData>>(json);
                }
                catch
                {
                    throw new Exception("The file ocrdata.json is invalid.");
                }
            }
        }
    }
}
