using Microsoft.FlightSimulator.SimConnect;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace MSFSPopoutPanelManager.TouchPanel.FSConnector
{
    public class SimConnector
    {
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 4000;
        private const int WM_USER_SIMCONNECT = 0x0402;

        private SimConnect _simConnect;
        private MobiFlightWasmClient _mobiFlightWasmClient;
        private Timer _connectionTimer;
        private SimConnectSystemEvent _lastSimConnectSystemEvent;
        private bool _isInActiveFlightSession;

        public event EventHandler<EventArgs<string>> OnException;
        public event EventHandler<EventArgs<List<SimConnectDataDefinition>>> OnReceivedData;
        public event EventHandler<EventArgs<string>> OnReceiveSystemEvent;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnFlightSessionStarted;
        public event EventHandler OnFlightSessionStopped;

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

        public void ResetSimConnectDataArea(string planeId)
        {
            if (_simConnect != null)
            {
                _simConnect.ClearDataDefinition(SIMCONNECT_DATA_DEFINITION.SIMCONNECT_DATA_STRUCT);

                if (!String.IsNullOrEmpty(planeId))
                {
                    SimConnectDataDefinitions = ConfigurationReader.GetSimConnectDataDefinitions(planeId);
                    AddDataDefinitions();
                }
                else
                {
                    SimConnectDataDefinitions = null;
                    // Clear memory cache data to send back to ReactJs app
                    OnReceivedData?.Invoke(this, new EventArgs<List<SimConnectDataDefinition>>(null));
                }
            }
        }

        private void HandleOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Connected = true;

            _mobiFlightWasmClient = new MobiFlightWasmClient(_simConnect);
            _mobiFlightWasmClient.Initialize();

            AddDataDefinitions();

            _mobiFlightWasmClient.Ping();

            OnConnected?.Invoke(this, null);
        }

        public void StopAndReconnect()
        {
            Stop();
            InitializeSimConnect();
        }

        public void Stop()
        {
            _isInActiveFlightSession = false;

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
            if (_simConnect == null || !Connected || !_mobiFlightWasmClient.Connected)
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
                    OnException?.Invoke(this, new EventArgs<string>($"SimConnect receive message exception: {ex.Message}"));
            }
        }

        public void SetSimVar(String simVarCode)
        {
            if (_simConnect == null || !Connected || _mobiFlightWasmClient == null || !_mobiFlightWasmClient.Connected)
                return;

            _mobiFlightWasmClient.SetSimVar(simVarCode);
        }

        public void SetEventID(string eventID, uint value)
        {
            if (_simConnect == null || !Connected || _mobiFlightWasmClient == null || !_mobiFlightWasmClient.Connected)
                return;

            _mobiFlightWasmClient.SetEventID(eventID, value);
        }

        private void InitializeSimConnect()
        {
            // The constructor is similar to SimConnect_Open in the native API
            _simConnect = new SimConnect("Simconnect - Simvar test", Process.GetCurrentProcess().MainWindowHandle, WM_USER_SIMCONNECT, null, 0);

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

            System.Threading.Thread.Sleep(2000);
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
                        if (_mobiFlightWasmClient != null)
                            _mobiFlightWasmClient.GetSimVar($"(A:{definition.VariableName})");
                        break;
                    case DataDefinitionType.LVar:
                        if (_mobiFlightWasmClient != null)
                            _mobiFlightWasmClient.GetSimVar($"(L:{definition.VariableName})");
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

                        _simConnect.AddToDataDefinition(SIMCONNECT_DATA_DEFINITION.SIMCONNECT_DATA_STRUCT, definition.VariableName, definition.SimConnectUnit, simmConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                        break;
                }
            }

            // register simConnect data structure
            _simConnect.RegisterDataDefineStruct<SimConnectStruct>(SIMCONNECT_DATA_DEFINITION.SIMCONNECT_DATA_STRUCT);
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
                case SIMCONNECT_EXCEPTION.ALREADY_CREATED:
                case SIMCONNECT_EXCEPTION.UNRECOGNIZED_ID:
                case SIMCONNECT_EXCEPTION.EVENT_ID_DUPLICATE:
                    break;
                default:
                    TouchPanelLogger.ServerLog("SimConnectCache::Exception " + e.ToString(), LogLevel.ERROR);
                    break;
            }
        }

        private void HandleOnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_simConnect == null || !Connected || !_mobiFlightWasmClient.Connected || SimConnectDataDefinitions == null)
                return;

            if (data.dwRequestID != 0)
                TouchPanelLogger.ServerLog($"SimConnect unknown request ID: {data.dwRequestID}", LogLevel.ERROR);

            try
            {
                var simConnectStruct = (SimConnectStruct)data.dwData[0];
                var simConnectStructFields = typeof(SimConnectStruct).GetFields();

                int i = 0;
                foreach (var definition in SimConnectDataDefinitions)
                {
                    switch (definition.DataDefinitionType)
                    {
                        case DataDefinitionType.AVar:
                            definition.Value = _mobiFlightWasmClient.GetSimVar($"(A:{definition.VariableName})");
                            break;
                        case DataDefinitionType.LVar:
                            definition.Value = _mobiFlightWasmClient.GetSimVar($"(L:{definition.VariableName})");
                            break;
                        case DataDefinitionType.SimConnect:
                            definition.Value = simConnectStructFields[i++].GetValue(simConnectStruct); // increment structure counter after assignment
                            break;
                    }
                }

                OnReceivedData?.Invoke(this, new EventArgs<List<SimConnectDataDefinition>>(SimConnectDataDefinitions));
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"SimConnect receive data exception: {ex.Message}", LogLevel.ERROR);
            }
        }

        private void HandleOnReceiveSystemEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var systemEvent = ((SimConnectSystemEvent)data.uEventID);

            // Detect flight session starts and ends
            if (_lastSimConnectSystemEvent == SimConnectSystemEvent.SIMSTART && systemEvent == SimConnectSystemEvent.VIEW)
            {
                _isInActiveFlightSession = true;
                _lastSimConnectSystemEvent = SimConnectSystemEvent.NONE;
                OnFlightSessionStarted?.Invoke(this, null);

                if (_mobiFlightWasmClient.Connected)
                    _mobiFlightWasmClient.GetLVarList();

                return;
            }

            // look for pair of events denoting sim ended after sim is active
            if ((_isInActiveFlightSession && _lastSimConnectSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.VIEW) ||
                (_isInActiveFlightSession && _lastSimConnectSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.SIMSTART))
            {
                _isInActiveFlightSession = false;
                _lastSimConnectSystemEvent = SimConnectSystemEvent.NONE;
                OnFlightSessionStopped?.Invoke(this, null);
                return;
            }

            _lastSimConnectSystemEvent = systemEvent;

            OnReceiveSystemEvent?.Invoke(this, new EventArgs<string>(systemEvent.ToString()));
        }
    }
}