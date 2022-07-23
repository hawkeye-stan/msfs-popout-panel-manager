namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnectDataDefinition
    {
        public SIMCONNECT_DEFINE_ID DefineId = SIMCONNECT_DEFINE_ID.Dummy;

        public SIMCONNECT_REQUEST RequestId = SIMCONNECT_REQUEST.Dummy;

        public string PropName { get; set; }

        public string VariableName { get; set; }

        public string SimConnectUnit { get; set; }

        public DataType DataType { get; set; }

        public DataDefinitionType DataDefinitionType { get; set; }

        public object DefaultValue { get; set; }

        public string JavaScriptFormatting { get; set; }

        public object Value { get; set; }
    }

    public enum SIMCONNECT_DEFINE_ID
    {
        Dummy = 0
    }

    public enum SIMCONNECT_REQUEST
    {
        Dummy = 0,
        SimConnectStruct
    };

    public enum DataDefinitionType
    {
        AVar,
        LVar,
        SimConnect
    }

    public enum DataType
    {
        String,
        Float64,
        Default
    }
}
