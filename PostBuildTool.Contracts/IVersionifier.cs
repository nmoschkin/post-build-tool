using PostBuildTool.Versioning;

using System;
using System.Linq;
using System.Text;

namespace PostBuildTool.Contracts
{
    /// <summary>
    /// Durable contract for a class that does the logic of calculating new and old project versions.
    /// </summary>
    public interface IVersionifier
    {
        /// <summary>
        /// Indicates whether or not the <see cref="IVersionifier"/> implementation writes the resultant changes to the project file.
        /// </summary>
        WriteMode WriteMode { get; }

        /// <summary>
        /// Apply the desired version changes to the project.
        /// </summary>
        /// <param name="project"></param>
        void Versionify(IProject project);
    }

    /// <summary>
    /// A version of <see cref="IVersionifier"/> that allows the program to alter the <see cref="WriteMode"/>.
    /// </summary>
    public interface IAdjustableVersionifier : IVersionifier
    {
        /// <summary>
        /// Indicates whether or not the <see cref="IVersionifier"/> implementation writes the resultant changes to the project file.
        /// </summary>
        new public WriteMode WriteMode { get; set; }
    }
}