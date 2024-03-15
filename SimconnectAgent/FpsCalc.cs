using System;
using System.Linq;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class FpsCalc
    {
        private const int FpsLen = 25;     
        private static readonly float[] FpsStatistic = new float[FpsLen];
        private static int _fpsIndex = -1;

        public static int GetAverageFps(int newValue)
        {
            if (_fpsIndex == -1)
            {
                for (var i = 0; i < FpsLen; i++)
                    FpsStatistic[i] = newValue;
                
                _fpsIndex = 1;
            }
            else
            {
                FpsStatistic[_fpsIndex] = newValue;
                _fpsIndex++;
                if (_fpsIndex >= FpsLen)
                    _fpsIndex = 0;
            }

            var fps = 0;
            if (_fpsIndex != -1)
                fps = Convert.ToInt32(FpsStatistic.Sum() / FpsLen);

            return fps;
        }
    }
}
