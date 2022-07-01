using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.TouchPanel.FSConnector
{
    public class MobiFlightWasmClient
    {
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_SIMVAR = "MobiFlight.LVars";
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_COMMAND = "MobiFlight.Command";
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_RESPONSE = "MobiFlight.Response";
        private const string STANDARD_EVENT_GROUP = "STANDARD";
        private const int MOBIFLIGHT_MESSAGE_SIZE = 1024;       // The message size for commands and responses, this has to be changed also in SimConnectDefintions
        private const int MSFS_TRANSMIT_LOCK_TIMEOUT = 50;
        private string ResponseStatus = "NEW";

        private SimConnect _simConnect;
        private uint _maxClientDataDefinition = 0;
        private Dictionary<String, List<Tuple<String, uint>>> _simConnectEvents;
        private List<String> LVars = new List<String>();
        private object _msfs_transmit_lock = new object();

        public MobiFlightWasmClient(SimConnect simConnect)
        {
            _simConnect = simConnect;
            Connected = false;

            LoadEventPresets();
        }

        public List<SimVar> SimVars = new List<SimVar>();
        public bool Connected { get; set; }
        public event EventHandler LVarListUpdated;

        public void Ping()
        {
            if (_simConnect == null) return;

            SendWasmCmd("MF.Ping");
            DummyCommand();
        }

        public void Stop()
        {
            if (_simConnect == null)
                return;

            SendWasmCmd("MF.SimVars.Clear");

            SimVars.Clear();
            _maxClientDataDefinition = 0;

            Connected = false;
        }

        public void DummyCommand()
        {
            if (_simConnect == null)
                return;

            SendWasmCmd("MF.DummyCmd");
        }

        private void SendWasmCmd(String command)
        {
            if (_simConnect == null)
                return;

            try
            {
                _simConnect.SetClientData(
                    SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_CMD,
                   (SIMCONNECT_CLIENT_DATA_ID)0,
                   SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, 0,
                   new ClientDataString(command)
                );
            }
            catch { }
        }

        public void Initialize()
        {
            foreach (string GroupKey in _simConnectEvents.Keys)
            {
                foreach (Tuple<string, uint> eventItem in _simConnectEvents[GroupKey])
                {
                    var prefix = "";
                    if (GroupKey != STANDARD_EVENT_GROUP) prefix = "MobiFlight.";
                    _simConnect.MapClientEventToSimEvent((MOBIFLIGHT_EVENTS)eventItem.Item2, prefix + eventItem.Item1);
                }
            }

            // Map Client Data Access Area
            // Register Client Data (for SimVars)
            _simConnect.MapClientDataNameToID(MOBIFLIGHT_CLIENT_DATA_NAME_SIMVAR, SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_LVARS);
            _simConnect.CreateClientData(SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_LVARS, 4096, SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);

            // Register Client Data (for WASM Module Commands)
            var ClientDataStringSize = (uint)Marshal.SizeOf(typeof(ClientDataString));
            _simConnect.MapClientDataNameToID(MOBIFLIGHT_CLIENT_DATA_NAME_COMMAND, SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_CMD);
            _simConnect.CreateClientData(SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_CMD, MOBIFLIGHT_MESSAGE_SIZE, SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);

            // Register Client Data (for WASM Module Responses)
            _simConnect.MapClientDataNameToID(MOBIFLIGHT_CLIENT_DATA_NAME_RESPONSE, SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_RESPONSE);
            _simConnect.CreateClientData(SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_RESPONSE, MOBIFLIGHT_MESSAGE_SIZE, SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);

            _simConnect.AddToClientDataDefinition((SIMCONNECT_DEFINE_ID)0, 0, MOBIFLIGHT_MESSAGE_SIZE, 0, 0);
            _simConnect.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, ResponseString>((SIMCONNECT_DEFINE_ID)0);
            _simConnect.RequestClientData(
                SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_RESPONSE,
                (SIMCONNECT_REQUEST_ID)0,
                (SIMCONNECT_DEFINE_ID)0,
                SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET,
                SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.DEFAULT,
                0,
                0,
                0
            );

            // Reset LVARs
            ResetSimVar();

            // MobiFlight wasm event
            _simConnect.OnRecvClientData -= HandleOnRecvClientData;
            _simConnect.OnRecvClientData += HandleOnRecvClientData;
        }

        public float GetSimVar(String simVarName)
        {
            if (!SimVars.Exists(lvar => lvar.Name == simVarName))
            {
                RegisterSimVar(simVarName);
                SendWasmCmd("MF.SimVars.Add." + simVarName);
            }

            return SimVars.Find(lvar => lvar.Name == simVarName).Data;
        }

        public void SetSimVar(String simVarCode)
        {
            SendWasmCmd("MF.SimVars.Set." + simVarCode);
            DummyCommand();
        }

        public void ResetSimVar()
        {
            ClearSimVars();
            GetLVarList();
        }

        public void GetLVarList()
        {
            if (_simConnect == null)
                return;

            SendWasmCmd("MF.LVars.List");
            DummyCommand();
        }

        public void SetEventID(string eventID, uint value)
        {
            if (_simConnect == null || !Connected) return;

            Tuple<String, uint> eventItem = null;

            try
            {
                foreach (String GroupKey in _simConnectEvents.Keys)
                {
                    eventItem = _simConnectEvents[GroupKey].Find(x => x.Item1 == eventID);
                    if (eventItem != null) break;
                }

                if (eventItem == null) return;

                if (System.Threading.Monitor.TryEnter(_msfs_transmit_lock))
                {
                    _simConnect?.TransmitClientEvent(
                        0,
                        (MOBIFLIGHT_EVENTS)eventItem.Item2,
                        value,
                        SIMCONNECT_NOTIFICATION_GROUP_ID.SIMCONNECT_GROUP_PRIORITY_DEFAULT,
                        SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY
                    );

                    System.Threading.Thread.Sleep(MSFS_TRANSMIT_LOCK_TIMEOUT);

                    System.Threading.Monitor.Exit(_msfs_transmit_lock);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "0xC00000B0")
                    TouchPanelLogger.ServerLog($"MobiFlight SimConnect SetEventId '{eventID}' Error: {ex.Message}", LogLevel.ERROR);
            }
        }

        private void LoadEventPresets()
        {
            if (_simConnectEvents == null)
                _simConnectEvents = new Dictionary<string, List<Tuple<String, uint>>>();
            else
                _simConnectEvents.Clear();

            var GroupKey = "Dummy";
            uint EventIdx = 0;
            string[] lines;

            lines = ConfigurationReader.GetMobiFlightPresets("msfs2020_eventids.cip");
            if (lines != null)
            {
                _simConnectEvents[GroupKey] = new List<Tuple<String, uint>>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("//")) continue;

                    var cols = line.Split(':');
                    if (cols.Length > 1)
                    {
                        GroupKey = cols[0];
                        if (_simConnectEvents.ContainsKey(GroupKey)) continue;

                        _simConnectEvents[GroupKey] = new List<Tuple<String, uint>>();
                        continue; // we found a group
                    }

                    _simConnectEvents[GroupKey].Add(new Tuple<string, uint>(cols[0], EventIdx++));
                }
            }

            lines = ConfigurationReader.GetMobiFlightPresets("msfs2020_eventids_user.cip");
            if (lines != null)
            {
                GroupKey = "User";
                _simConnectEvents[GroupKey] = new List<Tuple<String, uint>>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("//")) continue;
                    var cols = line.Split(':');
                    if (cols.Length > 1)
                    {
                        GroupKey = cols[0];
                        if (_simConnectEvents.ContainsKey(GroupKey)) continue;

                        _simConnectEvents[GroupKey] = new List<Tuple<String, uint>>();
                        continue; // we found a group
                    }

                    _simConnectEvents[GroupKey].Add(new Tuple<string, uint>(cols[0], EventIdx++));
                }
            }
        }

        private void RegisterSimVar(string simVarName)
        {
            SimVar newSimVar = new SimVar() { Name = simVarName, Id = (uint)SimVars.Count + 1 };
            SimVars.Add(newSimVar);

            if (_maxClientDataDefinition >= newSimVar.Id)
            {
                return;
            }

            _maxClientDataDefinition = newSimVar.Id;

            _simConnect?.AddToClientDataDefinition(
                (SIMCONNECT_DEFINE_ID)newSimVar.Id,
                (uint)((SimVars.Count - 1) * sizeof(float)),
                sizeof(float),
                0,
                0);

            _simConnect?.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, ClientDataValue>((SIMCONNECT_DEFINE_ID)newSimVar.Id);

            _simConnect?.RequestClientData(
                SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_LVARS,
                (SIMCONNECT_REQUEST_ID)newSimVar.Id,
                (SIMCONNECT_DEFINE_ID)newSimVar.Id,
                SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET,
                SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED,
                0,
                0,
                0
            );
        }

        private void ClearSimVars()
        {
            SimVars.Clear();
        }

        private void HandleOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
            if (data.dwRequestID != 0)
            {
                var simData = (ClientDataValue)(data.dwData[0]);
                if (SimVars.Count < (int)(data.dwRequestID)) return;
                SimVars[(int)(data.dwRequestID - 1)].Data = simData.data;
            }
            else
            {
                var simData = (ResponseString)(data.dwData[0]);

                if (simData.Data == "MF.Pong")
                {
                    Connected = true;
                }

                if (simData.Data == "MF.LVars.List.Start")
                {
                    ResponseStatus = "LVars.List.Receiving";
                    LVars.Clear();
                }
                else if (simData.Data == "MF.LVars.List.End")
                {
                    ResponseStatus = "LVars.List.Completed";
                    LVarListUpdated?.Invoke(LVars, new EventArgs());
                }
                else if (ResponseStatus == "LVars.List.Receiving")
                {
                    LVars.Add(simData.Data);
                }
            }
        }
    }

    public class SimVar
    {
        public UInt32 Id { get; set; }
        public String Name { get; set; }
        public float Data { get; set; }
    }
}
