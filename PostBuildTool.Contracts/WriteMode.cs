namespace PostBuildTool.Versioning
{
    /// <summary>
    /// An enum of options for indicating how the <see cref="IVersionifier"/> deals with the final result
    /// </summary>
    public enum WriteMode
    {
        /// <summary>
        /// The application will manage writing changes to the project file. The versionifier should not write changes.
        /// </summary>
        Application = 0,

        /// <summary>
        /// The versionifier will write changes to the project file. The application will not attempt to write.
        /// </summary>
        Versionifier
    }
}