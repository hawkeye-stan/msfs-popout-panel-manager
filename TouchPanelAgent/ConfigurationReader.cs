using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSFSPopoutPanelManager.TouchPanelAgent
{
    internal class ConfigurationReader
    {
        public static List<PlaneProfileInfo> GetPlaneProfilesConfiguration()
        {
            try
            {
                string filePath;

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    filePath = Path.Combine(GetSolutionPath(), $@"ReactClient\public\config\PlanePanelProfile.json");
                else
                    filePath = @"ReactClient\config\PlanePanelProfile.json";

                using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, filePath)))
                {
                    return JsonConvert.DeserializeObject<List<PlaneProfileInfo>>(reader.ReadToEnd());
                }
            }
            catch
            {
                FileLogger.WriteException("PlanePanelProfile.json file is not found or is invalid.", null);
                return new List<PlaneProfileInfo>();
            }
        }

        private static string GetSolutionPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var path = currentDir.Substring(0, currentDir.IndexOf("WpfApp") - 1);

            return path;
        }
    }
}