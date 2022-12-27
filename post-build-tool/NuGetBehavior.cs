using Newtonsoft.Json;

using PostBuildTool.Converters;

namespace PostBuildTool
{
    [JsonConverter(typeof(EnumToStringConverter<NuGetBehavior>))]
    public enum NuGetBehavior
    {
        QuitError,
        QuitNoError,
        ContinueIgnore
    }

    [JsonConverter(typeof(EnumToStringConverter<PackageConstraint>))]
    public enum PackageConstraint
    {
        Release,
        Debug,
        DontCare
    }
}