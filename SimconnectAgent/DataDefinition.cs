using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class DefaultSimConnectDataDefinition
    {
        public static List<SimConnectDataDefinition> GetDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>();
            definitions.Add(new SimConnectDataDefinition() { DataDefinitionType = DataDefinitionType.SimConnect, PropName = "Title", VariableName = "TITLE", SimConnectUnit = "", DataType = DataType.String, DefaultValue = String.Empty, JavaScriptFormatting = null });
            definitions.Add(new SimConnectDataDefinition() { DataDefinitionType = DataDefinitionType.SimConnect, PropName = "ElectricalMasterBattery", VariableName = "ELECTRICAL MASTER BATTERY", SimConnectUnit = "Bool", DataType = DataType.Float64, DefaultValue = String.Empty, JavaScriptFormatting = null });
            definitions.Add(new SimConnectDataDefinition() { DataDefinitionType = DataDefinitionType.SimConnect, PropName = "TrackIREnable", VariableName = "TRACK IR ENABLE", SimConnectUnit = "Bool", DataType = DataType.Float64, DefaultValue = String.Empty, JavaScriptFormatting = null });
            definitions.Add(new SimConnectDataDefinition() { DataDefinitionType = DataDefinitionType.SimConnect, PropName = "PlaneInParkingSpot", VariableName = "ATC ON PARKING SPOT", SimConnectUnit = "Bool", DataType = DataType.Float64, DefaultValue = String.Empty, JavaScriptFormatting = null });
            definitions.Add(new SimConnectDataDefinition() { DataDefinitionType = DataDefinitionType.SimConnect, PropName = "CameraState", VariableName = "CAMERA STATE", SimConnectUnit = "Number", DataType = DataType.Float64, DefaultValue = String.Empty, JavaScriptFormatting = null });
            return definitions;
        }
    }
}
