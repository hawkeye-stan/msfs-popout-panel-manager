using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnector
    {
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private const int WM_USER_SIMCONNECT = 0x0402;

        private SimConnect _simConnect;
        private Timer _connectionTimer;
        private bool _isDisabledReconnect;

        public event EventHandler<string> OnException;
        public event EventHandler<List<SimConnectDataDefinition>> OnReceivedData;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<SimConnectSystemEvent> OnReceiveSystemEvent;

        public bool Connected { get; set; }

        public List<SimConnectDataDefinition> SimConnectDataDefinitions;

        public void Start()
        {
            _connectionTimer = new Timer();
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

        public void Restart()
        {
            _connectionTimer.Enabled = true;
        }

        private void HandleOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            ReceiveMessage();
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
            }

            Connected = false;
        }

        public void RequestData()
        {
            if (_simConnect == null || !Connected)
                return;

            foreach (var definition in SimConnectDataDefinitions)
            {
                if (definition.DataDefinitionType == DataDefinitionType.SimConnect)
                    _simConnect.RequestDataOnSimObjectType(definition.RequestId, definition.DefineId, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
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
                    // Need to stop and reconnect server since the data is SimConnect connection or data is probably corrupted.
                    StopAndReconnect();
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
                }
                catch (Exception ex)
                {
                    var eventName = eventID.ToString()[4..];        // trim out KEY_ prefix
                    FileLogger.WriteLog($"SimConnector: SimConnect transmit event exception - EventName: {eventName} - {ex.Message}", StatusMessageType.Error);
                }
            }
        }

        public void SetDataObject(SIMCONNECT_DEFINE_ID defineId, object dValue)
        {
            if (_simConnect != null)
            {
                try
                {
                    _simConnect.SetDataOnSimObject(defineId, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dValue);
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLog($"SimConnector: Unable to set SimConnect variable - {ex.Message}", StatusMessageType.Error);
                }
            }
        }

        private int _connectRetryCount = 0;

        private void InitializeSimConnect()
        {
            StatusMessageWriter.WriteMessage($"Connecting to MSFS {new StringBuilder().Insert(0, ". ", _connectRetryCount++ % 20).ToString()}", StatusMessageType.Info, false);

            _simConnect = new SimConnect("MSFS Pop Out Panel Manager", Process.GetCurrentProcess().MainWindowHandle, WM_USER_SIMCONNECT, null, 0);

            _connectRetryCount = 0;
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
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.SIMSTART);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.SIMSTART, "SimStart");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.SIMSTOP);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.SIMSTOP, "SimStop");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.VIEW);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.VIEW, "View");
            _simConnect.UnsubscribeFromSystemEvent(SimConnectSystemEvent.AIRCRAFTLOADED);
            _simConnect.SubscribeToSystemEvent(SimConnectSystemEvent.AIRCRAFTLOADED, "AircraftLoaded");

            AddDataDefinitions();

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
            StatusMessageWriter.WriteMessage("MSFS is connected", StatusMessageType.Info, false);
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
                case (uint)SimConnectSystemEvent.AIRCRAFTLOADED:
                    SetActiveAircraftTitle(data.szFileName);
                    break;
            }
        }

        private void AddDataDefinitions()
        {
            int definitionId = 1;
            int requestId = 1;

            SimConnectDataDefinitions = DefaultSimConnectDataDefinition.GetDefinitions();

            foreach (var definition in SimConnectDataDefinitions)
            {
                if (definition.DataDefinitionType == DataDefinitionType.SimConnect)
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

                    _simConnect.AddToDataDefinition((SIMCONNECT_DEFINE_ID)definitionId, definition.VariableName, definition.SimConnectUnit, simmConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                    // Assign definition id and request id back into definition object
                    definition.DefineId = (SIMCONNECT_DEFINE_ID)definitionId;
                    definition.RequestId = (SIMCONNECT_REQUEST)requestId;

                    if (definition.DataType == DataType.String)
                        _simConnect.RegisterDataDefineStruct<SimConnectStruct>(definition.DefineId);
                    else
                        _simConnect.RegisterDataDefineStruct<double>(definition.DefineId);

                    definitionId++;
                    requestId++;
                }
            }

            // Setup SimEvent mapping
            foreach (var item in Enum.GetValues(typeof(ActionEvent)))
            {
                if (item.ToString().StartsWith("KEY_"))
                    _simConnect.MapClientEventToSimEvent((ActionEvent)item, item.ToString()[4..]);
            }
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

        private void HandleOnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnect == null || !Connected || SimConnectDataDefinitions == null)
                return;

            try
            {
                foreach (var definition in SimConnectDataDefinitions)
                {
                    if (data.dwRequestID == (uint)definition.RequestId)
                    {
                        if (definition.DataType == DataType.String)
                        {
                            definition.Value = ((SimConnectStruct)data.dwData[0]).SValue;
                        }
                        else
                        {
                            definition.Value = (double)data.dwData[0];
                        }
                    }
                }

                OnReceivedData?.Invoke(this, SimConnectDataDefinitions);
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"SimConnector: SimConnect receive data exception - {ex.Message}", ex);
            }
        }

        private void HandleOnReceiveSystemEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var systemEvent = ((SimConnectSystemEvent)data.uEventID);
            OnReceiveSystemEvent?.Invoke(this, systemEvent);
        }

        private void SetActiveAircraftTitle(string aircraftFilePath)
        {
            var filePathToken = aircraftFilePath.Split(@"\");

            if (filePathToken.Length > 1)
            {
                var aircraftName = filePathToken[filePathToken.Length - 2];
                aircraftName = aircraftName.Replace("_", " ").ToUpper();

                SimConnectDataDefinitions.Find(s => s.PropName == "AircraftName").Value = aircraftName;

                OnReceivedData?.Invoke(this, SimConnectDataDefinitions);
            }
        }
    }
}