using Newtonsoft.Json;

namespace PostBuildTool.Versioning
{
    [JsonConverter(typeof(VersionConverter))]
    public class BuildVersion : IEquatable<BuildVersion>, ICloneable
    {
        public int Major { get; set; } = 1;

        public int Minor { get; set; } = 0;

        public int Revision { get; set; } = 0;

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

        public bool Equals(BuildVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Revision == other.Revision && Build == other.Build;
        }

        public override bool Equals(object obj)
        {
            if (obj is BuildVersion v) return Equals(v);
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

        public static bool TryParse(string s, out BuildVersion v)
        {
            try
            {
                v = Parse(s);
                return true;
            }
            catch
            {
                v = null;
                return false;
            }
        }

        public static BuildVersion Parse(string version)
        {
            if (string.IsNullOrEmpty(version)) throw new ArgumentNullException(nameof(version));

            var sp = version.Split('.');
            var v = new BuildVersion();

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

        public BuildVersion()
        { }

        public BuildVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public BuildVersion(int major, int minor, int revision, int build)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
            Build = build;
        }

        object ICloneable.Clone() => Clone();

        public BuildVersion Clone() => (BuildVersion)MemberwiseClone();

        public static implicit operator string(BuildVersion v)
        { return v.ToString(); }

        public static explicit operator BuildVersion(string s)
        {
            if (!TryParse(s, out var v)) throw new InvalidCastException("Cannot cast string to Version");
            return v;
        }
    }
}