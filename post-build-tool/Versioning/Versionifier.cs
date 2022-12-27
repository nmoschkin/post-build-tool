using PostBuildTool.Projects;

using System;
using System.Linq;
using System.Text;

namespace PostBuildTool.Versioning
{
    public class Versionifier : IVersionifier
    {
        /// <summary>
        /// The type of built-in versionification to run.
        /// </summary>
        /// <remarks>
        /// This is a flag
        /// </remarks>
        public VersionifyMode Mode { get; set; } = VersionifyMode.BumpBuild;

        /// <summary>
        /// The default start integer for the <see cref="BuildVersion.Build"/> value if it is uninitialized.
        /// </summary>
        public int BuildStart { get; set; } = 1000;

        public Versionifier()
        {
        }

        /// <summary>
        /// Create a new versionifier.
        /// </summary>
        /// <param name="mode">The type of built-in versionification to run.</param>
        /// <param name="buildStart">The default start integer for the <see cref="BuildVersion.Build"/> value if it is uninitialized.</param>
        public Versionifier(VersionifyMode mode, int buildStart = 1000)
        {
            Mode = mode;
            BuildStart = buildStart;
        }

        public void Versionify(Project project)
        {
            var prev = project.PreviousVersion = project.Version.Clone();
            var vers = project.Version ?? new BuildVersion() { Build = BuildStart };
            int h, m;

            var mode = Mode & VersionifyMode.RegularMask;
            var noinc = (Mode & VersionifyMode.DoNotIncrement) != 0;

            switch (mode)
            {
                case VersionifyMode.BumpBuild:
                    var build = vers.Build ?? BuildStart;
                    build++;
                    vers.Build = build;
                    break;

                case VersionifyMode.BumpRevsion:
                    vers.Revision++;
                    break;

                case VersionifyMode.BumpMinor:
                    vers.Minor++;
                    break;

                case VersionifyMode.BumpMajor:
                    vers.Major++;
                    break;

                case VersionifyMode.BuildHour:
                    h = BuildVersion.GetHourOfYear();

                    if (!noinc && prev.Build is int ipb)
                    {
                        while (h <= ipb) h++;
                    }

                    vers.Build = h;
                    break;

                case VersionifyMode.RevisionHour:
                    h = BuildVersion.GetHourOfYear();

                    if (!noinc && prev.Revision is int ipr)
                    {
                        while (h <= ipr) h++;
                    }

                    vers.Revision = h;
                    break;

                case VersionifyMode.BuildMinute:

                    h = BuildVersion.GetHourOfYear();
                    m = BuildVersion.GetMinuteOfDay();

                    if (prev.Revision == h)
                    {
                        if (!noinc && prev.Build is int ibr)
                        {
                            while (m <= ibr) m++;
                        }
                    }

                    vers.Revision = h;
                    vers.Build = m;

                    break;
            }

            project.FileVersion = vers.Clone();
            project.AssemblyVersion = vers.Clone();
        }
    }
}