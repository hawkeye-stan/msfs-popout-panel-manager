using Microsoft.FlightSimulator.SimConnect;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class MobiFlightWasmClient
    {
        public static void Ping(SimConnect simConnect)
        {
            if (simConnect == null) return;

            SendWasmCmd(simConnect, "MF.Ping");
            DummyCommand(simConnect);
        }

        public static void Stop(SimConnect simConnect)
        {
            if (simConnect == null)
                return;

            SendWasmCmd(simConnect, "MF.SimVars.Clear");
        }

        public static void GetLVarList(SimConnect simConnect)
        {
            if (simConnect == null) return;

            SendWasmCmd(simConnect, "MF.LVars.List");
            DummyCommand(simConnect);
        }

        public static void DummyCommand(SimConnect simConnect)
        {
            if (simConnect == null)
                return;

            SendWasmCmd(simConnect, "MF.DummyCmd");
        }

        public static void SendWasmCmd(SimConnect simConnect, string command)
        {
            if (simConnect == null)
                return;

            try
            {
                simConnect.SetClientData(
                    SIMCONNECT_CLIENT_DATA_ID.MOBIFLIGHT_CMD,
                   (SIMCONNECT_CLIENT_DATA_ID)0,
                   SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, 0,
                   new ClientDataString(command)
                );
            }
            catch { }
        }
    }
}
