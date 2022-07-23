using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class TouchPanelSimConnector
    {
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private const int WM_USER_SIMCONNECT = 0x0402;
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_SIMVAR = "MobiFlight.LVars";
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_COMMAND = "MobiFlight.Command";
        private const string MOBIFLIGHT_CLIENT_DATA_NAME_RESPONSE = "MobiFlight.Response";
        private const string STANDARD_EVENT_GROUP = "STANDARD";
        private const int MOBIFLIGHT_MESSAGE_SIZE = 1024;       // The message size for commands and responses, this has to be changed also in SimConnectDefintions
        private const int MSFS_TRANSMIT_LOCK_TIMEOUT = 50;

        private Dictionary<string, List<Tuple<string, uint>>> _simConnectEvents;
        private List<SimVar> SimVars = new List<SimVar>();
        private List<string> LVars = new List<string>();
        private string ResponseStatus = "NEW";
        private uint _maxClientDataDefinition = 0;
        private object _msfs_transmit_lock = new object();

        private SimConnect _simConnect;
        private System.Timers.Timer _connectionTimer;

        public event EventHandler<string> OnCriticalException;
        public event EventHandler<List<SimConnectDataDefinition>> OnReceivedData;
        public event EventHandler OnConnected;
        public event EventHandler<SimConnectSystemEvent> OnReceiveSystemEvent;
        public event EventHandler LVarListUpdated;

        public bool Connected { get; set; }

        public List<SimConnectDataDefinition> SimConnectDataDefinitions;

        public void Start()
        {
            LoadEventPresets();

            _connectionTimer = new System.Timers.Timer();
            _connectionTimer.Interval = MSFS_CONNECTION_RETRY_TIMEOUT;
            _connectionTimer.Enabled = true;
            _connectionTimer.Elapsed += (source, e) =>
            {
                try
                {
                    InitializeSimConnect();
                }
                catch
                {
                    // handle SimConnect instantiation error when MSFS is not connected
                }
            };
        }

        public void ResetSimConnectDataArea(string planeId)
        {
            if (_simConnect != null)
            {
                _simConnect.ClearDataDefinition(SIMCONNECT_DATA_DEFINITION_TOUCHPANEL.SIMCONNECT_DATA_STRUCT_TOUCHPANEL);
                MobiFlightWasmClient.Ping(_simConnect);

                // Reset MobiFlight wasm event
                _simConnect.OnRecvClientData -= HandleOnRecvClientData;
                _simConnect.OnRecvClientData += HandleOnRecvClientData;

                // Requery new LVar list
                MobiFlightWasmClient.GetLVarList(_simConnect);

                if (!String.IsNullOrEmpty(planeId))
                {
                    SimConnectDataDefinitions = ConfigurationReader.GetSimConnectDataDefinitions(planeId);
                    AddDataDefinitions();
                }
                else
                {
                    SimConnectDataDefinitions = null;
                    // Clear memory cache data to send back to ReactJs app
                    OnReceivedData?.Invoke(this, null);
                }
            }
        }

        public void StopAndReconnect()
        {
            Stop();
            InitializeSimConnect();
        }

        public void Stop()
        {
            if (_simConnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                _simConnect.Dispose();
                _simConnect = null;

                SimVars.Clear();
                _maxClientDataDefinition = 0;
            }

            Connected = false;
        }

        public void RequestData()
        {
            if (_simConnect == null || !Connected)
                return;

            _simConnect.RequestDataOnSimObjectType(DATA_REQUEST.REQUEST_1, SIMCONNECT_DEFINE_ID.Dummy, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        public void ReceiveMessage()
        {
            if (_simConnect == null)
                return;

            try
            {
                _simConnect.ReceiveMessage();
            }
            catch (Exception ex)
            {
                if (ex.Message != "0xC00000B0")
                {
                    OnCriticalException?.Invoke(this, $"TouchPanel SimConnector: SimConnect receive message exception - {ex.Message}");
                }
            }
        }

        public void SetSimVar(string simVarCode)
        {
            if (_simConnect == null || !Connected)
                return;

            MobiFlightWasmClient.SendWasmCmd(_simConnect, "MF.SimVars.Set." + simVarCode);
            MobiFlightWasmClient.DummyCommand(_simConnect);
        }

        private void InitializeSimConnect()
        {
            // The constructor is similar to SimConnect_Open in the native API
            _simConnect = new SimConnect("TouchPanel Simconnect - Touch Panel Server", Process.GetCurrentProcess().MainWindowHandle, WM_USER_SIMCONNECT, null, 0);

            _connectionTimer.Enabled = false;

            // Listen to connect and quit msgs
            _simConnect.OnRecvOpen += HandleOnRecvOpen;
            _simConnect.OnRecvQuit += HandleOnRecvQuit;
            _simConnect.OnRecvException += HandleOnRecvException;
            _simConnect.OnRecvEvent += HandleOnReceiveSystemEvent;
            _simConnect.OnRecvSimobjectDataBytype += HandleOnRecvSimobjectDataBytype;

            // Register simConnect system events
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.SIMSTART);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.SIMSTART, "SimStart");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.SIMSTOP);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.SIMSTOP, "SimStop");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.VIEW);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.VIEW, "View");

            System.Threading.Thread.Sleep(4000);
            ReceiveMessage();
        }

        private void AddDataDefinitions()
        {
            if (SimConnectDataDefinitions == null)
                return;

            foreach (var definition in SimConnectDataDefinitions)
            {
                switch (definition.DataDefinitionType)
                {
                    case DataDefinitionType.AVar:
                        GetSimVar($"(A:{definition.VariableName})");
                        break;
                    case DataDefinitionType.LVar:
                        GetSimVar($"(L:{definition.VariableName})");
                        break;
                    case DataDefinitionType.SimConnect:
                        SIMCONNECT_DATATYPE simmConnectDataType;
                        switch (definition.DataType)
                        {
                            case DataType.String:
                                simmConnectDataType = SIMCONNECT_DATATYPE.STRING256;
                                break;
                            case DataType.Float64:
                                simmConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                                break;
                            default:
                                simmConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                                break;
                        }

                        _simConnect.AddToDataDefinition(SIMCONNECT_DATA_DEFINITION_TOUCHPANEL.SIMCONNECT_DATA_STRUCT_TOUCHPANEL, definition.VariableName, definition.SimConnectUnit, simmConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                        break;
                }
            }

            // register simConnect data structure
            _simConnect.RegisterDataDefineStruct<TouchPanelSimConnectStruct>(SIMCONNECT_DATA_DEFINITION_TOUCHPANEL.SIMCONNECT_DATA_STRUCT_TOUCHPANEL);
        }

        private void HandleOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            MobilFlightInitialize();

            // MobiFlight wasm event
            _simConnect.OnRecvClientData -= HandleOnRecvClientData;
            _simConnect.OnRecvClientData += HandleOnRecvClientData;

            Connected = true;

            OnConnected?.Invoke(this, null);
            MobiFlightWasmClient.Ping(_simConnect);
        }

        private void HandleOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Stop();
        }

        private void HandleOnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION e = (SIMCONNECT_EXCEPTION)data.dwException;

            switch (e)
            {
                case SIMCONNECT_EXCEPTION.ALREADY_CREATED:
                case SIMCONNECT_EXCEPTION.UNRECOGNIZED_ID:
                case SIMCONNECT_EXCEPTION.EVENT_ID_DUPLICATE:
                    break;
                default:
                    FileLogger.WriteLog($"TouchPanel SimConnector: SimConnect OnRecv Exception - {e}", StatusMessageType.Error);
                    break;
            }
        }

        private void HandleOnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnect == null || !Connected || SimConnectDataDefinitions == null)
                return;

            if (data.dwRequestID != 0)
                FileLogger.WriteLog($"TouchPanel SimConnector: SimConnect unknown request ID - {data.dwRequestID}", StatusMessageType.Error);

            try
            {
                var simConnectStruct = (TouchPanelSimConnectStruct)data.dwData[0];
                var simConnectStructFields = typeof(TouchPanelSimConnectStruct).GetFields();

                int i = 0;
                foreach (var definition in SimConnectDataDefinitions)
                {
                    switch (definition.DataDefinitionType)
                    {
                        case DataDefinitionType.AVar:
                            definition.Value = GetSimVar($"(A:{definition.VariableName})");
                            break;
                        case DataDefinitionType.LVar:
                            definition.Value = GetSimVar($"(L:{definition.VariableName})");
                            break;
                        case DataDefinitionType.SimConnect:
                            definition.Value = simConnectStructFields[i++].GetValue(simConnectStruct); // increment structure counter after assignment
                            break;
                    }
                }

                OnReceivedData?.Invoke(this, SimConnectDataDefinitions);
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"TouchPanel SimConnector - SimConnect receive data exception: {ex.Message}", ex);
            }
        }

        private void HandleOnReceiveSystemEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var systemEvent = ((SimConnectSystemEvent)data.uEventID);
            OnReceiveSystemEvent?.Invoke(this, systemEvent);
        }

        private void MobilFlightInitialize()
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
        }

        public void HandleOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
            if (data.dwRequestID != 0)
            {
                var simData = (ClientDataValue)data.dwData[0];
                if (SimVars.Count < (int)data.dwRequestID) return;
                SimVars[(int)(data.dwRequestID - 1)].Data = simData.data;
            }
            else
            {
                var simData = (ResponseString)data.dwData[0];

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

        private float GetSimVar(string simVarName)
        {
            if (!SimVars.Exists(lvar => lvar.Name == simVarName))
            {
                RegisterSimVar(simVarName);
                MobiFlightWasmClient.SendWasmCmd(_simConnect, "MF.SimVars.Add." + simVarName);
            }

            return SimVars.Find(lvar => lvar.Name == simVarName).Data;
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

        public void SetEventID(string eventID, uint value)
        {
            if (_simConnect == null || !Connected)
                return;

            Tuple<string, uint> eventItem = null;

            try
            {
                foreach (string GroupKey in _simConnectEvents.Keys)
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
                    FileLogger.WriteLog($"TouchPanel MobiFlight: MobiFlight SimConnect SetEventId '{eventID}' Error - {ex.Message}", StatusMessageType.Error);
            }
        }

        private void LoadEventPresets()
        {
            if (_simConnectEvents == null)
                _simConnectEvents = new Dictionary<string, List<Tuple<string, uint>>>();
            else
                _simConnectEvents.Clear();

            var GroupKey = "Dummy";
            uint EventIdx = 0;
            string[] lines;

            lines = ConfigurationReader.GetMobiFlightPresets("msfs2020_eventids.cip");
            if (lines != null)
            {
                _simConnectEvents[GroupKey] = new List<Tuple<string, uint>>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("//")) continue;

                    var cols = line.Split(':');
                    if (cols.Length > 1)
                    {
                        GroupKey = cols[0];
                        if (_simConnectEvents.ContainsKey(GroupKey)) continue;

                        _simConnectEvents[GroupKey] = new List<Tuple<string, uint>>();
                        continue; // we found a group
                    }

                    _simConnectEvents[GroupKey].Add(new Tuple<string, uint>(cols[0], EventIdx++));
                }
            }

            lines = ConfigurationReader.GetMobiFlightPresets("msfs2020_eventids_user.cip");
            if (lines != null)
            {
                GroupKey = "User";
                _simConnectEvents[GroupKey] = new List<Tuple<string, uint>>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("//")) continue;
                    var cols = line.Split(':');
                    if (cols.Length > 1)
                    {
                        GroupKey = cols[0];
                        if (_simConnectEvents.ContainsKey(GroupKey)) continue;

                        _simConnectEvents[GroupKey] = new List<Tuple<string, uint>>();
                        continue; // we found a group
                    }

                    _simConnectEvents[GroupKey].Add(new Tuple<string, uint>(cols[0], EventIdx++));
                }
            }
        }
    }
    public class SimVar
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public float Data { get; set; }
    }
}