using System.Collections.Generic;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimDataDefinitions
    {
        public static List<SimConnectDataDefinition> GetRequiredDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>
            {
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElectricalMasterBattery, VariableName = "ELECTRICAL MASTER BATTERY", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AvionicsMasterSwitch, VariableName = "AVIONICS MASTER SWITCH", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.TrackIREnable, VariableName = "TRACK IR ENABLE", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.PlaneInParkingSpot, VariableName = "ATC ON PARKING SPOT", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraState, VariableName = "CAMERA STATE", SimConnectUnit = "Number", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CockpitCameraZoom, VariableName = "COCKPIT CAMERA ZOOM", SimConnectUnit = "Percentage", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraViewTypeAndIndex0, VariableName = "CAMERA VIEW TYPE AND INDEX:0", SimConnectUnit = "Enum", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraViewTypeAndIndex1, VariableName = "CAMERA VIEW TYPE AND INDEX:1", SimConnectUnit = "Enum", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraViewTypeAndIndex1Max, VariableName = "CAMERA VIEW TYPE AND INDEX MAX:1", SimConnectUnit = "Number", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.REQUIRED_DEFINITION, RequestId = DataRequest.REQUIRED_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.CameraViewTypeAndIndex2Max, VariableName = "CAMERA VIEW TYPE AND INDEX MAX:2", SimConnectUnit = "Number", DataType = DataType.Float64 },

            };
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
                case SimDataDefinitionType.Pmdg737HudBar:
                    definitions = GetSharedHudBarDefinitions();
                    definitions.AddRange(GetPmdg737HudBarDefinitions());
                    return definitions;
                case SimDataDefinitionType.NoHudBar:
                default:
                    return null;
            }
        }

        public static List<SimConnectDataDefinition> GetDynamicLodDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>
            {
                new() { DefinitionId = DataDefinition.DYNAMICLOD_DEFINITION, RequestId = DataRequest.DYNAMICLOD_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.PlaneAltAboveGround, VariableName = "PLANE ALT ABOVE GROUND", SimConnectUnit = "feet", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.DYNAMICLOD_DEFINITION, RequestId = DataRequest.DYNAMICLOD_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.PlaneAltAboveGroundMinusCg, VariableName = "PLANE ALT ABOVE GROUND MINUS CG", SimConnectUnit = "feet", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.DYNAMICLOD_DEFINITION, RequestId = DataRequest.DYNAMICLOD_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.SimOnGround, VariableName = "SIM ON GROUND", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.DYNAMICLOD_DEFINITION, RequestId = DataRequest.DYNAMICLOD_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GroundVelocity, VariableName = "GROUND VELOCITY", SimConnectUnit = "knots", DataType = DataType.Float64 }
            };
            return definitions;
        }

        private static List<SimConnectDataDefinition> GetSharedHudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>
            {
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.SimRate, VariableName = "SIMULATION RATE", SimConnectUnit = "Number", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ParkingBrake, VariableName = "BRAKE PARKING INDICATOR", SimConnectUnit = "Bool", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearLeft, VariableName = "GEAR LEFT POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearCenter, VariableName = "GEAR CENTER POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.GearRight, VariableName = "GEAR RIGHT POSITION", SimConnectUnit = "Percent", DataType = DataType.Float64 }
            };
            return definitions;
        }

        private static List<SimConnectDataDefinition> GetGenericAircraftHudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>
            {
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElevatorTrim, VariableName = "ELEVATOR TRIM PCT", SimConnectUnit = "percent", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AileronTrim, VariableName = "AILERON TRIM", SimConnectUnit = "radians", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.RudderTrim, VariableName = "RUDDER TRIM", SimConnectUnit = "radians", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.Flap, VariableName = "TRAILING EDGE FLAPS LEFT ANGLE", SimConnectUnit = "degrees", DataType = DataType.Float64 }
            };
            return definitions;
        }

        private static List<SimConnectDataDefinition> GetPmdg737HudBarDefinitions()
        {
            var definitions = new List<SimConnectDataDefinition>
            {
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.ElevatorTrim, VariableName = "L:switch_690_73X", SimConnectUnit = "number", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.AileronTrim, VariableName = "AILERON POSITION", SimConnectUnit = "Position 16k", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.RudderTrim, VariableName = "RUDDER DEFLECTION PCT", SimConnectUnit = "Percent Over 100", DataType = DataType.Float64 },
                new() { DefinitionId = DataDefinition.HUDBAR_DEFINITION, RequestId = DataRequest.HUDBAR_REQUEST, DataDefinitionType = DataDefinitionType.SimConnect, PropName = PropName.Flap, VariableName = "FLAPS HANDLE INDEX", SimConnectUnit = "Number", DataType = DataType.Float64 }
            };
            return definitions;
        }
        
        public static class PropName
        {
            public static string ElectricalMasterBattery = "ElectricalMasterBattery";
            public static string AvionicsMasterSwitch = "AvionicsMasterSwitch";
            public static string TrackIREnable = "TrackIREnable";
            public static string PlaneInParkingSpot = "PlaneInParkingSpot";
            public static string CameraState = "CameraState";
            public static string CockpitCameraZoom = "CockpitCameraZoom";
            public static string CameraViewTypeAndIndex0 = "CameraViewTypeAndIndex0";
            public static string CameraViewTypeAndIndex1 = "CameraViewTypeAndIndex1";
            public static string CameraViewTypeAndIndex1Max = "CameraViewTypeAndIndex1Max";
            public static string CameraViewTypeAndIndex2Max = "CameraViewTypeAndIndex2Max";

            // Dynamic LOD
            public static string PlaneAltAboveGround = "PlaneAltAboveGround";
            public static string PlaneAltAboveGroundMinusCg = "PlaneAltAboveGroundMinusCg";
            public static string SimOnGround = "SimOnGround";
            public static string GroundVelocity = "GroundVelocity";

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

        public enum WritableVariableName
        {
            TrackIREnable,
            CockpitCameraZoom,
            CameraState,
            CameraRequestAction,
            CameraViewTypeAndIndex0,
            CameraViewTypeAndIndex1
        }
    }

    public enum SimDataDefinitionType
    {
        NoHudBar,
        GenericHudBar,
        Pmdg737HudBar
    }
}
