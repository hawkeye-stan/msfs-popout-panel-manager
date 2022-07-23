using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class SimConnectStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string SValue;
    }
}
