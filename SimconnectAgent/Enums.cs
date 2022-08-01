using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClientDataValue
    {
        public float data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClientDataString
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] data;

        public ClientDataString(string strData)
        {
            byte[] txtBytes = Encoding.ASCII.GetBytes(strData);
            var ret = new byte[1024];
            Array.Copy(txtBytes, ret, txtBytes.Length);
            data = ret;
        }
    }

    //public enum SIMCONNECT_DATA_DEFINITION
    //{
    //    SIMCONNECT_DATA_STRUCT
    //}

    public enum SIMCONNECT_DATA_DEFINITION_TOUCHPANEL
    {
        SIMCONNECT_DATA_STRUCT_TOUCHPANEL
    }

    public enum DATA_REQUEST
    {
        REQUEST_1
    }

    public enum NotificationGroup
    {
        GROUP0
    }

    public enum SimConnectSystemEvent
    {
        FOURSECS,
        SIMSTART,
        SIMSTOP,
        FLIGHTLOADED,
        AIRCRAFTLOADED,
        PAUSED,
        VIEW,
        NONE
    };
}
