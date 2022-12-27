using PostBuildTool.Projects;
using PostBuildTool.Versioning;

using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PostBuildTool
{
    public static partial class Program
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

        public static bool Recurse { get; private set; }
        public static bool Silent { get; private set; }
        public static FileConsole FConsole { get; private set; }
        public static Dictionary<string, CommandSwitch> RunArgs { get; private set; }

        public static IVersionifier Versionifier { get; set; } = new Versionifier(VersionifyMode.BumpBuild);

        public static void Main(string[] args)
        {
            var folder = Path.GetFullPath(".");

            var parsed = CommandSwitch.ParseArgs(args);

            RunArgs = parsed;

            //if (parsed == null || parsed.Count == 0)
            //{
            //    CommandSwitch.PrintHelp(true);
            //    return;
            //}

            if (parsed.TryGetValue("/d", out var fldarg))
            {
                folder = fldarg.ArgumentValue;
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

            ProcessFolder(folder);
        }
    }
}