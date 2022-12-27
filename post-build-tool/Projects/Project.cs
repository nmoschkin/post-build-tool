using Newtonsoft.Json;

using PostBuildTool.Versioning;

using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace PostBuildTool.Projects
{
    public class Project
    {
        public void ToXml(XmlDocument doc)
        {
            var elem = doc.GetElementsByTagName("PropertyGroup")[0];

            var json = JsonConvert.SerializeObject(this);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (XmlNode node in elem.ChildNodes)
            {
                if (dict.ContainsKey(node.Name))
                {
                    node.InnerText = dict[node.Name];
                }
            }
        }

        public static Project FromXml(XmlDocument doc)
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
            return proj;
        }

        [JsonProperty("TargetFramework")]
        public string TargetFramework { get; set; }

        [JsonProperty("ImplicitUsings")]
        public string ImplicitUsings { get; set; }

        [JsonProperty("Nullable")]
        public string Nullable { get; set; }

        [JsonProperty("AssemblyVersion")]
        public BuildVersion AssemblyVersion { get; set; }

        [JsonProperty("FileVersion")]
        public BuildVersion FileVersion { get; set; }

        [JsonProperty("Authors")]
        public string Authors { get; set; }

        [JsonProperty("PreviousVersion")]
        public BuildVersion PreviousVersion { get; set; }

        [JsonProperty("Version")]
        public BuildVersion Version { get; set; }

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