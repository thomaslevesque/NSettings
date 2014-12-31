using System;
using System.IO;
using System.Threading.Tasks;

namespace NSettings.Desktop
{
    public class FileStreamStorageProvider : IStreamStorageProvider
    {
        private readonly string _path;

        public FileStreamStorageProvider(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            _path = path;
        }

        public Stream OpenRead()
        {
            try
            {
                return File.OpenRead(_path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public Stream OpenWrite()
        {
            return File.Open(_path, FileMode.Create);
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
