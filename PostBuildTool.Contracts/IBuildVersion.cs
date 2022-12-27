namespace PostBuildTool.Contracts
{
    /// <summary>
    /// Provides a contract for a parseable class for encoding and decoding version strings
    /// </summary>
    public interface IBuildVersion : IEquatable<IBuildVersion>, IComparable<IBuildVersion>, ICloneable
    {
        /// <summary>
        /// Major verison
        /// </summary>
        int Major { get; set; }

        /// <summary>
        /// Minor version
        /// </summary>
        int Minor { get; set; }

        /// <summary>
        /// Revision
        /// </summary>
        int Revision { get; set; }

        /// <summary>
        /// Build
        /// </summary>
        int? Build { get; set; }

        /// <summary>
        /// Prints the version number
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// Try to parse a version number from a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool TryParse<T>(string s, out T v) where T : class, IBuildVersion, new()
        {
            try
            {
                v = Parse<T>(s);
                return true;
            }
            catch
            {
                v = null;
                return false;
            }
        }

        /// <summary>
        /// Parse a version number from a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Parse<T>(string version) where T : class, IBuildVersion, new()
        {
            if (string.IsNullOrEmpty(version)) throw new ArgumentNullException(nameof(version));

            var sp = version.Split('.');
            var v = new T();

            if (sp.Length >= 1)
            {
                v.Major = int.Parse(sp[0]);
            }

            if (sp.Length >= 2)
            {
                v.Minor = int.Parse(sp[1]);
            }

            if (sp.Length >= 3)
            {
                v.Revision = int.Parse(sp[2]);
            }

            if (sp.Length >= 4)
            {
                v.Build = int.Parse(sp[3]);
            }

            return v;
        }
    }
}