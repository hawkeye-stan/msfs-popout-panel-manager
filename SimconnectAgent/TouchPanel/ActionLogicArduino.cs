using MSFSPopoutPanelManager.ArduinoAgent;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class ActionLogicArduino
    {
        private const string NO_ACTION = "NO_ACTION";

        public static CommandAction GetSimConnectCommand(SimConnectEncoderAction encoderAction, InputName inputName, InputAction inputAction)
        {
            switch (inputName)
            {
                case InputName.EncoderLower:
                    switch (inputAction)
                    {
                        case InputAction.CW:
                            return new CommandAction(encoderAction.EncoderLowerCW, encoderAction.ActionType);
                        case InputAction.CCW:
                            return new CommandAction(encoderAction.EncoderLowerCCW, encoderAction.ActionType);
                        case InputAction.SW:
                            return new CommandAction(encoderAction.EncoderLowerSwitch, encoderAction.ActionType);
                    }
                    break;
                case InputName.EncoderUpper:
                    switch (inputAction)
                    {
                        case InputAction.CW:
                            return new CommandAction(encoderAction.EncoderUpperCW, encoderAction.ActionType);
                        case InputAction.CCW:
                            return new CommandAction(encoderAction.EncoderUpperCCW, encoderAction.ActionType);
                        case InputAction.SW:
                            return new CommandAction(encoderAction.EncoderUpperSwitch, encoderAction.ActionType);
                    }
                    break;
                case InputName.Joystick:
                    switch (inputAction)
                    {
                        case InputAction.UP:
                            return new CommandAction(encoderAction.Joystick1Up, encoderAction.ActionType);
                        case InputAction.DOWN:
                            return new CommandAction(encoderAction.Joystick1Down, encoderAction.ActionType);
                        case InputAction.LEFT:
                            return new CommandAction(encoderAction.Joystick1Left, encoderAction.ActionType);
                        case InputAction.RIGHT:
                            return new CommandAction(encoderAction.Joystick1Right, encoderAction.ActionType);
                        case InputAction.SW:
                            return new CommandAction(encoderAction.Joystick1Switch, encoderAction.ActionType);
                    }
                    break;
            }

            return new CommandAction(NO_ACTION, SimConnectActionType.EncoderAction);
        }
    }
}
