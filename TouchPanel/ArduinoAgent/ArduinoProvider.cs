using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace MSFSPopoutPanelManager.TouchPanel.ArduinoAgent
{
    public class ArduinoProvider
    {
        private const string ARDUINO_COM_PORT = "COM3";
        private const int ARDUINO_BAUD_RATE = 9600;

        private SerialPort _serialPort;

        public event EventHandler<EventArgs<bool>> OnConnectionChanged;
        public event EventHandler<EventArgs<ArduinoInputData>> OnDataReceived;

        public ArduinoProvider()
        {
            try
            {
                _serialPort = new SerialPort
                {
                    PortName = ARDUINO_COM_PORT,
                    BaudRate = ARDUINO_BAUD_RATE
                };
                _serialPort.DataReceived += DataReceived;
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"Arduino Error: {ex.Message}", LogLevel.ERROR);
            }
        }

        public void Start()
        {
            if (!IsConnected)
            {
                try
                {
                    _serialPort.Open();
                    OnConnectionChanged?.Invoke(this, new EventArgs<bool>(true));
                    TouchPanelLogger.ServerLog("Arduino connected", LogLevel.INFO);
                }
                catch (Exception ex)
                {
                    TouchPanelLogger.ServerLog($"Arduino Connection Error - {ex.Message}", LogLevel.ERROR);
                }
            }
        }

        public void Stop()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                    TouchPanelLogger.ServerLog("Arduino disconnected", LogLevel.INFO);
                }
                catch (Exception ex)
                {
                    TouchPanelLogger.ServerLog($"Arduino Disconnect Error - {ex.Message}", LogLevel.ERROR);
                }
            }

            OnConnectionChanged?.Invoke(this, new EventArgs<bool>(false));
        }

        public void SendToArduino(string data)
        {
            try
            {
                if (IsConnected)
                    _serialPort.WriteLine(data);
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"Arduino Connection Error: {ex.Message}", LogLevel.ERROR);
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    var dataEvents = new List<ArduinoInputData>();

                    int byteToRead = _serialPort.BytesToRead;

                    while (byteToRead > 0)
                    {

                        var message = _serialPort.ReadTo("\r\n");
                        var data = message.Split(":");

                        ArduinoInputData dataEvent;

                        // Calculate acceleration
                        if (data.Length == 3)
                        {
                            var accelerationValue = Convert.ToInt32(data[2]);
                            //dataEvent = new ArduinoInputData(data[0], data[1], accelerationValue == 1 ? 1 : accelerationValue / 2);
                            dataEvent = new ArduinoInputData(data[0], data[1], accelerationValue <= 2 ? 1 : accelerationValue);
                        }
                        else
                        {
                            dataEvent = new ArduinoInputData(data[0], data[1], 1);
                        }

                        dataEvents.Add(dataEvent);

                        byteToRead = _serialPort.BytesToRead;
                    }

                    foreach (var evt in dataEvents)
                    {
                        OnDataReceived?.Invoke(this, new EventArgs<ArduinoInputData>(evt));
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    _serialPort.DiscardInBuffer();
                }
            }
        }

        private bool IsConnected
        {
            get
            {
                if (_serialPort == null)
                    return false;

                return _serialPort.IsOpen;
            }
        }
    }
}
