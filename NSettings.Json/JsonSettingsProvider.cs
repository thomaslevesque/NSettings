using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NSettings.Json
{
    public class JsonSettingsProvider : SettingsProviderBase
    {
        private readonly JsonSerializer _serializer;

        public JsonSettingsProvider(IStreamStorageProvider storageProvider)
            : base(storageProvider)
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
        }

        private JObject _jsonSettings;

        protected override void LoadSection(string sectionName, object section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));

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
            if (section == null) throw new ArgumentNullException(nameof(section));

            EnsureLoaded();

            var settings = _jsonSettings ?? new JObject();
            settings[sectionName] = JObject.FromObject(section);
            _jsonSettings = settings;
        }

        protected override void LoadDefaults()
        {
            _jsonSettings = new JObject();
        }

        protected override void LoadCore(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                LoadFromJson(json);
            }
        }

        protected override async Task LoadCoreAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                LoadFromJson(json);
            }
        }

        protected override void SaveCore(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                string json = SaveToJson();
                writer.WriteLine(json);
            }
        }

        protected override async Task SaveCoreAsync(Stream stream)
        {
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
