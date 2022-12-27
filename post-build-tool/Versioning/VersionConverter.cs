using Newtonsoft.Json;

namespace PostBuildTool.Versioning
{
    public class VersionConverter : JsonConverter<BuildVersion>
    {
        public override BuildVersion ReadJson(JsonReader reader, Type objectType, BuildVersion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return BuildVersion.Parse(s);
            }
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, BuildVersion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}