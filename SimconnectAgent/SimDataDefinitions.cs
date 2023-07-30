using System.Collections.Generic;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimDataDefinitions
    {
        public static List<SimConnectDataDefinition> GetRequiredDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>();
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElectricalMasterBattery, VariableName = "ELECTRICAL MASTER BATTERY", SimConnectUnit = "Bool", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AvionicsMasterSwitch, VariableName = "AVIONICS MASTER SWITCH", SimConnectUnit = "Bool", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.TrackIREnable, VariableName = "TRACK IR ENABLE", SimConnectUnit = "Bool", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.PlaneInParkingSpot, VariableName = "ATC ON PARKING SPOT", SimConnectUnit = "Bool", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraState, VariableName = "CAMERA STATE", SimConnectUnit = "Number", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.REQUIRED_DEFINITION, RequestId = DATA_REQUEST.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CockpitCameraZoom, VariableName = "COCKPIT CAMERA ZOOM", SimConnectUnit = "Percentage", DataType = DataType.Float64 });
            return definitions;
        }

        public static List<SimConnectDataDefinition> GetHudBarDefinitions(SimDataDefinitionType definitionType)
        {
            List<SimConnectDataDefinition> definitions;

            switch (definitionType)
            {
                case SimDataDefinitionType.GenericHudBar:
                    definitions = GetSharedHudBarDefinitions();
                    definitions.AddRange(GetGenericAircraftHudBarDefinitions());
                    return definitions;
                case SimDataDefinitionType.PMDG737HudBar:
                    definitions = GetSharedHudBarDefinitions();
                    definitions.AddRange(GetPMDG37HudBarDefinitions());
                    return definitions;
                case SimDataDefinitionType.NoHudBar:
                default:
                    return null;
            }
        }

        private static List<SimConnectDataDefinition> GetSharedHudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>();
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.SimRate, VariableName = "SIMULATION RATE", SimConnectUnit = "Number", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ParkingBrake, VariableName = "BRAKE PARKING INDICATOR", SimConnectUnit = "Bool", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearLeft, VariableName = "GEAR LEFT POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearCenter, VariableName = "GEAR CENTER POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearRight, VariableName = "GEAR RIGHT POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 });
            return definitions;
        }

        private static List<SimConnectDataDefinition> GetGenericAircraftHudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>();
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElevatorTrim, VariableName = "ELEVATOR TRIM PCT", SimConnectUnit = "percent", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AileronTrim, VariableName = "AILERON TRIM", SimConnectUnit = "radians", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.RudderTrim, VariableName = "RUDDER TRIM", SimConnectUnit = "radians", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.Flap, VariableName = "TRAILING EDGE FLAPS LEFT ANGLE", SimConnectUnit = "degrees", DataType = DataType.Float64 });
            return definitions;
        }

        private static List<SimConnectDataDefinition> GetPMDG37HudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>();
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElevatorTrim, VariableName = "L:switch_690_73X", SimConnectUnit = "number", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AileronTrim, VariableName = "AILERON POSITION", SimConnectUnit = "Position 16k", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.RudderTrim, VariableName = "RUDDER DEFLECTION PCT", SimConnectUnit = "Percent Over 100", DataType = DataType.Float64 });
            definitions.Add(new SimConnectDataDefinition() { DefinitionId = DATA_DEFINITION.HUDBAR_DEFINITION, RequestId = DATA_REQUEST.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.Flap, VariableName = "FLAPS HANDLE INDEX", SimConnectUnit = "Number", DataType = DataType.Float64 });
            return definitions;
        }

        public static class PropName
        {
            public static string ElectricalMasterBattery = "ElectricalMasterBattery";
            public static string AvionicsMasterSwitch = "AvionicsMasterSwitch";
            public static string TrackIREnable = "TrackIREnable";
            public static string PlaneInParkingSpot = "PlaneInParkingSpot";
            public static string CameraState = "CameraState";
            public static string AircraftName = "AircraftName";
            public static string CockpitCameraZoom = "CockpitCameraZoom";

            // Hud Bar data
            public static string ElevatorTrim = "ElevatorTrim";
            public static string AileronTrim = "AileronTrim";
            public static string RudderTrim = "RudderTrim";
            public static string ParkingBrake = "ParkingBrake";
            public static string Flap = "Flap";
            public static string GearLeft = "GearLeft";
            public static string GearCenter = "GearCenter";
            public static string GearRight = "GearRight";
            public static string SimRate = "SimRate";
        }

        public enum WriteableVariableName
        {
            TrackIREnable,
            CockpitCameraZoom
        }
    }

    public enum SimDataDefinitionType
    {
        NoHudBar,
        GenericHudBar,
        PMDG737HudBar
    }
}
