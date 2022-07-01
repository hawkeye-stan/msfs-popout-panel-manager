using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.FsConnector
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SimConnectStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
        public string Prop01;

        public double Prop02;
        public double Prop03;
        public double Prop04;
    }
}
