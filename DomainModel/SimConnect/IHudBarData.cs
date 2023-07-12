using MSFSPopoutPanelManager.DomainModel.Profile;

namespace MSFSPopoutPanelManager.DomainModel.SimConnect
{
    public interface IHudBarData
    {
        public HudBarType HudBarType { get; }

        public double ElevatorTrim { get; set; }

        public double AileronTrim { get; set; }

        public double RudderTrim { get; set; }

        public double Flap { get; set; }

        public double ParkingBrake { get; set; }

        public double GearLeft { get; set; }

        public double GearCenter { get; set; }

        public double GearRight { get; set; }

        public double SimRate { get; set; }

        public string ParkingBrakeFormatted { get; }

        public string GearLeftFormatted { get; }

        public string GearCenterFormatted { get; }

        public string GearRightFormatted { get; }

        public string ElevatorTrimFormatted { get; }

        public string AileronTrimFormatted { get; }

        public string RudderTrimFormatted { get; }

        public string FlapFormatted { get; }

        public void Clear();
    }
}
