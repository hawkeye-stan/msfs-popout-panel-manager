using MSFSPopoutPanelManager.ArduinoAgent;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using StringMath;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class ActionProvider
    {
        private int MAX_ACTION_QUEUE_COMMAND = 50;

        private TouchPanelSimConnector _simConnector;
        private bool _isSimConnected;
        private ConcurrentQueue<CommandAction> _actionQueue;
        private bool _isUsedArduino;

        private SimConnectEncoderAction _currentSimConnectEncoderAction;
        private System.Timers.Timer _actionExecutionTimer;

        public ActionProvider(TouchPanelSimConnector simConnector, bool isUsedArduino)
        {
            _currentSimConnectEncoderAction = null;
            _isSimConnected = false;
            _simConnector = simConnector;
            _isUsedArduino = isUsedArduino;
            _actionQueue = new ConcurrentQueue<CommandAction>();

            _actionExecutionTimer = new System.Timers.Timer();
            _actionExecutionTimer.Interval = 25;
            _actionExecutionTimer.Enabled = true;
            _actionExecutionTimer.Elapsed += (sender, e) =>
                {
                    CommandAction command;
                    _actionQueue.TryDequeue(out command);
                    if (command != null)
                        ExecuteCommand(command);

                    if (_actionQueue.Count == 0)
                        _actionExecutionTimer.Enabled = false;
                };
        }

        public void Start()
        {
            _isSimConnected = true;
        }

        public void Stop()
        {
            _isSimConnected = false;
        }

        public void ExecAction(SimConnectActionData actionData)
        {
            CommandAction commandAction;

            if (_isSimConnected && actionData.Action != null)
            {
                try
                {
                    if (actionData.Action == "NO_ACTION") return;

                    // clear encoder actions on each new SimConnect action submitted other than actual encoder movement
                    if (actionData.ActionType != SimConnectActionType.EncoderAction)
                        _currentSimConnectEncoderAction = actionData.EncoderAction;

                    switch (actionData.Action)
                    {
                        case "LOWER_ENCODER_INC":
                            if (_currentSimConnectEncoderAction == null)
                                return;
                            commandAction = new CommandAction(_currentSimConnectEncoderAction.EncoderLowerCW, _currentSimConnectEncoderAction.ActionType);
                            break;
                        case "LOWER_ENCODER_DEC":
                            if (_currentSimConnectEncoderAction == null)
                                return;
                            commandAction = new CommandAction(_currentSimConnectEncoderAction.EncoderLowerCCW, _currentSimConnectEncoderAction.ActionType);
                            break;
                        case "UPPER_ENCODER_INC":
                            if (_currentSimConnectEncoderAction == null)
                                return;
                            commandAction = new CommandAction(_currentSimConnectEncoderAction.EncoderUpperCW, _currentSimConnectEncoderAction.ActionType);
                            break;
                        case "UPPER_ENCODER_DEC":
                            if (_currentSimConnectEncoderAction == null)
                                return;
                            commandAction = new CommandAction(_currentSimConnectEncoderAction.EncoderUpperCCW, _currentSimConnectEncoderAction.ActionType);
                            break;
                        case "ENCODER_PUSH":
                            if (_currentSimConnectEncoderAction == null)
                                return;
                            commandAction = new CommandAction(_currentSimConnectEncoderAction.EncoderLowerSwitch, _currentSimConnectEncoderAction.ActionType);
                            break;
                        default:
                            commandAction = new CommandAction(actionData.Action, actionData.ActionType, Convert.ToUInt16(actionData.ActionValue));
                            if (_isUsedArduino)
                            {
                                Task.Run(() =>
                                {
                                    Thread.Sleep(500);
                                    InputEmulationManager.LeftClickGameWindow();
                                });
                            }
                            break;
                    }

                    ExecuteCommand(commandAction);
                }
                catch (Exception ex)
                {
                    FileLogger.WriteException(ex.Message, ex);
                }
            }
        }

        public void ArduinoInputHandler(object sender, ArduinoInputData e)
        {
            if (_actionQueue.Count < MAX_ACTION_QUEUE_COMMAND && _currentSimConnectEncoderAction != null)
            {
                _actionExecutionTimer.Enabled = true;

                var commandAction = ActionLogicArduino.GetSimConnectCommand(_currentSimConnectEncoderAction, e.InputName, e.InputAction);

                if (e.Acceleration == 1)
                {
                    _actionQueue.Enqueue(commandAction);
                }
                else
                {
                    for (var a = 0; a < e.Acceleration; a++)
                    {
                        _actionQueue.Enqueue(commandAction);
                    }
                }
            }
        }

        private void ExecuteCommand(CommandAction commandAction)
        {
            string simConnectCommand = commandAction.Action;
            SimConnectActionType simConnectCommandType = commandAction.ActionType;
            uint simConnectCommandValue = commandAction.ActionValue;

            if (string.IsNullOrEmpty(simConnectCommand))
                return;

            switch (simConnectCommandType)
            {
                case SimConnectActionType.SimEventId:
                    _simConnector.SetEventID(simConnectCommand, simConnectCommandValue);
                    break;
                case SimConnectActionType.SimVarCode:
                    // Match content between slash + curly braces for variable and to be replaced by simConnect data. For example: "/{Plane_Speed/} (>L:Plane_Speed)" 
                    const string pattern = @"(?<=\/{)[^}]*(?=\/})";
                    foreach (var match in Regex.Matches(simConnectCommand, pattern))
                    {
                        var variableName = match.ToString();
                        var variable = _simConnector.SimConnectDataDefinitions.Find(x => x.PropName == variableName);
                        var variableValue = variable != null ? variable.Value : 0;

                        simConnectCommand = simConnectCommand.Replace("{" + match.ToString() + "}", variableValue.ToString());

                        // Find any math string that needs evaluation   For example: "[/{Plane_Speed/} + 100] (>L:Plane_Speed)" 
                        const string mathStringPattern = @"(?<=\[).+?(?=\])";
                        foreach (var mathStringMatch in Regex.Matches(simConnectCommand, mathStringPattern))
                        {
                            var myCalculator = new Calculator();
                            var result = myCalculator.Evaluate(mathStringMatch.ToString()).ToString();

                            simConnectCommand = simConnectCommand.Replace("[" + mathStringMatch.ToString() + "]", result);
                        }
                    }

                    _simConnector.SetSimVar(simConnectCommand);
                    break;
            }
        }

        private void RefocusMsfs()
        {
            Thread.Sleep(250);
            InputEmulationManager.LeftClickGameWindow();
        }
    }
}