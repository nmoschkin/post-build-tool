using PostBuildTool.Contracts;
using PostBuildTool.Versioning;

namespace ExternalVersionify
{
    /// <summary>
    /// This is a sample class to demonstrate how to write a custom versionifier
    /// </summary>
    public class CustomVersionify : IVersionifier
    {
        public WriteMode WriteMode => WriteMode.Application;

        public void Versionify(IProject project)
        {
            var version = project.Version;

            project.PreviousVersion = (IBuildVersion)version.Clone();

            version.Minor++;

            project.AssemblyVersion = (IBuildVersion)version.Clone();
            project.FileVersion = (IBuildVersion)version.Clone();
        }
    }
}