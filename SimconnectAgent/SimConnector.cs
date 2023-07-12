using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnector
    {
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private const uint WM_USER_SIMCONNECT = 0x0402;

        private SimConnect _simConnect;
        private Timer _connectionTimer;
        private bool _isDisabledReconnect;

        private List<SimConnectDataDefinition> _simConnectRequiredDataDefinitions;
        private List<SimConnectDataDefinition> _simConnectHudBarDataDefinitions;
        private FieldInfo[] _simConnectStructFields;

        public event EventHandler<string> OnException;
        public event EventHandler<List<SimDataItem>> OnReceivedRequiredData;
        public event EventHandler<List<SimDataItem>> OnReceivedHudBarData;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<SimConnectEvent> OnReceiveSystemEvent;
        public event EventHandler<string> OnActiveAircraftChanged;

        public bool Connected { get; set; }

        public SimConnector()
        {
            _simConnectStructFields = typeof(SimConnectStruct).GetFields(BindingFlags.Public | BindingFlags.Instance);
            _simConnectRequiredDataDefinitions = SimDataDefinitions.GetRequiredDefinitions();
        }

        public void Start()
        {
            _connectionTimer = new Timer();
            _connectionTimer.Interval = MSFS_CONNECTION_RETRY_TIMEOUT;
            _connectionTimer.Enabled = true;
            _connectionTimer.Elapsed += (source, e) =>
            {
                try
                {
                    Debug.WriteLine("Connecting to MSFS...");
                    InitializeSimConnect();
                }
                catch
                {
                    // handle SimConnect instantiation error when MSFS is not connected
                }
            };
        }

        public void Restart()
        {
            _connectionTimer.Enabled = true;
        }

        public void SetSimConnectHudBarDataDefinition(SimDataDefinitionType definitionType)
        {
            _simConnectHudBarDataDefinitions = SimDataDefinitions.GetHudBarDefinitions(definitionType);
            AddHudBarDataDefinitions();
        }

        private void HandleOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            ReceiveMessage();
        }

        public void StopAndReconnect()
        {
            if (!_isDisabledReconnect)
            {
                Stop();
                InitializeSimConnect();
            }
        }

        public void Stop()
        {
            if (_simConnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                //try { _simConnect.Dispose(); } catch { }
                _simConnect = null;
            }

            Connected = false;
        }

        public void RequestRequiredData()
        {
            if (_simConnect == null || !Connected)
                return;

            try
            {
                _simConnect.RequestDataOnSimObjectType(DATA_REQUEST.REQUIRED_REQUEST, DATA_DEFINITION.REQUIRED_DEFINITION, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch
            {
                if (!_isDisabledReconnect)
                    _isDisabledReconnect = true;

                OnException?.Invoke(this, null);
            }
        }

        public void RequestHudBarData()
        {
            if (_simConnect == null || !Connected)
                return;

            try
            {

                if (_simConnectHudBarDataDefinitions != null)
                    _simConnect.RequestDataOnSimObjectType(DATA_REQUEST.HUDBAR_REQUEST, DATA_DEFINITION.HUDBAR_DEFINITION, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch
            {
                if (!_isDisabledReconnect)
                    _isDisabledReconnect = true;

                OnException?.Invoke(this, null);
            }
        }

        public void ReceiveMessage()
        {
            if (_simConnect == null)
                return;

            try
            {
                if (!_isDisabledReconnect)
                    _simConnect.ReceiveMessage();
            }
            catch (Exception ex)
            {
                if (ex.Message != "0xC00000B0")
                {
                    FileLogger.WriteLog($"SimConnector: SimConnect receive message exception - {ex.Message}", StatusMessageType.Error);
                }

                if (!_isDisabledReconnect)
                {
                    // Prevent multiple reconnects from running
                    _isDisabledReconnect = true;

                    OnException?.Invoke(this, null);
                }
            }
        }

        public void TransmitActionEvent(ActionEvent eventID, uint data)
        {
            if (_simConnect != null)
            {
                try
                {
                    _simConnect.TransmitClientEvent(0U, eventID, data, NotificationGroup.GROUP0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    var eventName = eventID.ToString()[4..];        // trim out KEY_ prefix
                    FileLogger.WriteLog($"SimConnector: SimConnect transmit event exception - EventName: {eventName} - {ex.Message}", StatusMessageType.Error);
                }
            }
        }

        public void SetDataObject(string propName, object dValue)
        {
            try
            {
                var dataStruct = new WriteableDataStruct();
                dataStruct.Prop0 = (double)dValue;

                _simConnect.SetDataOnSimObject(DATA_DEFINITION.WRITEABLE_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLog($"SimConnector: Unable to set SimConnect variable - {ex.Message}", StatusMessageType.Error);
            }
        }

        private void InitializeSimConnect()
        {
            Debug.WriteLine("Trying to start simConnect");
            _simConnect = new SimConnect("MSFS Pop Out Panel Manager", Process.GetCurrentProcess().MainWindowHandle, WM_USER_SIMCONNECT, null, 0);
            Debug.WriteLine("SimConnect started");

            _connectionTimer.Enabled = false;

            // Listen to connect and quit msgs
            _simConnect.OnRecvOpen += HandleOnRecvOpen;
            _simConnect.OnRecvQuit += HandleOnRecvQuit;
            _simConnect.OnRecvException += HandleOnRecvException;
            _simConnect.OnRecvEvent += HandleOnReceiveSystemEvent;
            _simConnect.OnRecvSimobjectDataBytype += HandleOnRecvSimobjectDataBytype;
            _simConnect.OnRecvEventFilename += HandleOnRecvEventFilename;
            _simConnect.OnRecvSystemState += HandleOnRecvSystemState;

            // Register simConnect system events
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.SIMSTART);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.SIMSTART, "SimStart");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.SIMSTOP);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.SIMSTOP, "SimStop");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.VIEW);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.VIEW, "View");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.AIRCRAFTLOADED);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.AIRCRAFTLOADED, "AircraftLoaded");


            AddRequiredDataDefinitions();
            SetupActionEvents();

            _simConnect.RequestSystemState(SystemStateRequestId.AIRCRAFTPATH, "AircraftLoaded");

            _isDisabledReconnect = false;

            Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    ReceiveMessage();
                }
            });

            OnConnected?.Invoke(this, null);
            Connected = true;
        }

        private void HandleOnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            switch ((SystemStateRequestId)Enum.Parse(typeof(SystemStateRequestId), data.dwRequestID.ToString()))
            {
                case SystemStateRequestId.AIRCRAFTPATH:
                    SetActiveAircraftTitle(data.szString);
                    break;
            }
        }

        private void HandleOnRecvEventFilename(SimConnect sender, SIMCONNECT_RECV_EVENT_FILENAME data)
        {
            switch (data.uEventID)
            {
                case (uint)SimConnectEvent.AIRCRAFTLOADED:
                    SetActiveAircraftTitle(data.szFileName);
                    break;
            }
        }

        private void SetupActionEvents()
        {
            // Setup SimEvent mapping
            //_simConnect.SetInputGroupPriority(NotificationGroup.GROUP0, (uint)SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);

            foreach (ActionEvent item in Enum.GetValues(typeof(ActionEvent)))
            {
                _simConnect.MapClientEventToSimEvent(item, item.ToString());
                //_simConnect.AddClientEventToNotificationGroup(NotificationGroup.GROUP0, item, true);
            }
        }

        private void AddRequiredDataDefinitions()
        {
            if (_simConnect == null || _simConnectRequiredDataDefinitions == null)
                return;

            foreach (var definition in _simConnectRequiredDataDefinitions)
            {
                if (definition.DefinitionId == DATA_DEFINITION.REQUIRED_DEFINITION && definition.DataDefinitionType == DataDefinitionType.SimConnect)
                {
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

                    _simConnect.AddToDataDefinition(definition.DefinitionId, definition.VariableName, definition.SimConnectUnit, simmConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
            }

            _simConnect.AddToDataDefinition(DATA_DEFINITION.WRITEABLE_DEFINITION, "TRACK IR ENABLE", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DATA_DEFINITION.REQUIRED_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DATA_DEFINITION.WRITEABLE_DEFINITION);
        }

        private void AddHudBarDataDefinitions()
        {
            if (_simConnect == null)
                return;

            _simConnect.ClearDataDefinition(DATA_DEFINITION.HUDBAR_DEFINITION);

            if (_simConnectHudBarDataDefinitions == null)
                return;

            foreach (var definition in _simConnectHudBarDataDefinitions)
            {
                if (definition.DefinitionId == DATA_DEFINITION.HUDBAR_DEFINITION && definition.DataDefinitionType == DataDefinitionType.SimConnect)
                {
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

                    _simConnect.AddToDataDefinition(definition.DefinitionId, definition.VariableName, definition.SimConnectUnit, simmConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
            }

            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DATA_DEFINITION.HUDBAR_DEFINITION);
        }

        private void HandleOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Stop();
            OnDisconnected?.Invoke(this, null);
        }

        private void HandleOnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION e = (SIMCONNECT_EXCEPTION)data.dwException;

            switch (e)
            {
                case SIMCONNECT_EXCEPTION.DATA_ERROR:
                case SIMCONNECT_EXCEPTION.NAME_UNRECOGNIZED:
                case SIMCONNECT_EXCEPTION.ALREADY_CREATED:
                case SIMCONNECT_EXCEPTION.UNRECOGNIZED_ID:
                case SIMCONNECT_EXCEPTION.EVENT_ID_DUPLICATE:
                    break;
                default:
                    OnException?.Invoke(this, $"SimConnector: SimConnect on recieve exception - {e}");
                    break;
            }
        }

        private void HandleOnReceiveSystemEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var systemEvent = ((SimConnectEvent)data.uEventID);
            OnReceiveSystemEvent?.Invoke(this, systemEvent);
        }

        private void HandleOnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnect == null || !Connected)
                return;

            if (data.dwRequestID == (int)DATA_REQUEST.REQUIRED_REQUEST)
                ParseRequiredReceivedSimData(data);
            else if (data.dwRequestID == (int)DATA_REQUEST.HUDBAR_REQUEST)
                ParseHudBarReceivedSimData(data);
        }


        private void ParseRequiredReceivedSimData(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnectRequiredDataDefinitions == null)
                return;

            try
            {
                var simData = new List<SimDataItem>();
                var simDataStruct = (SimConnectStruct)data.dwData[0];

                int i = 0;
                foreach (var definition in _simConnectRequiredDataDefinitions)
                {
                    if (definition.DataDefinitionType == DataDefinitionType.SimConnect)
                    {
                        simData.Add(new SimDataItem { PropertyName = definition.PropName, Value = (double)_simConnectStructFields[i].GetValue(simDataStruct) });
                        i++;
                    }
                }

                OnReceivedRequiredData?.Invoke(this, simData);
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"SimConnector: SimConnect received required data exception - {ex.Message}", ex);
            }
        }

        private void ParseHudBarReceivedSimData(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            try
            {
                if (_simConnectHudBarDataDefinitions == null)
                    return;

                var simData = new List<SimDataItem>();
                var simDataStruct = (SimConnectStruct)data.dwData[0];

                int i = 0;
                lock (_simConnectHudBarDataDefinitions)
                {
                    foreach (var definition in _simConnectHudBarDataDefinitions)
                    {
                        if (definition.DataDefinitionType == DataDefinitionType.SimConnect)
                        {
                            simData.Add(new SimDataItem { PropertyName = definition.PropName, Value = (double)_simConnectStructFields[i].GetValue(simDataStruct) });
                            i++;
                        }
                    }
                }

                OnReceivedHudBarData?.Invoke(this, simData);
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"SimConnector: SimConnect received hud bar data exception - {ex.Message}", ex);
                StopAndReconnect();
            }
        }

        private void SetActiveAircraftTitle(string aircraftFilePath)
        {
            var filePathToken = aircraftFilePath.Split(@"\");

            if (filePathToken.Length > 1)
            {
                var aircraftName = filePathToken[filePathToken.Length - 2];
                aircraftName = aircraftName.Replace("_", " ").ToUpper();

                OnActiveAircraftChanged?.Invoke(this, aircraftName);

                //var def = _simConnectDataDefinitions.Find(s => s.PropName == SimDataDefinitions.PropName.AircraftName);
                //if(def != null)
                //{
                //    def.Value = aircraftName;
                //    OnReceivedData?.Invoke(this, _simConnectDataDefinitions);
                //}
            }
        }
    }
}