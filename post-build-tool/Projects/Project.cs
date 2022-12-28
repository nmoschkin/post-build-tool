using Newtonsoft.Json;

using PostBuildTool.Contracts;
using PostBuildTool.Versioning;

using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace PostBuildTool.Projects
{
    public class Project : IProject
    {
        public void ToXml(XmlDocument doc)
        {
            var elem = doc.GetElementsByTagName("PropertyGroup")[0];

            var json = JsonConvert.SerializeObject(this);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            var seen = new List<string>();

            foreach (XmlNode node in elem.ChildNodes)
            {
                if (dict.ContainsKey(node.Name))
                {
                    node.InnerText = dict[node.Name];
                    seen.Add(node.Name);
                }
            }

            foreach (var kv in dict)
            {
                if (kv.Value != null && !seen.Contains(kv.Key))
                {
                    elem.InnerXml += $"<{kv.Key}>{kv.Value}</{kv.Key}>";
                }
            }
        }

        public static Project FromXml(XmlDocument doc, string filename)
        {
            var elem = doc.GetElementsByTagName("PropertyGroup")[0];
            var dict = new Dictionary<string, string>();

            foreach (XmlNode node in elem.ChildNodes)
            {
                dict.Add(node.Name, node.InnerText);
            }

            var json = JsonConvert.SerializeObject(dict);

            var proj = new Project();

            JsonConvert.PopulateObject(json, proj);

            proj.XmlDoc = doc;
            proj.Filename = filename;

            return proj;
        }

        [JsonIgnore] public virtual string Filename { get; protected set; }

        [JsonIgnore]
        public virtual XmlDocument XmlDoc { get; protected internal set; }

        [JsonProperty("TargetFramework")]
        public virtual string TargetFramework { get; set; }

        [JsonProperty("ImplicitUsings")]
        public virtual string ImplicitUsings { get; set; }

        [JsonProperty("Nullable")]
        public virtual string Nullable { get; set; }

        [JsonConverter(typeof(VersionConverter<BuildVersion>))]
        [JsonProperty("AssemblyVersion")]
        public virtual IBuildVersion AssemblyVersion { get; set; } = new BuildVersion();

        [JsonConverter(typeof(VersionConverter<BuildVersion>))]
        [JsonProperty("FileVersion")]
        public virtual IBuildVersion FileVersion { get; set; } = new BuildVersion();

        [JsonProperty("Authors")]
        public virtual string Authors { get; set; }

        [JsonConverter(typeof(VersionConverter<BuildVersion>))]
        [JsonProperty("PreviousVersion")]
        public virtual IBuildVersion PreviousVersion { get; set; } = new BuildVersion();

        [JsonConverter(typeof(VersionConverter<BuildVersion>))]
        [JsonProperty("Version")]
        public virtual IBuildVersion Version { get; set; } = new BuildVersion();

        [JsonProperty("Product")]
        public virtual string Product { get; set; }

        [JsonProperty("Description")]
        public virtual string Description { get; set; }

        [JsonProperty("Copyright")]
        public virtual string Copyright { get; set; }

        [JsonProperty("RepositoryUrl")]
        public virtual string RepositoryUrl { get; set; }

        [JsonProperty("RepositoryType")]
        public virtual string RepositoryType { get; set; }

        [JsonProperty("NeutralLanguage")]
        public virtual string NeutralLanguage { get; set; }

        [JsonProperty("SignAssembly")]
        public virtual string SignAssembly { get; set; }

        [JsonProperty("GeneratePackageOnBuild")]
        public virtual string GeneratePackageOnBuild { get; set; }

        [JsonProperty("Title")]
        public virtual string Title { get; set; }

        [JsonProperty("PackageLicenseExpression")]
        public virtual string PackageLicenseExpression { get; set; }

        [JsonProperty("PackageRequireLicenseAcceptance")]
        public virtual string PackageRequireLicenseAcceptance { get; set; }
    }
}