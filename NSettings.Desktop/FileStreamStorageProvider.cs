using System;
using System.IO;
using System.Threading.Tasks;

namespace NSettings.Desktop
{
    public class FileStreamStorageProvider : IStreamStorageProvider
    {
        public string Path { get; }

        public FileStreamStorageProvider(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            Path = path;
        }

        public Stream OpenRead()
        {
            try
            {
                return File.OpenRead(Path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public Stream OpenWrite()
        {
            return File.Open(Path, FileMode.Create);
        }

        public Task<Stream> OpenReadAsync()
        {
            return Task.FromResult(OpenRead());
        }

        public Task<Stream> OpenWriteAsync()
        {
            return Task.FromResult(OpenWrite());
        }
    }
}
