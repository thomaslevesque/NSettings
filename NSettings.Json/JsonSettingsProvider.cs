using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NSettings.Json
{
    public class JsonSettingsProvider : SettingsProviderBase
    {
        private readonly IStreamStorageProvider _storageProvider;
        private readonly JsonSerializer _serializer;

        public JsonSettingsProvider(IStreamStorageProvider storageProvider)
        {
            if (storageProvider == null) throw new ArgumentNullException("storageProvider");
            _storageProvider = storageProvider;
            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
        }

        private JObject _jsonSettings;

        protected override TSettings CreateSection<TSettings>(string sectionName)
        {
            var section = new TSettings();
            LoadSection(sectionName, section);
            return section;
        }

        protected override void LoadSection(string sectionName, object section)
        {
            if (section == null) throw new ArgumentNullException("section");

            EnsureLoaded();

            var jsonSection = _jsonSettings[sectionName];
            if (jsonSection != null)
            {
                using (var reader = jsonSection.CreateReader())
                {
                    _serializer.Populate(reader, section);
                }
            }
        }

        protected override void SaveSection(string sectionName, object section)
        {
            if (section == null) throw new ArgumentNullException("section");

            var settings = _jsonSettings ?? new JObject();
            settings[sectionName] = JObject.FromObject(section);
            _jsonSettings = settings;
        }

        public override void LoadCore()
        {
            using (var stream = _storageProvider.OpenRead())
            {
                if (stream == null)
                {
                    _jsonSettings = new JObject();
                    return;
                }

                using (var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    LoadFromJson(json);
                }
            }
        }

        public override async Task LoadCoreAsync()
        {
            using (var stream = await _storageProvider.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                LoadFromJson(json);
            }
        }

        public override void SaveCore()
        {
            using (var stream = _storageProvider.OpenWrite())
            {
                if (stream == null)
                {
                    _jsonSettings = new JObject();
                    return;
                }

                using (var writer = new StreamWriter(stream))
                {
                    string json = SaveToJson();
                    writer.WriteLine(json);
                }
            }
        }

        public override async Task SaveCoreAsync()
        {
            using (var stream = await _storageProvider.OpenWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                string json = SaveToJson();
                await writer.WriteLineAsync(json);
            }
        }

        private void LoadFromJson(string json)
        {
            _jsonSettings = JObject.Parse(json);
        }

        private string SaveToJson()
        {
            EnsureLoaded();
            return _jsonSettings.ToString();
        }

        private void EnsureLoaded()
        {
            if (_jsonSettings == null)
                throw new InvalidOperationException("The settings haven't been loaded");
        }
    }
}
