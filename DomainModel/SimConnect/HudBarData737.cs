using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Reflection;

namespace MSFSPopoutPanelManager.DomainModel.SimConnect
{
    public class HudBarData737 : ObservableObject, IHudBarData
    {
        public HudBarType HudBarType => HudBarType.PMDG_737;

        public double ElevatorTrim { get; set; }

        public double AileronTrim { get; set; }

        public double RudderTrim { get; set; }

        public double Flap { get; set; }

        public double ParkingBrake { get; set; }

        public double GearLeft { get; set; }

        public double GearCenter { get; set; }

        public double GearRight { get; set; }

        public double SimRate { get; set; }

        public string ElevatorTrimFormatted => Math.Round(ElevatorTrim / 10, 2).ToString("F2");

        public string AileronTrimFormatted => Math.Round(AileronTrim / 16384 * 57, 2).ToString("F2");

        public string RudderTrimFormatted => Math.Round(RudderTrim * 2 * 10, 2).ToString("F2");

        public string FlapFormatted => MapFlap(Flap);

        public string ParkingBrakeFormatted => ParkingBrake > 0 ? "Engaged" : "Disengaged";

        public string GearLeftFormatted => MapGear(GearLeft);

        public string GearCenterFormatted => MapGear(GearCenter);

        public string GearRightFormatted => MapGear(GearRight);

        private string MapFlap(double flap)
        {
            switch (Convert.ToInt32(flap))
            {
                case 0:
                    return "0";
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 3:
                    return "5";
                case 4:
                    return "10";
                case 5:
                    return "15";
                case 6:
                    return "25";
                case 7:
                    return "30";
                case 8:
                    return "40";
                default:
                    return "5";
            }
        }

        private string MapGear(double gear)
        {
            if (gear == 100)
                return "DOWN";
            if (gear == 0)
                return "UP";

            return "MOVING";
        }

        public void Clear()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; ++i)
            {
                if (properties[i].SetMethod != null)
                    properties[i].SetValue(this, 0);
            }
        }
    }
}
