using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.DynamicLod
{
    public class DynamicLodSimData : ObservableObject
    {
        public int Fps { get; set; }

        public int Tlod { get; set; }

        public int Olod { get; set; }

        public double Agl { get; set; }

        public string CloudQuality { get; set; } = "N/A";

        public double AltAboveGround { get; set; }

        public double AltAboveGroundMinusCg { get; set; }

        public double GroundVelocity { get; set; }

        public bool PlaneOnGround { get; set; } = true;

        public void Clear()
        {
            Fps = 0;
            Tlod = 0;
            Olod = 0;
            Agl = 0;
            CloudQuality = "N/A";
            AltAboveGround = 0;
            AltAboveGroundMinusCg = 0;
            GroundVelocity = 0;
            PlaneOnGround = true;
        }
    }
}
