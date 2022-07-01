using Newtonsoft.Json;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.TouchPanel.SimConnectAgent
{
    public class FlightPlan
    {
        public int activeLegIndex { get; set; }

        public List<ATCWaypoint> waypoints { get; set; }

        public int dtk { get; set; }
    }

    public class ATCWaypoint
    {
        public string id { get; set; }

        public string description { get; set; }

        public int index { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        public double[] latLong { get; set; }

        public double[] startLatLong { get; set; }

        public int? altitude { get; set; }

        public int? maxSpeed { get; set; }

        public double? distance { get; set; }

        public int? course { get; set; }

        public bool isActiveLeg { get; set; }
    }
}
