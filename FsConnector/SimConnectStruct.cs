
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
        public double Prop05;
        public double Prop06;
        public double Prop07;
        public double Prop08;
        public double Prop09;
        public double Prop10;

        // Add more as DataDefinition grows
    }
}
