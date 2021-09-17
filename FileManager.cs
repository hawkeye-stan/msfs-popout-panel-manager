using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSFSPopoutPanelManager
{
    public class FileManager
    {
        public static UserData ReadUserData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(@".\config\userdata.json"))
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

        public static void WriteUserData(UserData userData)
        {
            using (StreamWriter file = File.CreateText(@".\config\userdata.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, userData);
            }
        }

        public static List<OcrEvalData> ReadProfileData()
        {
            using (StreamReader reader = new StreamReader(@".\config\ocrdata.json"))
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
