using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace MSFSPopoutPanelManager.Shared
{
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
