using System;
using System.Linq;
using System.Text;

namespace PostBuildTool.Versioning
{
    public static class StringExtensions
    {
        public static BuildVersion MakeBuildVersion(this string version)
        {
            return BuildVersion.Parse(version);
        }
    }
}