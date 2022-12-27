using System;
using System.Linq;
using System.Text;

using PostBuildTool.Projects;

namespace PostBuildTool.Versioning
{
    public interface IVersionifier
    {
        void Versionify(Project project);
    }
}