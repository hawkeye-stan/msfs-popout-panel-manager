using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    internal class AppAutoStart
    {
        public static void Activate()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var filePath = GetFilePath();
            var autoStartArg = new LaunchAddOn() { Name = "MSFS Popout Panel Manager", Disabled = "false", Path = $@"{Directory.GetCurrentDirectory()}\MSFSPopoutPanelManager.exe" };
            var serializer = new XmlSerializer(typeof(SimBaseDocument));

            if (filePath != null && File.Exists(filePath))
            {
                // Create backup file if needed
                if (!File.Exists(filePath + ".backup"))
                {
                    File.Copy(filePath, filePath + ".backup");
                }

                SimBaseDocument data;

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    data = (SimBaseDocument)serializer.Deserialize(stream);
                }

                if (data == null) 
                    return;
                
                if (data.LaunchAddOn.Count == 0)
                {
                    data.LaunchAddOn.Add(autoStartArg);
                }
                else
                {
                    var autoStartIndex = data.LaunchAddOn.FindIndex(x => x.Name == "MSFS Popout Panel Manager");

                    if (autoStartIndex > -1)
                        data.LaunchAddOn[autoStartIndex] = autoStartArg;
                    else
                        data.LaunchAddOn.Add(autoStartArg);
                }

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.SetLength(0);
                    serializer.Serialize(stream, data,
                        new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                }
            }
            else
            {
                var data = new SimBaseDocument
                {
                    Descr = "SimConnect",
                    Filename = "SimConnect.xml",
                    Disabled = "False",
                    Type = "SimConnect",
                    Version = "1,0",
                    LaunchAddOn = new List<LaunchAddOn>() { autoStartArg }
                };

                if (filePath == null) 
                    return;
                
                using var file = File.Create(filePath);
                serializer.Serialize(file, data, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
        }

        internal static void Deactivate()
        {
            var filePath = GetFilePath();

            if (filePath == null || !File.Exists(filePath)) 
                return;
            
            SimBaseDocument data;
            var serializer = new XmlSerializer(typeof(SimBaseDocument));

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                data = (SimBaseDocument)serializer.Deserialize(stream);
            }

            if (data == null) 
                return;

            if (data.LaunchAddOn.Count > 0)
            {
                var autoStartIndex = data.LaunchAddOn.FindIndex(x => x.Name == "MSFS Popout Panel Manager");

                if (autoStartIndex > -1)
                    data.LaunchAddOn.RemoveAt(autoStartIndex);
            }

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.SetLength(0);
                serializer.Serialize(stream, data, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
        }

        internal static bool CheckIsAutoStart()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var filePath = GetFilePath();

            if (filePath != null && File.Exists(filePath))
            {
                SimBaseDocument data;
                XmlSerializer serializer = new XmlSerializer(typeof(SimBaseDocument));

                using (Stream stream = new FileStream(filePath, FileMode.Open))
                {
                    data = (SimBaseDocument)serializer.Deserialize(stream);
                }

                if (data != null)
                {
                    if (data.LaunchAddOn.Count > 0)
                    {
                        var autoStartIndex = data.LaunchAddOn.FindIndex(x => x.Name == "MSFS Popout Panel Manager");

                        if (autoStartIndex > -1)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return false;
        }

        private static string GetFilePath()
        {
            var filePathMsStore = Environment.ExpandEnvironmentVariables("%LocalAppData%") + @"\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\";
            var filePathSteam = Environment.ExpandEnvironmentVariables("%AppData%") + @"\Microsoft Flight Simulator\";

            if (Directory.Exists(filePathMsStore))
                return filePathMsStore + "exe.xml";
            else if (Directory.Exists(filePathSteam))
                return filePathSteam + "exe.xml";
            else
                return null;
        }
    }

    [XmlRoot(ElementName = "SimBase.Document")]
    public class SimBaseDocument
    {
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "Descr")]
        public string Descr { get; set; }

        [XmlElement(ElementName = "Filename")]
        public string Filename { get; set; }

        [XmlElement(ElementName = "Disabled")]
        public string Disabled { get; set; }

        [XmlElement(ElementName = "Launch.Addon")]
        public List<LaunchAddOn> LaunchAddOn { get; set; }
    }

    public class LaunchAddOn
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Disabled")]
        public string Disabled { get; set; }

        [XmlElement(ElementName = "Path")]
        public string Path { get; set; }

        [XmlElement(ElementName = "CommandLine")]
        public string CommandLine { get; set; }
    }
}
