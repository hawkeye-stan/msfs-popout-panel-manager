using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.FsConnector
{
    public class DataDefinition
    {
        public static List<(string PropName, string SimConnectName, string SimConnectUnit, SIMCONNECT_DATATYPE SimConnectDataType, Type ObjectType)> GetDefinition()
        {
            var def = new List<(string, string, string, SIMCONNECT_DATATYPE, Type)>
            {
                ("Title", "Title", null, SIMCONNECT_DATATYPE.STRING256, typeof(string)),
                ("ElectricalMasterBattery", "ELECTRICAL MASTER BATTERY", "Bool", SIMCONNECT_DATATYPE.FLOAT64, typeof(bool)),
                ("TrackIREnable", "TRACK IR ENABLE", "Bool", SIMCONNECT_DATATYPE.FLOAT64, typeof(bool))
            };

            return def;
        }
    }
}
