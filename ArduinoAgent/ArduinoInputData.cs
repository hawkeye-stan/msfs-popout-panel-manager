using System;

namespace MSFSPopoutPanelManager.ArduinoAgent
{
    public class ArduinoInputData
    {
        public ArduinoInputData(string inputName, string inputAction, int acceleration)
        {
            InputName = (InputName)Enum.Parse(typeof(InputName), inputName);
            InputAction = (InputAction)Enum.Parse(typeof(InputAction), inputAction);
            Acceleration = acceleration;
        }

        public InputName InputName { get; set; }

        public InputAction InputAction { get; set; }

        public int Acceleration { get; set; }
    }

    public enum InputAction
    {
        NONE,

        // Rotary Encoder
        CW,
        CCW,
        SW,

        // Joystick
        UP,
        DOWN,
        LEFT,
        RIGHT,

        // Keypad
        Key1,
        Key2,
        Key3,
        Key4,
        Key5,
        Key6,
        Key7,
        Key8,
        Key9,
        Key0,
        KeyA,
        KeyB,
        KeyC,
        KeyD,
        KeyAsterisk,
        KeyPound
    }

    public enum InputName
    {
        EncoderLower,
        EncoderUpper,
        Joystick,
        Keypad
    }
}
