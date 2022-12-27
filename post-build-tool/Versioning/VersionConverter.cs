using Newtonsoft.Json;

using PostBuildTool.Contracts;

namespace PostBuildTool.Versioning
{
    internal class VersionConverter<T> : JsonConverter<T> where T : class, IBuildVersion, new()
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return IBuildVersion.Parse<T>(s);
            }

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}