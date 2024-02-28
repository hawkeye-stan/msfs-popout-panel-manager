using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class SimConnectStruct
    {
        public double Prop0;
        public double Prop1;
        public double Prop2;
        public double Prop3;
        public double Prop4;
        public double Prop5;
        public double Prop6;
        public double Prop7;
        public double Prop8;
        public double Prop9;
        public double Prop10;
        public double Prop11;
        public double Prop12;
        public double Prop13;
        public double Prop14;
        public double Prop15;
        public double Prop16;
        public double Prop17;
        public double Prop18;
        public double Prop19;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class WritableDataStruct
    {
        public double Prop0;
    }
}
