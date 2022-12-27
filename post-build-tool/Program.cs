using Newtonsoft.Json;

using PostBuildTool.Contracts;
using PostBuildTool.Projects;
using PostBuildTool.Versioning;

using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PostBuildTool
{
    internal static class Program
    {
        private const string EnvConfig = "%POST_BUILD_TOOL_CONFIG%";

        private static void ProcessProject(string file)
        {
            FConsole.WriteLine($"Processing file '{Path.GetFileName(file)}' ...");

            var txt = File.ReadAllText(file);

            var doc = new XmlDocument();

            doc.LoadXml(txt);

            var proj = Project.FromXml(doc, file);
            bool nug = false;

            if (Options.UseNuGetPackages)
            {
                var folder = Path.GetDirectoryName(file);
                string nugfile = null;
                string projname = proj?.Title ?? Path.GetFileName(folder);

                FConsole.WriteLine($"Scanning for NuGet packages in '{folder}' ...");

                var nugets = FindAllNuGets(folder + "\\bin");
                bool failget = true;

                if (nugets != null && nugets.Count > 0)
                {
                    FConsole.WriteLine($"{nugets.Count} packages found. Determining most recent file...");

                    if (!string.IsNullOrEmpty(Options.PackageName))
                    {
                        projname = Options.PackageName;
                    }

                    var k = new List<DateTime>(nugets.Keys);

                    k.Sort((x, y) => -1 * x.CompareTo(y));
                    projname = projname.ToLower();
                    foreach (var key in k)
                    {
                        var tp = nugets[key].ToLower();

                        if (Path.GetFileName(tp).ToLower().Contains(projname))
                        {
                            if ((Constraint != PackageConstraint.Debug && (tp.Contains("release\\") || tp.Contains("release/"))) ||
                                (Constraint != PackageConstraint.Release && (tp.Contains("debug\\") || tp.Contains("debug/"))))
                            {
                                failget = false;
                                nugfile = tp;

                                break;
                            }
                        }
                    }
                }

                if (!failget && !string.IsNullOrEmpty(nugfile))
                {
                    var fn = Path.GetFileNameWithoutExtension(nugfile).ToLower().Replace(projname.ToLower() + ".", "");

                    if (BuildVersion.TryParse(fn, out var vers))
                    {
                        FConsole.WriteLine($"Taking version from NuGet package file dated {File.GetLastWriteTime(nugfile)}");
                        nug = true;
                        proj.Version = vers;
                    }
                    else
                    {
                        failget = true;
                    }
                }

                if (failget || string.IsNullOrEmpty(nugfile))
                {
                    if (Behavior == NuGetBehavior.QuitError)
                    {
                        FConsole.WriteLine("No NuGet packages found. Quitting with Error Code 2.");
                        FConsole.Flush();
                        Environment.Exit(2);
                    }
                    else if (Behavior == NuGetBehavior.QuitNoError)
                    {
                        FConsole.WriteLine("No NuGet packages found. Quitting with Error Code 0.");
                        FConsole.Flush();
                        Environment.Exit(2);
                    }
                    else
                    {
                        FConsole.WriteLine("No NuGet packages found. Ignoring, and continuing.");
                    }
                }
            }

            if (nug)
            {
                FConsole.WriteLine($"Current Version (NuGet Package): {proj.Version}");
            }
            else
            {
                FConsole.WriteLine($"Current Version: {proj.Version}");
            }

            bool nowrite = false;

            if (Options.ReportOnly)
            {
                FConsole.WriteLine("Report-Only Mode");
                nowrite = true;

                if (Versionifier.WriteMode == WriteMode.Versionifier)
                {
                    if (Versionifier is IAdjustableVersionifier adjs)
                    {
                        adjs.WriteMode = WriteMode.Application;
                    }
                    else
                    {
                        FConsole.WriteLine("Cannot calculate new version in report-only mode with an immutable IVersionifier that writes its own changes to the disk.");
                        FConsole.Flush();
                        FConsole.Flush();
                        Environment.Exit(1);
                    }
                }
            }

            FConsole.WriteLine("Versionifying ...");
            Versionifier.Versionify(proj);

            FConsole.WriteLine($"New Version: {proj.Version}");

            if (!nowrite && Versionifier.WriteMode == WriteMode.Application)
            {
                proj.ToXml(doc);
                File.WriteAllText(file, PrettyXml(doc.OuterXml));
                FConsole.WriteLine("Changes to project file saved.");
            }
            else if (nowrite)
            {
                FConsole.WriteLine("No changes were made in report-only mode.");
            }

            FConsole.Flush();
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

        private static Dictionary<DateTime, string> FindAllNuGets(string folder, Dictionary<DateTime, string> current = null)
        {
            current = current ?? new Dictionary<DateTime, string>();

            var abspath = Path.GetFullPath(folder);

            var files = Directory.GetFiles(abspath, "*.nupkg");
            var dirs = Directory.GetDirectories(abspath);

            foreach (var file in files)
            {
                current.Add(File.GetLastWriteTime(file), file);
            }

            foreach (var dir in dirs)
            {
                FindAllNuGets(dir, current);
            }

            return current;
        }

        [JsonProperty("recurse")]
        public static bool Recurse { get; private set; }

        [JsonProperty("silent")]
        public static bool Silent { get; private set; }

        [JsonProperty("nugetFailBehavior")]
        public static NuGetBehavior Behavior { get; set; }

        [JsonProperty("nugetConstraint")]
        public static PackageConstraint Constraint { get; set; } = PackageConstraint.Release;

        public static IVersionifier Versionifier { get; set; }

        public static FileConsole FConsole { get; private set; }
        public static Dictionary<string, CommandSwitch> RunArgs { get; private set; }

        public static ProgramOptions Options { get; private set; }

        public static void Main(string[] args)
        {
            CommandSwitch.Commands = MakeSwitches();
            bool optloaded = false;
            var folder = Path.GetFullPath(".");
            string conffile = null;

            var parsed = CommandSwitch.ParseArgs(args);

            RunArgs = parsed;

            CommandSwitch cfg = null;

            if (parsed.Count == 0 || (parsed.Count == 1 && parsed.TryGetValue("/opt", out cfg)))
            {
                conffile = Environment.ExpandEnvironmentVariables(EnvConfig);
                if (conffile == EnvConfig) conffile = null;

                conffile = conffile ?? "config.json";

                if (cfg != null && !string.IsNullOrEmpty(cfg.ArgumentValue)) conffile = cfg.ArgumentValue;
                try
                {
                    Options = JsonConvert.DeserializeObject<ProgramOptions>(File.ReadAllText(conffile));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading options file '{conffile}'!");
                    Console.WriteLine(ex.Message);
                    CommandSwitch.PrintHelp(false);
                    Environment.Exit(4);
                }
                optloaded = true;
            }
            else
            {
                Options = ProgramOptions.ParseFromArgs(parsed);
            }

            if (Options.DisplayHelp)
            {
                // that's that
                CommandSwitch.PrintHelp();
                return;
            }

            // check if we're importing a lib
            if (!string.IsNullOrEmpty(Options.Library))
            {
                Versionifier = VLibLoader.GetInstance(Options.Library, Options.ClassName);
            }
            else
            {
                Versionifier vf;
                Versionifier = vf = new Versionifier(Options.Mode);

                if (!string.IsNullOrEmpty(Options.Version))
                {
                    vf.OverrideVersion = BuildVersion.Parse(Options.Version);
                }

                if (!string.IsNullOrEmpty(Options.OldVersion))
                {
                    vf.OverridePrevious = BuildVersion.Parse(Options.OldVersion);
                }
            }

            Recurse = Options.Recursive;
            Silent = Options.Quiet;

            if (!string.IsNullOrEmpty(Options.LogFile))
            {
                FConsole = new FileConsole(Silent, Options.LogFile);
            }
            else
            {
                FConsole = new FileConsole(Silent);
            }

            Constraint = Options.PackageConstraint;
            Behavior = Options.NuGetFailBehavior;

            FConsole.WriteLine("post-build-tool v1.0");
            FConsole.WriteLine();
            FConsole.WriteLine("Copyright (C) 2023 Nathaniel Moschkin");
            FConsole.WriteLine("All Rights Reserved");
            FConsole.WriteLine();

            if (optloaded && conffile != null)
            {
                FConsole.WriteLine($"Config file '{Path.GetFileName(conffile)}' loaded.");
            }

            if (!string.IsNullOrEmpty(Options.Project))
            {
                ProcessProject(Options.Project);
            }
            else
            {
                if (!string.IsNullOrEmpty(Options.Directory))
                {
                    folder = Options.Directory;
                }

                if (!Directory.Exists(folder))
                {
                    FConsole.WriteLine("Cannot find directory '" + folder + "', aborting!");
                    FConsole.Flush();

                    Environment.Exit(3);
                }

                ProcessFolder(folder);
            }

            FConsole.Flush();
        }

        public static List<CommandSwitch> MakeSwitches()
        {
            var cmds = new List<CommandSwitch>
            {
                new CommandSwitch("/opt|/options", "Use JSON config file. Highly recommended.", "file"),
                new CommandSwitch("/d|/dir", "Open and update all .csproj files in the specified directory.", "dir"),
                new CommandSwitch("/r|/recursive", "Recursively scan the directory and sub-directories for .csproj files."),
                new CommandSwitch("/p|/project", "Open and update the specified project.", "project"),
                new CommandSwitch("/v|/version", "Force the specified version number (as opposed to auto-incrementing.)", "version"),
                new CommandSwitch("/ov|/oldversion", "Specify the PreviousVersion variable (as opposed to being automatically calculated.)", "version"),
                new CommandSwitch("/q|/quiet", "Suppress all output."),
                new CommandSwitch("/l|/log", "Log output to the specified file.", "file"),
                new CommandSwitch("/lib", "Use an IVersionifier instance from the specified DLL.", "dll"),
                new CommandSwitch("/cn", "Optional class name to be used with /lib.", "name"),
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
                }),
                new CommandSwitch("/ngo", "Scan NuGet packages to determine most recent last version."),
                new CommandSwitch("/pn", "Specify the NuGet package name (used with /ngo.)", "name"),
                new CommandSwitch(new[] { "/ngb" }, "Set the behavior if no NuGet packages are found.", true, "behavior", options: new Dictionary<string, string>()
                {
                    { "QuitError" , "Quit with non-zero exit code (default). This will cause a build failure if run in post-build actions." },
                    { "QuitNoError" , "Quit with zero exit code." },
                    { "ContinueIgnore" , "Ignore the NuGet package request and continue without it." },
                }),
                new CommandSwitch(new[] { "/ngc" }, "Set the NuGet package constraint.", true, "constraint", options: new Dictionary<string, string>()
                {
                    { "Release" , "Must be release packages (default)." },
                    { "Debug" , "Must be debug packages." },
                    { "DontCare" , "No preference." },
                }),
                new CommandSwitch("/nw|/report", "Report only. Do not write any changes."),
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