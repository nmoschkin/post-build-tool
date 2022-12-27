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
        private static void ProcessProject(string file)
        {
            FConsole.WriteLine($"Processing file '{Path.GetFileName(file)}' ...");

            var txt = File.ReadAllText(file);

            var doc = new XmlDocument();

            doc.LoadXml(txt);

            var proj = Project.FromXml(doc, file);
            bool nug = false;

            if (RunArgs.ContainsKey("/ngo"))
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

                    if (RunArgs.TryGetValue("/pn", out var pno))
                    {
                        projname = pno.ArgumentValue ?? projname;
                    }

                    var k = new List<DateTime>(nugets.Keys);

                    k.Sort((x, y) => -1 * x.CompareTo(y));
                    projname = projname.ToLower();
                    foreach (var key in k)
                    {
                        var tp = nugets[key].ToLower();

                        if ((Path.GetFileName(tp).ToLower().Contains(projname)) && (tp.Contains("release\\") || tp.Contains("release/")))
                        {
                            failget = false;
                            nugfile = tp;

                            break;
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
                    if (RunArgs.TryGetValue("/ngb", out var ngb))
                    {
                        if (Enum.TryParse<NuGetBehavior>(ngb.ArgumentValue, out var result))
                        {
                            Behavior = result;
                        }
                    }

                    if (Behavior == NuGetBehavior.QuitError)
                    {
                        FConsole.WriteLine("No NuGet packages found. Quitting with Error Code 2.");
                        Environment.Exit(2);
                    }
                    else if (Behavior == NuGetBehavior.QuitNoError)
                    {
                        FConsole.WriteLine("No NuGet packages found. Quitting with Error Code 0.");
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

            FConsole.WriteLine("Versionifying ...");
            Versionifier.Versionify(proj);
            FConsole.WriteLine($"New Version: {proj.Version}");

            if (Versionifier.WriteMode == WriteMode.Application)
            {
                proj.ToXml(doc);
                File.WriteAllText(file, PrettyXml(doc.OuterXml));
            }
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

        public static string NuGetFile { get; private set; }
        public static bool Recurse { get; private set; }
        public static bool Silent { get; private set; }
        public static FileConsole FConsole { get; private set; }
        public static Dictionary<string, CommandSwitch> RunArgs { get; private set; }

        public static IVersionifier Versionifier { get; set; }

        public static NuGetBehavior Behavior { get; set; }

        public static void Main(string[] args)
        {
            CommandSwitch.Commands = MakeSwitches();

            var folder = Path.GetFullPath(".");

            var parsed = CommandSwitch.ParseArgs(args);

            RunArgs = parsed;

            if (parsed.ContainsKey("/h"))
            {
                // that's that
                CommandSwitch.PrintHelp();
                return;
            }

            // check if we're importing a lib
            if (parsed.TryGetValue("/lib", out var libfile))
            {
                parsed.TryGetValue("/cn", out var classname);
                Versionifier = VLibLoader.GetInstance(libfile.ArgumentValue, classname?.ArgumentValue);
            }
            else
            {
                Versionifier vf;
                Versionifier = vf = new Versionifier(VersionifyMode.BumpBuild);

                if (parsed.TryGetValue("/m", out var cmode))
                {
                    if (Enum.TryParse<VersionifyMode>(cmode.ArgumentValue, out var gm))
                    {
                        vf.Mode = gm;
                    }
                }

                if (parsed.TryGetValue("/v", out var version))
                {
                    vf.OverrideVersion = BuildVersion.Parse(version.ArgumentValue);
                }

                if (parsed.TryGetValue("/ov", out version))
                {
                    vf.OverridePrevious = BuildVersion.Parse(version.ArgumentValue);
                }
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
                    { "QuitError" , "Quit with non-zero exit code (default). This will cause a compiler failure in post-build actions." },
                    { "QuitNoError" , "Quit with zero exit code." },
                    { "ContinueIgnore" , "Ignore the NuGet package request and continue without it." },
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