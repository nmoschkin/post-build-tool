using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace PostBuildTool.Contracts
{
    /// <summary>
    /// Provides some core components of a .NET Sdk-style Project
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// The filename of the current project file
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// The loaded XML document
        /// </summary>
        XmlDocument XmlDoc { get; }

        string TargetFramework { get; set; }

        string ImplicitUsings { get; set; }

        string Nullable { get; set; }

        IBuildVersion AssemblyVersion { get; set; }

        IBuildVersion FileVersion { get; set; }

        string Authors { get; set; }

        IBuildVersion PreviousVersion { get; set; }

        IBuildVersion Version { get; set; }

        string Product { get; set; }

        string Description { get; set; }

        string Copyright { get; set; }

        string RepositoryUrl { get; set; }

        string RepositoryType { get; set; }

        string NeutralLanguage { get; set; }

        string SignAssembly { get; set; }

        string GeneratePackageOnBuild { get; set; }

        string Title { get; set; }

        string PackageLicenseExpression { get; set; }

        string PackageRequireLicenseAcceptance { get; set; }
    }
}