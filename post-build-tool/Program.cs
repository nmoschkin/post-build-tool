using PostBuildTool.Projects;
using PostBuildTool.Versioning;

using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PostBuildTool
{
    public static class Program
    {
        private static void ProcessProject(string file)
        {
            FConsole.WriteLine($"Processing file '{Path.GetFileName(file)}' ...");

            var txt = File.ReadAllText(file);

            var doc = new XmlDocument();

            doc.LoadXml(txt);

            var proj = Project.FromXml(doc);

            FConsole.WriteLine($"Current Version: {proj.Version}");
            FConsole.WriteLine("Versionifying ...");

            Versionifier.Versionify(proj);

            FConsole.WriteLine($"New Version: {proj.Version}");
            proj.ToXml(doc);
            File.WriteAllText(file, PrettyXml(doc.OuterXml));
        }

        private static string PrettyXml(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = false;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        private static void ProcessFolder(string folder)
        {
            FConsole.WriteLine($"Scanning folder '{folder}' ...");

            var abspath = Path.GetFullPath(folder);

            var files = Directory.GetFiles(abspath, "*.csproj");
            var dirs = Directory.GetDirectories(abspath);

            foreach (var file in files)
            {
                ProcessProject(file);
            }

            if (!Recurse) return;

            foreach (var dir in dirs)
            {
                ProcessFolder(dir);
            }
        }

        private static Dictionary<DateTime, string> FindNuGets(string folder, Dictionary<DateTime, string> current = null)
        {
            current = current ?? new Dictionary<DateTime, string>();

            FConsole.WriteLine($"Scanning for NuGet packages in '{folder}' ...");

            var abspath = Path.GetFullPath(folder);

            var files = Directory.GetFiles(abspath, "*.nupkg");
            var dirs = Directory.GetDirectories(abspath);

            foreach (var file in files)
            {
                current.Add(File.GetLastWriteTime(file), file);
            }

            foreach (var dir in dirs)
            {
                FindNuGets(dir, current);
            }

            return current;
        }

        public static bool Recurse { get; private set; }
        public static bool Silent { get; private set; }
        public static FileConsole FConsole { get; private set; }
        public static Dictionary<string, CommandSwitch> RunArgs { get; private set; }

        public static Versionifier Versionifier { get; set; } = new Versionifier(VersionifyMode.BumpBuild);

        public static void Main(string[] args)
        {
            CommandSwitch.Commands = MakeSwitches();

            var folder = Path.GetFullPath(".");

            var parsed = CommandSwitch.ParseArgs(args);

            RunArgs = parsed;

            if (parsed.TryGetValue("/m", out var cmode))
            {
                if (Enum.TryParse<VersionifyMode>(cmode.ArgumentValue, out var gm))
                {
                    Versionifier.Mode = gm;
                }
            }

            if (parsed.ContainsKey("/h"))
            {
                CommandSwitch.PrintHelp();
            }

            if (parsed.TryGetValue("/v", out var version))
            {
                Versionifier.OverrideVersion = BuildVersion.Parse(version.ArgumentValue);
            }

            if (parsed.TryGetValue("/ov", out version))
            {
                Versionifier.OverridePrevious = BuildVersion.Parse(version.ArgumentValue);
            }

            Recurse = parsed.ContainsKey("/r");
            Silent = parsed.ContainsKey("/q");

            if (parsed.TryGetValue("/l", out var logarg))
            {
                FConsole = new FileConsole(Silent, logarg.ArgumentValue);
            }
            else
            {
                FConsole = new FileConsole(Silent);
            }

            FConsole.WriteLine("post-build-tool v1.0");
            FConsole.WriteLine();
            FConsole.WriteLine("Copyright (C) 2023 Nathaniel Moschkin");
            FConsole.WriteLine("All Rights Reserved");
            FConsole.WriteLine();

            if (parsed.TryGetValue("/d", out var fldarg))
            {
                folder = fldarg.ArgumentValue;
                ProcessFolder(folder);
            }
            else if (parsed.TryGetValue("/p", out var projarg))
            {
                ProcessProject(projarg.ArgumentValue);
            }
        }

        public static List<CommandSwitch> MakeSwitches()
        {
            var cmds = new List<CommandSwitch>
            {
                new CommandSwitch("/d|/dir", "Open and update all .csproj files in the specified directory.", "dir"),
                new CommandSwitch("/r|/recursive", "Recursively scan the directory and sub-directories for .csproj files."),
                new CommandSwitch("/p|/project", "Open and update the specified project.", "project"),
                new CommandSwitch("/v|/version", "Force the specified version number (as opposed to auto-incrementing.)", "version"),
                new CommandSwitch("/ngo", "Scan NuGet packages to determine most recent last version."),
                new CommandSwitch("/pn", "Specify the NuGet package name (used with /ngo.)", "name"),
                new CommandSwitch("/ov|/oldversion", "Specify the PreviousVersion variable (as opposed to being automatically calculated.)", "version"),
                new CommandSwitch("/q|/quiet", "Suppress all output."),
                new CommandSwitch("/l|/log", "Log output to the specified file.", "file"),
                new CommandSwitch("/h|/help", "Displays this help screen."),
                new CommandSwitch(new[] { "/m", "/mode" }, "Set the versioning mode.", true, "mode", options: new Dictionary<string, string>()
                {
                    { "BumpBuild" , "Bump build by 1 (default)" },
                    { "BumpRevsion" , "Bump revsion by 1" },
                    { "BumpMinor" , "Bump minor version by 1" },
                    { "BumpMajor" , "Bump major version by 1" },
                    { "BuildHour" , "Put the hour of the year in the build number" },
                    { "RevisionHour" , "Put the hour of the year in the revision number" },
                    { "BuildMinute" , "Put the hour of the year in the revision number," },
                    { "BuildMinute2" , "and put the minute of the day in the build number" }
                })
            };

            return cmds;

            /*

             { "BumpBuild" , "BumpBuild" },
        { "BumpRevsion" , "BumpRevsion" },
        { "BumpMinor" , "BumpMinor" },
        { "BumpMajor" , "BumpMajor" },
        { "BuildHour" , "BuildHour" },
        { "RevisionHour" , "RevisionHour" },
        { "BuildMinute" , "BuildMinute" },

             */
        }
    }
}