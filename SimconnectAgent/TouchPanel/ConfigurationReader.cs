using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class ConfigurationReader
    {
        public static List<SimConnectDataDefinition> GetSimConnectDataDefinitions(string planeId)
        {
            try
            {
                string filePath;
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    filePath = Path.Combine(GetSolutionPath(), @$"ReactClient\public\config\profiles\{planeId}\SimConnectDataDefinition.json");
                else
                    filePath = @$"ReactClient\config\profiles\{planeId}\SimConnectDataDefinition.json";


                var definitions = new List<SimConnectDataDefinition>();

                // Add the special TITLE string variable first. It has to be to first property in SimConnectStruct
                definitions.Add(new SimConnectDataDefinition() { PropName = "Title", VariableName = "TITLE", DataType = DataType.String, DataDefinitionType = DataDefinitionType.SimConnect, DefaultValue = "" });

                using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, filePath)))
                {
                    var definitionGroup = JsonConvert.DeserializeObject<List<SimConnectDataDefinition>>(reader.ReadToEnd());

                    foreach (var def in definitionGroup)
                    {
                        if (!definitions.Exists(d => d.PropName == def.PropName))
                            definitions.Add(def);
                        else
                            FileLogger.WriteLog(@$"config\profiles\{planeId}\SimConnectDataDefinition.json has duplicate entry with PropName: {def.PropName}", StatusMessageType.Error);
                    }
                }

                definitions.Add(new SimConnectDataDefinition() { PropName = "CameraState", VariableName = "CAMERA STATE", DataType = DataType.Float64, DataDefinitionType = DataDefinitionType.SimConnect, SimConnectUnit = "Number", DefaultValue = String.Empty, JavaScriptFormatting = null });

                return definitions;
            }
            catch
            {
                FileLogger.WriteLog("TouchPanel: SimConnectDataDefinition.json file is not found or is invalid.", StatusMessageType.Error);
                return new List<SimConnectDataDefinition>();
            }
        }

        public static string[] GetMobiFlightPresets(string fileName)
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, $@"Resources\MobiFlightPresets\{fileName}");

                if (File.Exists(filePath))
                    return File.ReadAllLines(filePath);

                return null;
            }
            catch
            {
                FileLogger.WriteLog("TouchPanel: Unable to read MobilFlightPresets cip files.", StatusMessageType.Error);
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