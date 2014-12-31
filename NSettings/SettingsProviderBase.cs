using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace NSettings
{
    public abstract class SettingsProviderBase : ISettingsProvider
    {
        private readonly ConcurrentDictionary<Type, object> _sections = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, string> _sectionNames = new ConcurrentDictionary<Type, string>();

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

        protected abstract TSettings CreateSection<TSettings>(string sectionName) where TSettings : class, new();
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
            LoadCore();
            LoadSections();
        }

        public async Task LoadAsync()
        {
            await LoadCoreAsync();
            LoadSections();
        }

        public void Save()
        {
            SaveSections();
            SaveCore();
        }

        public Task SaveAsync()
        {
            SaveSections();
            return SaveCoreAsync();
        }

        public abstract void LoadCore();
        public abstract Task LoadCoreAsync();
        public abstract void SaveCore();
        public abstract Task SaveCoreAsync();
    }
}
