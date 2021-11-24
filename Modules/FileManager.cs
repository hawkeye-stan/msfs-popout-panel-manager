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

        public static List<PlaneProfile> ReadAllPlaneProfileData()
        {
            List<PlaneProfile> allProfiles = new List<PlaneProfile>();
            allProfiles.AddRange(FileManager.ReadBuiltInPlaneProfileData());
            allProfiles.AddRange(FileManager.ReadCustomPlaneProfileData());

            return allProfiles;
        }

        public static List<PlaneProfile> ReadBuiltInPlaneProfileData()
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

        public static List<PlaneProfile> ReadCustomPlaneProfileData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(GetFilePathByType(FilePathType.ProfileData) + "customplaneprofile.json"))
                {
                    return JsonConvert.DeserializeObject<List<PlaneProfile>>(reader.ReadToEnd());
                }
            }
            catch
            {
               return new List<PlaneProfile>();
            }
        }

        public static bool WriteBuiltInPlaneProfileData(List<PlaneProfile> profiles)
        {
            try
            {
                using (StreamWriter file = File.CreateText(GetFilePathByType(FilePathType.ProfileData) + "planeprofile.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, profiles);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WriteCustomPlaneProfileData(List<PlaneProfile> profiles)
        {
            try
            {
                using (StreamWriter file = File.CreateText(GetFilePathByType(FilePathType.ProfileData) + "customplaneprofile.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, profiles);
                }

                return true;
            }
            catch
            {
                return false;
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

        public static List<AnalysisData> ReadAllAnalysisTemplateData()
        {
            List<AnalysisData> allTemplates = new List<AnalysisData>();
            allTemplates.AddRange(FileManager.ReadBuiltInAnalysisTemplateData());
            allTemplates.AddRange(FileManager.ReadCustomAnalysisTemplateData());

            return allTemplates;
        }

        public static List<AnalysisData> ReadBuiltInAnalysisTemplateData()
        {
            try
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
            catch
            {
                return new List<AnalysisData>();
            }
        }

        public static List<AnalysisData> ReadCustomAnalysisTemplateData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(GetFilePathByType(FilePathType.AnalysisData) + "customanalysisconfig.json"))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<AnalysisData>>(reader.ReadToEnd());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("The file customanalysisconfig.json is invalid.");
                    }
                }
            }
            catch
            {
                return new List<AnalysisData>();
            }
            
        }

        public static void WriteCustomAnalysisTemplateData(List<AnalysisData> analysisDataList)
        {
            using (StreamWriter file = File.CreateText(GetFilePathByType(FilePathType.AnalysisData) + "customanalysisconfig.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, analysisDataList);
            }
        }

        public static void RemoveCustomAnalysisTemplate(string analysisTemplateName)
        {
            try
            {
                var templates = ReadCustomAnalysisTemplateData();
                var template = templates.Find(x => x.TemplateName == analysisTemplateName);

                if (template != null)
                {
                    var fullFilePath = GetFilePathByType(FilePathType.AnalysisData) + template.TemplateImagePath;
                    Directory.Delete(fullFilePath, true);
                }
            }
            catch
            {

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

        public static void SaveFile(FilePathType filePathType, string subFolder, string fileName, MemoryStream memoryStream)
        {
            subFolder = String.IsNullOrEmpty(subFolder) ? String.Empty : subFolder + @"\";

            var folderPath = GetFilePathByType(filePathType) + subFolder;
            var fullFilePath = folderPath + fileName;

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

        public static string GetFilePathByType(FilePathType filePathType)
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
