using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSFSPopoutPanelManager.TouchPanel.Shared
{
    public class ConfigurationReader
    {
        public static List<PlaneProfileInfo> GetPlaneProfilesConfiguration()
        {
            try
            {
                string filePath;

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    filePath = Path.Combine(GetSolutionPath(), $@"TouchPanel\ReactClient\public\config\PlanePanelProfile.json");
                else
                    filePath = @"ReactClient\config\PlanePanelProfile.json";

                using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, filePath)))
                {
                    return JsonConvert.DeserializeObject<List<PlaneProfileInfo>>(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog(ex.Message, LogLevel.ERROR);
                TouchPanelLogger.ServerLog("PlanePanelProfile.json file is not found or is invalid.", LogLevel.ERROR);
                return new List<PlaneProfileInfo>();
            }
        }

        public static List<SimConnectDataDefinition> GetSimConnectDataDefinitions(string planeId)
        {
            try
            {
                string filePath;
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    filePath = Path.Combine(GetSolutionPath(), @$"TouchPanel\ReactClient\public\config\profiles\{planeId}\SimConnectDataDefinition.json");
                else
                    filePath = @$"ReactClient\config\profiles\{planeId}\SimConnectDataDefinition.json";


                var definitions = new List<SimConnectDataDefinition>();

                // Add the special TITLE string variable first. It has to be to first property in SimConnectStruct
                definitions.Add(new SimConnectDataDefinition() { PropName = "TITLE", VariableName = "TITLE", DataType = DataType.String, DataDefinitionType = DataDefinitionType.SimConnect, DefaultValue = "" });

                using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, filePath)))
                {
                    var definitionGroup = JsonConvert.DeserializeObject<List<SimConnectDataDefinition>>(reader.ReadToEnd());

                    foreach (var def in definitionGroup)
                    {
                        if (!definitions.Exists(d => d.PropName == def.PropName))
                            definitions.Add(def);
                        else
                            TouchPanelLogger.ServerLog(@$"config\profiles\{planeId}\SimConnectDataDefinition.json has duplicate entry with PropName: {def.PropName}", LogLevel.ERROR);
                    }
                }


                return definitions;
            }
            catch
            {
                TouchPanelLogger.ServerLog("SimConnectDataDefinition.json file is not found or is invalid.", LogLevel.ERROR);
                return new List<SimConnectDataDefinition>();
            }
        }

        public static string[] GetMobiFlightPresets(string fileName)
        {
            try
            {
                string filePath;
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    filePath = Path.Combine(GetSolutionPath(), $@"TouchPanel\ReactClient\public\config\mobiFlightPresets\{fileName}");
                else
                    filePath = Path.Combine(AppContext.BaseDirectory, $@"ReactClient\config\mobiFlightPresets\{fileName}");

                if (File.Exists(filePath))
                    return File.ReadAllLines(filePath);

                return null;
            }
            catch
            {
                TouchPanelLogger.ServerLog("Unable to read MobilFlightPresets cip files.", LogLevel.ERROR);
                return null;
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