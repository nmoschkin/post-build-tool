using Newtonsoft.Json;

using System;
using System.Linq;
using System.Text;

namespace post_build_tool
{
    public class Version : IEquatable<Version>
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Revision { get; set; }

        public int? Build { get; set; } = null;

        public static int GetHourOfYear()
        {
            var d = DateTime.Now.DayOfYear * 24;
            d += DateTime.Now.Hour;

            return d;
        }

        public static int GetMinuteOfDay()
        {
            return (int)DateTime.Now.TimeOfDay.TotalMinutes;
        }

        public bool Equals(Version other)
        {
            return Major == other.Major && Minor == other.Minor && Revision == other.Revision && Build == other.Build;
        }

        public override bool Equals(object obj)
        {
            if (obj is Version v) return Equals(v);
            return false;
        }

        public override int GetHashCode()
        {
            return (Major, Minor, Revision, Build).GetHashCode();
        }

        public override string ToString()
        {
            if (Build != null)
            {
                return $"{Major}.{Minor}.{Revision}.{Build}";
            }
            else
            {
                return $"{Major}.{Minor}.{Revision}";
            }
        }

        public static Version Parse(string version)
        {
            var sp = version.Split('.');
            var v = new Version();

            if (sp.Length >= 1)
            {
                v.Major = int.Parse(sp[0]);
            }

            if (sp.Length >= 2)
            {
                v.Minor = int.Parse(sp[1]);
            }

            if (sp.Length >= 3)
            {
                v.Revision = int.Parse(sp[2]);
            }

            if (sp.Length >= 4)
            {
                v.Build = int.Parse(sp[3]);
            }

            return v;
        }

        public Version()
        { }

        public Version(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public Version(int major, int minor, int revision, int build)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
            Build = build;
        }
    }

    public class Project
    {
        [JsonProperty("TargetFramework")]
        public string TargetFramework { get; set; }

        [JsonProperty("ImplicitUsings")]
        public string ImplicitUsings { get; set; }

        [JsonProperty("Nullable")]
        public string Nullable { get; set; }

        [JsonProperty("AssemblyVersion")]
        public Version AssemblyVersion { get; set; }

        [JsonProperty("FileVersion")]
        public Version FileVersion { get; set; }

        [JsonProperty("Authors")]
        public string Authors { get; set; }

        [JsonProperty("PreviousVersion")]
        public Version PreviousVersion { get; set; }

        [JsonProperty("Version")]
        public Version Version { get; set; }

        [JsonProperty("Product")]
        public string Product { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Copyright")]
        public string Copyright { get; set; }

        [JsonProperty("RepositoryUrl")]
        public string RepositoryUrl { get; set; }

        [JsonProperty("RepositoryType")]
        public string RepositoryType { get; set; }

        [JsonProperty("NeutralLanguage")]
        public string NeutralLanguage { get; set; }

        [JsonProperty("SignAssembly")]
        public string SignAssembly { get; set; }

        [JsonProperty("GeneratePackageOnBuild")]
        public string GeneratePackageOnBuild { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("PackageLicenseExpression")]
        public string PackageLicenseExpression { get; set; }

        [JsonProperty("PackageRequireLicenseAcceptance")]
        public string PackageRequireLicenseAcceptance { get; set; }
    }
}