using Newtonsoft.Json;

using PostBuildTool.Versioning;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PostBuildTool
{
    internal class SwitchAttribute : Attribute
    {
        public string Switch { get; set; }

        public SwitchAttribute(string sw)
        {
            Switch = sw;
        }
    }

    /// <summary>
    /// Program configuration options.
    /// </summary>
    public sealed class ProgramOptions
    {
        /// <summary>
        /// Open and update all .csproj files in the specified directory.
        /// </summary>
        [JsonProperty("directory")]
        [Switch("/d")]
        public string Directory { get; set; }

        /// <summary>
        /// Recursively scan the directory and sub-directories for .csproj files.
        /// </summary>
        [JsonProperty("recursive")]
        [Switch("/r")]
        public bool Recursive { get; set; }

        /// <summary>
        /// Open and update the specified project.
        /// </summary>
        [JsonProperty("project")]
        [Switch("/p")]
        public string Project { get; set; }

        /// <summary>
        /// Force the specified version number (as opposed to auto-incrementing.)
        /// </summary>
        [JsonProperty("version")]
        [Switch("/v")]
        public string Version { get; set; }

        /// <summary>
        /// Specify the PreviousVersion variable (as opposed to being automatically calculated.)
        /// </summary>
        [JsonProperty("oldVersion")]
        [Switch("/ov")]
        public string OldVersion { get; set; }

        /// <summary>
        /// Suppress all output.
        /// </summary>
        [JsonProperty("quiet")]
        [Switch("/q")]
        public bool Quiet { get; set; }

        /// <summary>
        /// Log output to the specified file.
        /// </summary>
        [JsonProperty("logFile")]
        [Switch("/l")]
        public string LogFile { get; set; }

        /// <summary>
        /// Use an IVersionifier instance from the specified DLL.
        /// </summary>
        [JsonProperty("library")]
        [Switch("/lib")]
        public string Library { get; set; }

        /// <summary>
        /// Optional class name to be used with /lib.
        /// </summary>
        [JsonProperty("className")]
        [Switch("/cn")]
        public string ClassName { get; set; }

        /// <summary>
        /// Displays this help screen.
        /// </summary>
        [JsonIgnore]
        [Switch("/h")]
        public bool DisplayHelp { get; set; }

        /// <summary>
        /// Set the versioning mode.
        /// </summary>
        [JsonProperty("mode")]
        [Switch("/m")]
        public VersionifyMode Mode { get; set; } = VersionifyMode.BumpBuild;

        /// <summary>
        /// Scan NuGet packages to determine most recent last version.
        /// </summary>
        [JsonProperty("ngo")]
        [Switch("/ngo")]
        public bool UseNuGetPackages { get; set; }

        /// <summary>
        /// Specify the NuGet package name (used with /ngo.)
        /// </summary>
        [JsonProperty("packageName")]
        [Switch("/pn")]
        public string PackageName { get; set; }

        /// <summary>
        /// Specify the NuGet package directory.
        /// </summary>
        [JsonProperty("packageDirectory")]
        [Switch("/ngd")]
        public string PackageDirectory { get; set; }

        /// <summary>
        /// Set the behavior if no NuGet packages are found.
        /// </summary>
        [JsonProperty("nugetFailBehavior")]
        [Switch("/ngb")]
        public NuGetBehavior NuGetFailBehavior { get; set; } = NuGetBehavior.QuitError;

        /// <summary>
        /// Set the NuGet package constraint.
        /// </summary>
        [JsonProperty("packageConstraint")]
        [Switch("/ngc")]
        public PackageConstraint PackageConstraint { get; set; } = PackageConstraint.Release;

        /// <summary>
        /// Report only. Do not write any changes.
        /// </summary>
        [JsonProperty("reportOnly")]
        [Switch("/nw")]
        public bool ReportOnly { get; set; }

        internal static ProgramOptions ParseFromArgs(Dictionary<string, CommandSwitch> parsed)
        {
            var props = typeof(ProgramOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var cfg = new ProgramOptions();

            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<SwitchAttribute>() is SwitchAttribute swa)
                {
                    if (parsed.TryGetValue(swa.Switch, out var cmd))
                    {
                        if (prop.PropertyType == typeof(bool))
                        {
                            prop.SetValue(cfg, true);
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(cfg, cmd.ArgumentValue);
                        }
                        else if (prop.PropertyType.IsEnum)
                        {
                            if (Enum.TryParse(prop.PropertyType, cmd.ArgumentValue, out var enumObj))
                            {
                                prop.SetValue(cfg, enumObj);
                            }
                            else
                            {
                                CommandSwitch.PrintHelp();
                            }
                        }
                    }
                }
            }

            return cfg;
        }
    }
}