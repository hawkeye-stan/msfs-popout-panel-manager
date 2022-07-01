namespace MSFSPopoutPanelManager.TouchPanel.Shared
{
    public class SimConnectDataDefinition
    {
        public string PropName { get; set; }

        public string VariableName { get; set; }

        public string SimConnectUnit { get; set; }

        public DataType DataType { get; set; }

        public DataDefinitionType DataDefinitionType { get; set; }

        public object DefaultValue { get; set; }

        public string JavaScriptFormatting { get; set; }

        public object Value { get; set; }
    }

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
