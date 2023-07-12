namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnectDataDefinition
    {
        public DATA_DEFINITION DefinitionId { get; set; }

        public DATA_REQUEST RequestId { get; set; }

        public string PropName { get; set; }

        public string VariableName { get; set; }

        public string SimConnectUnit { get; set; }

        public DataType DataType { get; set; }

        public DataDefinitionType DataDefinitionType { get; set; }

        public object Value { get; set; }
    }

    public enum DataDefinitionType
    {
        AVar,
        LVar,
        SimConnect,
        SimEvent
    }

    public enum DataType
    {
        String,
        Float64,
        Default
    }
}
