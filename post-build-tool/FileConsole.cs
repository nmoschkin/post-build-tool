using System.Text;

namespace PostBuildTool
{
    public class FileConsole : IDisposable
    {
        private FileStream fs = null;
        private bool disposedValue;

        public bool Silent { get; }

        public FileConsole(bool silent, string filename)
        {
            Silent = silent;
            this.fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public FileConsole(bool silent)
        {
            Silent = silent;
        }

        public FileConsole() : this(false)
        {
        }

        public void Write(object obj = null)
        {
            if (!Silent) Console.Write(obj);
            fs?.Write(Encoding.UTF8.GetBytes(obj.ToString()));
        }

        public void WriteLine(object obj = null)
        {
            if (!Silent) Console.WriteLine(obj);
            fs?.Write(Encoding.UTF8.GetBytes(obj.ToString() + "\r\n"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fs?.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FileConsole()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}