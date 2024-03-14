using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using static MSFSPopoutPanelManager.SimConnectAgent.SimDataDefinitions;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnector
    {
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private const uint WM_USER_SIMCONNECT = 0x0402;

        private SimConnect _simConnect;
        private Timer _connectionTimer;
        private bool _isDisabledReconnect;

        private readonly List<SimConnectDataDefinition> _simConnectRequiredDataDefinitions = SimDataDefinitions.GetRequiredDefinitions();
        private readonly List<SimConnectDataDefinition> _simConnectDynamicLodDataDefinitions = SimDataDefinitions.GetDynamicLodDefinitions();
        private List<SimConnectDataDefinition> _simConnectHudBarDataDefinitions;
        private readonly FieldInfo[] _simConnectStructFields = typeof(SimConnectStruct).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public event EventHandler<string> OnException;
        public event EventHandler<List<SimDataItem>> OnReceivedRequiredData;
        public event EventHandler<List<SimDataItem>> OnReceivedHudBarData;
        public event EventHandler<List<SimDataItem>> OnReceivedDynamicLodData;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<SimConnectEvent> OnReceiveSystemEvent;
        public event EventHandler<int> OnReceivedEventFrameData;
        public event EventHandler<string> OnActiveAircraftChanged;

        public bool Connected { get; set; }

        public void Start()
        {
            _connectionTimer = new()
            {
                Interval = MSFS_CONNECTION_RETRY_TIMEOUT,
                Enabled = true
            };

            _connectionTimer.Elapsed += (_, _) =>
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

        public void SetSimConnectDynamicLodDataDefinition()
        {
            AddDynamicLodDataDefinitions();
        }

        private void HandleOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            ReceiveMessage();
        }

        public void StopAndReconnect()
        {
            if (_isDisabledReconnect)
                return;

            Stop();
            InitializeSimConnect();
        }

        public void Stop()
        {
            _simConnect = null;
            Connected = false;
        }

        public void RequestRequiredData()
        {
            if (_simConnect == null || !Connected)
                return;

            try
            {
                _simConnect.RequestDataOnSimObjectType(DataRequest.REQUIRED_REQUEST, DataDefinition.REQUIRED_DEFINITION, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
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
                    _simConnect.RequestDataOnSimObjectType(DataRequest.HUDBAR_REQUEST, DataDefinition.HUDBAR_DEFINITION, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch
            {
                if (!_isDisabledReconnect)
                    _isDisabledReconnect = true;

                OnException?.Invoke(this, null);
            }
        }

        public void RequestDynamicLodData()
        {
            if (_simConnect == null || !Connected)
                return;

            try
            {

                if (_simConnectDynamicLodDataDefinitions != null)
                    _simConnect.RequestDataOnSimObjectType(DataRequest.DYNAMICLOD_REQUEST, DataDefinition.DYNAMICLOD_DEFINITION, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
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

        public void TransmitActionEvent(ActionEvent eventId, uint data)
        {
            if (_simConnect != null)
            {
                try
                {
                    _simConnect.TransmitClientEvent(0U, eventId, data, NotificationGroup.GROUP0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    var eventName = eventId.ToString()[4..];        // trim out KEY_ prefix
                    FileLogger.WriteLog($"SimConnector: SimConnect transmit event exception - EventName: {eventName} - {ex.Message}", StatusMessageType.Error);
                }
            }
        }

        public void SetDataObject(WritableVariableName propName, object dValue)
        {
            try
            {
                if (_simConnect == null)
                    return;

                var dataStruct = new WritableDataStruct
                {
                    Prop0 = (double)dValue
                };

                switch (propName)
                {
                    case WritableVariableName.TrackIREnable:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_TRACK_IR_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                    case WritableVariableName.CockpitCameraZoom:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_COCKPIT_CAMERA_ZOOM_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                    case WritableVariableName.CameraState:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_COCKPIT_CAMERA_STATE_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                    case WritableVariableName.CameraRequestAction:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_CAMERA_REQUEST_ACTION_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                    case WritableVariableName.CameraViewTypeAndIndex0:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_0_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                    case WritableVariableName.CameraViewTypeAndIndex1:
                        _simConnect.SetDataOnSimObject(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_1_DEFINITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dataStruct);
                        break;
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLog($"SimConnector: Unable to set SimConnect variable - {ex.Message}", StatusMessageType.Error);
            }
        }

        public void StartReceiveFrameData()
        {
            if (_simConnect == null)
                return;

            _simConnect.OnRecvEventFrame -= HandleOnRecvEventFrame;
            _simConnect.OnRecvEventFrame += HandleOnRecvEventFrame;

            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.FRAME);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.FRAME, "Frame");
        }

        public void StopReceiveFrameData()
        {
            _simConnect.OnRecvEventFrame -= HandleOnRecvEventFrame;

            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.FRAME);
        }

        private void InitializeSimConnect()
        {
            Debug.WriteLine("Trying to start simConnect");
            _simConnect = new SimConnect("MSFS Pop Out Panel Manager", Process.GetCurrentProcess().MainWindowHandle, WM_USER_SIMCONNECT, null, 0);
            Debug.WriteLine("SimConnect started");

            _connectionTimer.Enabled = false;

            // Listen to connect and quit messages
            _simConnect.OnRecvOpen += HandleOnRecvOpen;
            _simConnect.OnRecvQuit += HandleOnRecvQuit;
            _simConnect.OnRecvException += HandleOnRecvException;
            _simConnect.OnRecvEvent += HandleOnReceiveSystemEvent;
            _simConnect.OnRecvSimobjectDataBytype += HandleOnRecvSimObjectDataByType;
            _simConnect.OnRecvEventFilename += HandleOnRecvEventFilename;
            _simConnect.OnRecvSystemState += HandleOnRecvSystemState;

            // Register simConnect system events
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.SIM_START);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.SIM_START, "SimStart");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.SIM_STOP);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.SIM_STOP, "SimStop");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.VIEW);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.VIEW, "View");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectEvent.AIRCRAFT_LOADED);
            _simConnect.SubscribeToSystemEvent(SimConnectEvent.AIRCRAFT_LOADED, "AircraftLoaded");
          
            AddRequiredDataDefinitions();
            SetupActionEvents();

            _simConnect.RequestSystemState(SystemStateRequestId.AIRCRAFT_PATH, "AircraftLoaded");

            _isDisabledReconnect = false;

            Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    ReceiveMessage();
                }
            });

            OnConnected?.Invoke(this, EventArgs.Empty);
            Connected = true;
        }

        private void HandleOnRecvEventFrame(SimConnect sender, SIMCONNECT_RECV_EVENT_FRAME data)
        {
            if (data == null)
                return;

            OnReceivedEventFrameData?.Invoke(this, Convert.ToInt32(data.fFrameRate));
        }

        private void HandleOnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            switch ((SystemStateRequestId)Enum.Parse(typeof(SystemStateRequestId), data.dwRequestID.ToString()))
            {
                case SystemStateRequestId.AIRCRAFT_PATH:
                    SetActiveAircraftTitle(data.szString);
                    break;
            }
        }

        private void HandleOnRecvEventFilename(SimConnect sender, SIMCONNECT_RECV_EVENT_FILENAME data)
        {
            switch (data.uEventID)
            {
                case (uint)SimConnectEvent.AIRCRAFT_LOADED:
                    SetActiveAircraftTitle(data.szFileName);
                    break;
            }
        }

        private void SetupActionEvents()
        {
            foreach (ActionEvent item in Enum.GetValues(typeof(ActionEvent)))
            {
                _simConnect.MapClientEventToSimEvent(item, item.ToString());
            }
        }

        private void AddRequiredDataDefinitions()
        {
            if (_simConnect == null || _simConnectRequiredDataDefinitions == null)
                return;

            foreach (var definition in _simConnectRequiredDataDefinitions)
            {
                if (definition.DefinitionId != DataDefinition.REQUIRED_DEFINITION ||
                    definition.DataDefinitionType != DataDefinitionType.SimConnect) continue;

                SIMCONNECT_DATATYPE simConnectDataType;
                switch (definition.DataType)
                {
                    case DataType.String:
                        simConnectDataType = SIMCONNECT_DATATYPE.STRING256;
                        break;
                    case DataType.Float64:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                    default:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                }

                _simConnect.AddToDataDefinition(definition.DefinitionId, definition.VariableName, definition.SimConnectUnit, simConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            }

            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_TRACK_IR_DEFINITION, "TRACK IR ENABLE", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_COCKPIT_CAMERA_STATE_DEFINITION, "CAMERA STATE", "enum", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_COCKPIT_CAMERA_ZOOM_DEFINITION, "COCKPIT CAMERA ZOOM", "percentage", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_CAMERA_REQUEST_ACTION_DEFINITION, "CAMERA REQUEST ACTION", "enum", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_0_DEFINITION, "CAMERA VIEW TYPE AND INDEX:0", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_1_DEFINITION, "CAMERA VIEW TYPE AND INDEX:1", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.REQUIRED_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.WRITABLE_TRACK_IR_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.WRITABLE_COCKPIT_CAMERA_STATE_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.WRITABLE_COCKPIT_CAMERA_ZOOM_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_0_DEFINITION);
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.WRITABLE_CAMERA_VIEW_TYPE_INDEX_1_DEFINITION);
        }

        private void AddHudBarDataDefinitions()
        {
            if (_simConnect == null)
                return;

            _simConnect.ClearDataDefinition(DataDefinition.HUDBAR_DEFINITION);

            if (_simConnectHudBarDataDefinitions == null)
                return;

            foreach (var definition in _simConnectHudBarDataDefinitions)
            {
                if (definition.DefinitionId != DataDefinition.HUDBAR_DEFINITION ||
                    definition.DataDefinitionType != DataDefinitionType.SimConnect) continue;

                SIMCONNECT_DATATYPE simConnectDataType;
                switch (definition.DataType)
                {
                    case DataType.String:
                        simConnectDataType = SIMCONNECT_DATATYPE.STRING256;
                        break;
                    case DataType.Float64:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                    default:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                }

                _simConnect.AddToDataDefinition(definition.DefinitionId, definition.VariableName, definition.SimConnectUnit, simConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            }

            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.HUDBAR_DEFINITION);
        }

        private void AddDynamicLodDataDefinitions()
        {
            if (_simConnect == null)
                return;

            _simConnect.ClearDataDefinition(DataDefinition.DYNAMICLOD_DEFINITION);

            if (_simConnectDynamicLodDataDefinitions == null)
                return;

            foreach (var definition in _simConnectDynamicLodDataDefinitions)
            {
                if (definition.DefinitionId != DataDefinition.DYNAMICLOD_DEFINITION ||
                    definition.DataDefinitionType != DataDefinitionType.SimConnect) continue;

                SIMCONNECT_DATATYPE simConnectDataType;
                switch (definition.DataType)
                {
                    case DataType.String:
                        simConnectDataType = SIMCONNECT_DATATYPE.STRING256;
                        break;
                    case DataType.Float64:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                    default:
                        simConnectDataType = SIMCONNECT_DATATYPE.FLOAT64;
                        break;
                }

                _simConnect.AddToDataDefinition(definition.DefinitionId, definition.VariableName, definition.SimConnectUnit, simConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            }

            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(DataDefinition.DYNAMICLOD_DEFINITION);
        }

        private void HandleOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Stop();
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void HandleOnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            var e = (SIMCONNECT_EXCEPTION)data.dwException;

            switch (e)
            {
                case SIMCONNECT_EXCEPTION.DATA_ERROR:
                case SIMCONNECT_EXCEPTION.NAME_UNRECOGNIZED:
                case SIMCONNECT_EXCEPTION.ALREADY_CREATED:
                case SIMCONNECT_EXCEPTION.UNRECOGNIZED_ID:
                case SIMCONNECT_EXCEPTION.EVENT_ID_DUPLICATE:
                    break;
                default:
                    OnException?.Invoke(this, $"SimConnector: SimConnect on receive exception - {e}");
                    break;
            }
        }

        private void HandleOnReceiveSystemEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var systemEvent = ((SimConnectEvent)data.uEventID);
            OnReceiveSystemEvent?.Invoke(this, systemEvent);
        }

        private void HandleOnRecvSimObjectDataByType(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnect == null || !Connected)
                return;

            switch (data.dwRequestID)
            {
                case (int)DataRequest.REQUIRED_REQUEST:
                    ParseRequiredReceivedSimData(data);
                    break;
                case (int)DataRequest.HUDBAR_REQUEST:
                    ParseHudBarReceivedSimData(data);
                    break;
                case (int)DataRequest.DYNAMICLOD_REQUEST:
                    ParseDynamicLodReceivedSimData(data);
                    break;
            }
        }


        private void ParseRequiredReceivedSimData(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnectRequiredDataDefinitions == null)
                return;

            try
            {
                var simData = new List<SimDataItem>();
                var simDataStruct = (SimConnectStruct)data.dwData[0];

                var i = 0;
                foreach (var definition in _simConnectRequiredDataDefinitions)
                {
                    if (definition.DataDefinitionType != DataDefinitionType.SimConnect) 
                        continue;

                    var dataValue = _simConnectStructFields[i].GetValue(simDataStruct);

                    var simDataItem = new SimDataItem
                    {
                        PropertyName = definition.PropName,
                        Value = dataValue == null ? 0 : (double)dataValue
                    };

                    simData.Add(simDataItem);
                    i++;
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

                var i = 0;
                lock (_simConnectHudBarDataDefinitions)
                {
                    foreach (var definition in _simConnectHudBarDataDefinitions)
                    {
                        if (definition.DataDefinitionType != DataDefinitionType.SimConnect)
                            continue;

                        var dataValue = _simConnectStructFields[i].GetValue(simDataStruct);
                        var simDataItem = new SimDataItem
                        {
                            PropertyName = definition.PropName,
                            Value = dataValue == null ? 0 : (double)dataValue
                        };

                        simData.Add(simDataItem);
                        i++;
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


        private void ParseDynamicLodReceivedSimData(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            try
            {
                if (_simConnectDynamicLodDataDefinitions == null)
                    return;

                var simData = new List<SimDataItem>();
                var simDataStruct = (SimConnectStruct)data.dwData[0];

                var i = 0;
                lock (_simConnectDynamicLodDataDefinitions)
                {
                    foreach (var definition in _simConnectDynamicLodDataDefinitions)
                    {
                        if (definition.DataDefinitionType != DataDefinitionType.SimConnect)
                            continue;

                        var dataValue = _simConnectStructFields[i].GetValue(simDataStruct);
                        var simDataItem = new SimDataItem
                        {
                            PropertyName = definition.PropName,
                            Value = dataValue == null ? 0 : (double)dataValue
                        };

                        simData.Add(simDataItem);
                        i++;
                    }
                }

                OnReceivedDynamicLodData?.Invoke(this, simData);
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"SimConnector: SimConnect received dynamic lod data exception - {ex.Message}", ex);
                StopAndReconnect();
            }
        }

        private void SetActiveAircraftTitle(string aircraftFilePath)
        {
            var filePathToken = aircraftFilePath.Split(@"\");

            if (filePathToken.Length > 1)
            {
                var aircraftName = filePathToken[^2];
                aircraftName = aircraftName.Replace("_", " ").ToUpper();

                OnActiveAircraftChanged?.Invoke(this, aircraftName);
            }
        }
    }
}