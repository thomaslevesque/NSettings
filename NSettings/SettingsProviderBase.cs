using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NSettings
{
    public abstract class SettingsProviderBase : ISettingsProvider
    {
        private readonly IStreamStorageProvider _storageProvider;
        private readonly ConcurrentDictionary<Type, object> _sections = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, string> _sectionNames = new ConcurrentDictionary<Type, string>();

        protected SettingsProviderBase(IStreamStorageProvider storageProvider)
        {
            if (storageProvider == null) throw new ArgumentNullException(nameof(storageProvider));
            _storageProvider = storageProvider;
        }

        public TSettings GetSettings<TSettings>() where TSettings : class, new()
        {
            string sectionName = GetSectionName(typeof (TSettings));
            var section = _sections.GetOrAdd(typeof (TSettings), type => CreateSection<TSettings>(sectionName));
            return (TSettings) section;
        }

        private string GetSectionName(Type type)
        {
            return _sectionNames.GetOrAdd(type, t =>
            {
                var attr = t.GetTypeInfo().GetCustomAttribute<SettingsSectionAttribute>();
                return attr != null ? attr.Name : t.Name;
            });
        }

        protected TSettings CreateSection<TSettings>(string sectionName) where TSettings : class, new()
        {
            var section = new TSettings();
            LoadSection(sectionName, section);
            return section;
        }
        protected abstract void LoadSection(string sectionName, object section);
        protected abstract void SaveSection(string sectionName, object section);

        private void LoadSections()
        {
            foreach (var section in _sections.Values)
            {
                LoadSection(GetSectionName(section.GetType()), section);
            }
        }

        private void SaveSections()
        {
            foreach (var section in _sections.Values)
            {
                SaveSection(GetSectionName(section.GetType()), section);
            }
        }

        public void Load()
        {
            using (var stream = _storageProvider.OpenRead())
            {
                if (stream == null)
                {
                    LoadDefaults();
                }
                else
                {
                    LoadCore(stream);
                }
            }
            LoadSections();
        }

        public async Task LoadAsync()
        {
            using (var stream = await _storageProvider.OpenReadAsync())
            {
                if (stream == null)
                {
                    LoadDefaults();
                }
                else
                {
                    await LoadCoreAsync(stream);
                }
            }
            LoadSections();
        }

        public void Save()
        {
            SaveSections();
            using (var stream = _storageProvider.OpenWrite())
            {
                SaveCore(stream);
            }
        }

        public async Task SaveAsync()
        {
            SaveSections();
            using (var stream = await _storageProvider.OpenWriteAsync())
            {
                await SaveCoreAsync(stream); 
            }
        }

        protected abstract void LoadDefaults();
        protected abstract void LoadCore(Stream stream);
        protected abstract Task LoadCoreAsync(Stream stream);
        protected abstract void SaveCore(Stream stream);
        protected abstract Task SaveCoreAsync(Stream stream);
    }
}
