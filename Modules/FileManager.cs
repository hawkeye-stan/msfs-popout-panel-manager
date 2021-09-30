using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public class FileManager
    {
        private static string StartupPath;

        static FileManager()
        {
            FileManager.StartupPath = Application.StartupPath;
        }

        public static List<PlaneProfile> ReadPlaneProfileData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(GetFilePathByType(FilePathType.ProfileData) + "planeprofile.json"))
                {
                    return JsonConvert.DeserializeObject<List<PlaneProfile>>(reader.ReadToEnd());
                }
            }
            catch
            {
                return new List<PlaneProfile>();
            }
        }

        public static UserData ReadUserData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(GetFilePathByType(FilePathType.UserData) + "userdata.json"))
                {
                    return JsonConvert.DeserializeObject<UserData>(reader.ReadToEnd());
                }
            }
            catch
            {
                var userData = new UserData();
                return userData;
            }
        }

        public static UserPlaneProfile GetUserPlaneProfile(int profileId)
        {
            var userData = ReadUserData();
     
            var userPlaneProfile = userData.Profiles.Find(x => x.ProfileId == profileId);

            if (userPlaneProfile == null)
            { 
                userPlaneProfile = new UserPlaneProfile();
                userPlaneProfile.ProfileId = profileId;
                userPlaneProfile.PanelSettings.PanelDestinationList = new List<PanelDestinationInfo>();
                userPlaneProfile.PanelSourceCoordinates = new List<PanelSourceCoordinate>();
            }

            return userPlaneProfile;
        }

        public static void WriteUserData(UserData userData)
        {
            using (StreamWriter file = File.CreateText(GetFilePathByType(FilePathType.UserData) + "userdata.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, userData);
            }
        }

        public static List<AnalysisData> ReadAnalysisTemplateData()
        {
            using (StreamReader reader = new StreamReader(GetFilePathByType(FilePathType.AnalysisData) + "analysisconfig.json"))
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<AnalysisData>>(reader.ReadToEnd());
                }
                catch(Exception ex)
                {
                    throw new Exception("The file analysisconfig.json is invalid.");
                }
            }
        }

        public static Stream LoadAsStream(FilePathType filePathType, string fileName)
        {
            try
            {
                var fullFilePath = GetFilePathByType(filePathType) + fileName;
                return new MemoryStream(File.ReadAllBytes(fullFilePath));
            }
            catch
            {
                Logger.LogStatus($"Unable to load file {fileName}");
                return null;
            }
        }

        public static Stream LoadAsStream(string fullFilePath)
        {
            return new MemoryStream(File.ReadAllBytes(fullFilePath));
        }

        public static void SaveFile(FilePathType filePathType, string fileName, MemoryStream memoryStream)
        {
            var folderPath = GetFilePathByType(filePathType);
            var fullFilePath = GetFilePathByType(filePathType) + fileName;

            Directory.CreateDirectory(folderPath);
            
            using (var file = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(file);
            }
        }

        public static List<string> GetFileNames(FilePathType filePathType, string subFolder, string filePrefix)
        {
            List<string> files = new List<string>();

            var folderPath = GetFilePathByType(filePathType);
            if (!String.IsNullOrEmpty(subFolder))
                folderPath += subFolder + @"\";

            string[] fileEntries = Directory.GetFiles(folderPath);
            foreach (string fileEntry in fileEntries)
            {
                var fileName = Path.GetFileName(fileEntry);

                if (!String.IsNullOrEmpty(filePrefix))
                {
                    if(fileName.StartsWith(filePrefix))
                        files.Add(fileEntry);
                }
                else
                    files.Add(fileEntry);
            }

            return files;
        }

        private static string GetFilePathByType(FilePathType filePathType)
        {
            switch (filePathType)
            {
                case FilePathType.PreprocessingData:
                    return StartupPath + @"\Config\PreprocessingData\";
                case FilePathType.AnalysisData:
                    return StartupPath + @"\Config\AnalysisData\";
                case FilePathType.ProfileData:
                case FilePathType.UserData:
                    return StartupPath + @"\Config\";
                default:
                    return StartupPath;
            }
        }
    }

    public enum FilePathType
    {
        PreprocessingData,
        AnalysisData,
        ProfileData,
        UserData,
        Default
    }
}
