using System.IO;
using System.Threading.Tasks;

namespace NSettings
{
    public interface IStreamStorageProvider
    {
        Stream OpenRead();
        Stream OpenWrite();

        Task<Stream> OpenReadAsync();
        Task<Stream> OpenWriteAsync();
    }
}