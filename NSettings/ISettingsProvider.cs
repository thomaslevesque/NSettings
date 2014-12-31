using System.Threading.Tasks;

namespace NSettings
{
    public interface ISettingsProvider
    {
        TSettings GetSettings<TSettings>() where TSettings : class, new();
        void Load();
        Task LoadAsync();
        void Save();
        Task SaveAsync();
    }
}
