using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class SimConnectActionData
    {
        public string Action { get; set; }

        public SimConnectActionType ActionType { get; set; }

        public int ActionValue { get; set; }

        public SimConnectEncoderAction EncoderAction { get; set; }
    }

    public class SimConnectEncoderAction
    {
        public string EncoderLowerCW { get; set; }

        public string EncoderLowerCCW { get; set; }

        public string EncoderLowerSwitch { get; set; }

        public string EncoderUpperCW { get; set; }

        public string EncoderUpperCCW { get; set; }

        public string EncoderUpperSwitch { get; set; }

        public string Joystick1Up { get; set; }

        public string Joystick1Down { get; set; }

        public string Joystick1Left { get; set; }

        public string Joystick1Right { get; set; }

        public string Joystick1Switch { get; set; }

        public SimConnectActionType ActionType { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SimConnectActionType
    {
        SimEventId,
        SimVarCode,
        EncoderAction
    }

    public class CommandAction
    {
        public CommandAction()
        {
            ActionValue = 1;
            ActionType = SimConnectActionType.SimEventId;
        }

        public CommandAction(string action, SimConnectActionType actionType)
        {
            Action = action;
            ActionValue = 1;
            ActionType = actionType;
        }

        public CommandAction(string action, SimConnectActionType actionType, uint actionValue)
        {
            Action = action;
            ActionValue = actionValue;
            ActionType = actionType;
        }

        public string Action { get; set; }

        public SimConnectActionType ActionType { get; set; }

        public uint ActionValue { get; set; }
    }
}
