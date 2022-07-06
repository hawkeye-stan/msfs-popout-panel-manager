using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace MSFSPopoutPanelManager.Model
{
    public class UserProfile : ObservableObject
    {
        public UserProfile()
        {
            PanelSourceCoordinates = new ObservableCollection<PanelSourceCoordinate>();
            PanelConfigs = new ObservableCollection<PanelConfig>();
            BindingAircraftLiveries = new ObservableCollection<string>();
            IsLocked = false;
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        [JsonConverter(typeof(SingleValueArrayConvertor<string>))]
        public ObservableCollection<string> BindingAircraftLiveries { get; set; }

        public bool IsLocked { get; set; }

        public ObservableCollection<PanelSourceCoordinate> PanelSourceCoordinates { get; set; }

        public ObservableCollection<PanelConfig> PanelConfigs { get; set; }

        public bool PowerOnRequiredForColdStart { get; set; }

        public void Reset()
        {
            PanelSourceCoordinates.Clear();
            PanelConfigs.Clear();
            IsLocked = false;
        }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool HasBindingAircraftLiveries
        {
            get { return BindingAircraftLiveries.Count > 0; }
        }

        [JsonIgnore]
        public bool HasPanelSourceCoordinates
        {
            get { return PanelSourceCoordinates.Count > 0; }
        }

        #region Legacy Properties

        // Support pre-Version 3.3 tag for one time conversion
        [JsonConverter(typeof(SingleValueArrayConvertor<string>))]
        public ObservableCollection<string> BindingPlaneTitle
        {
            set { BindingAircraftLiveries = value; }
        }

        #endregion
    }

    public class SingleValueArrayConvertor<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object val = new object();

            if (reader.TokenType == JsonToken.String)
            {
                var instance = (string)serializer.Deserialize(reader, typeof(string));
                val = new ObservableCollection<string>() { instance };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                val = serializer.Deserialize(reader, objectType);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                val = new ObservableCollection<string>();
            }

            return val;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
