using System;
using System.Linq;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class FpsCalc
    {
        private const int FPS_LEN = 5;     
        private static readonly float[] FpsStatistic = new float[FPS_LEN];
        private static int _fpsIndex = -1;
        private static int _avgFps;
        private static int _ignoreFpsSpikeCount = 0;

        public static int GetAverageFps(int newValue)
        {
            if (_fpsIndex == -1)
            {
                // initialize FpsStatistic array
                for (var i = 0; i < FPS_LEN; i++)
                    FpsStatistic[i] = newValue;

                _avgFps = Convert.ToInt32(newValue);
                _fpsIndex = 1;
            }
            else
            {
                var isSpike = Math.Abs(newValue - _avgFps) / (_avgFps * 1.0) > 0.1;

                if (_ignoreFpsSpikeCount != 3 && isSpike) // if new FPS spikes more than 10%, ignore the value
                {
                    _ignoreFpsSpikeCount++;
                    return _avgFps;
                }

                _ignoreFpsSpikeCount = 0;

                var deltaFps = newValue - _avgFps;

                if (deltaFps < 0 && Math.Abs(deltaFps) > _avgFps * 0.1)     // FPS suddenly drops more than 10%
                {
                    newValue += Math.Abs(deltaFps) / 2;   // Let the new FPS to be only half the delta drop
                }

                FpsStatistic[_fpsIndex] = newValue;
                _fpsIndex++;
                if (_fpsIndex >= FPS_LEN)
                    _fpsIndex = 0;

                _avgFps = Convert.ToInt32(FpsStatistic.Sum() / FPS_LEN);
            }
            
            return _avgFps;
        }

        public static void Reset()
        {
            _fpsIndex = -1;
            _avgFps = 0;
        }
    }
}
