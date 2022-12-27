using PostBuildTool.Versioning;

using System;
using System.Linq;
using System.Text;

namespace PostBuildTool.Contracts
{
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
}