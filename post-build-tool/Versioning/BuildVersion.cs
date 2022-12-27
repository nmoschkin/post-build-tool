using Newtonsoft.Json;

using PostBuildTool.Contracts;

namespace PostBuildTool.Versioning
{
    [JsonConverter(typeof(VersionConverter<BuildVersion>))]
    public class BuildVersion : IBuildVersion
    {
        public virtual int Major { get; set; } = 1;

        public virtual int Minor { get; set; } = 0;

        public virtual int Revision { get; set; } = 0;

        public virtual int? Build { get; set; } = null;

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

        public bool Equals(IBuildVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Revision == other.Revision && Build == other.Build;
        }

        public override bool Equals(object obj)
        {
            if (obj is BuildVersion v) return Equals(v);
            return false;
        }

        public int CompareTo(IBuildVersion other)
        {
            var r = Major.CompareTo(other.Major);

            if (r == 0)
            {
                r = Minor.CompareTo(other.Minor);
                if (r == 0)
                {
                    r = Revision.CompareTo(other.Revision);
                    if (r == 0)
                    {
                        if (Build is int x && other.Build is int y)
                        {
                            r = x.CompareTo(y);
                        }
                        else if (Build == null)
                        {
                            r = -1;
                        }
                        else if (other.Build == null)
                        {
                            r = 1;
                        }
                    }
                }
            }

            return r;
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
            return IBuildVersion.TryParse(s, out v);
        }

        public static BuildVersion Parse(string version)
        {
            return IBuildVersion.Parse<BuildVersion>(version);
        }

        public BuildVersion()
        { }

        public BuildVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
            Build = null;
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