using PostBuildTool.Contracts;

using System.Reflection;

namespace PostBuildTool
{
    internal static class VLibLoader
    {
        /// <summary>
        /// Gets an instance of <see cref="IVersionifier"/> from the specified assembly.
        /// </summary>
        /// <param name="module">The DLL to load.</param>
        /// <param name="classname">Optional classname. If omitted, the first <see cref="IVersionifier"/> implementation is instantiated.</param>
        /// <returns></returns>
        public static IVersionifier GetInstance(string module, string classname = null)
        {
            var asm = Assembly.LoadFrom(module);
            Type t;

            if (string.IsNullOrEmpty(classname))
            {
                t = asm.GetExportedTypes().FirstOrDefault(x => x.GetInterface(nameof(IVersionifier)) == typeof(IVersionifier));
            }
            else
            {
                t = asm.GetType(classname);
                if (t != null)
                {
                    var iface = t.GetInterface(nameof(IVersionifier));
                    if (iface != typeof(IVersionifier)) return null;
                }
            }

            return t != null ? (IVersionifier)Activator.CreateInstance(t) : null;
        }
    }
}