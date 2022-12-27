using System.Text;

namespace PostBuildTool
{
    internal class FileConsole : IDisposable
    {
        private FileStream fs = null;
        private bool disposedValue;

        public bool Silent { get; }

        public FileConsole(bool silent, string filename)
        {
            Silent = silent;
            this.fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);

            WriteLine($"\r\n[{DateTime.Now:G}]");
        }

        public FileConsole(bool silent)
        {
            Silent = silent;
            WriteLine($"\r\n[{DateTime.Now:G}]");
        }

        public FileConsole() : this(false)
        {
        }

        public void Write(object obj = null)
        {
            if (!Silent) Console.Write(obj);
            fs?.Write(Encoding.UTF8.GetBytes($"{obj}"));
        }

        public void WriteLine(object obj = null)
        {
            if (!Silent) Console.WriteLine(obj);
            fs?.Write(Encoding.UTF8.GetBytes($"{obj}\r\n"));
        }

        public void Flush()
        {
            fs?.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                fs?.Flush();
                fs?.Close();

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~FileConsole()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}