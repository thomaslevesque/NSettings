using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NSettings.Xml
{
    public class XmlSettingsProvider : SettingsProviderBase
    {
        public XmlSettingsProvider(IStreamStorageProvider storageProvider)
            : base(storageProvider)
        {
        }

        private XDocument _xmlSettings;
        protected override void LoadSection(string sectionName, object section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));

            EnsureLoaded();

            var xmlSection = _xmlSettings.Element(sectionName);
            if (xmlSection != null)
            {
                using (var reader = xmlSection.CreateReader())
                {
                    var serializer = new XmlSerializer(section.GetType());
                    var loadedSection = serializer.Deserialize(reader);
                    CopyObject(loadedSection, section);
                }
            }
        }

        protected override void SaveSection(string sectionName, object section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));

            EnsureLoaded();

            var xmlSection = new XDocument();
            using (var writer = xmlSection.CreateWriter())
            {
                var serializer = new XmlSerializer(section.GetType(), new XmlRootAttribute(sectionName));
                serializer.Serialize(writer, section);
            }
            var oldXmlSection = _xmlSettings.Element(sectionName);
            if (oldXmlSection != null)
                oldXmlSection.ReplaceWith(xmlSection.Root);
            else
                _xmlSettings.Add(xmlSection.Root);
        }

        protected override void LoadDefaults()
        {
            _xmlSettings = new XDocument();
        }

        protected override void LoadCore(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string xml = reader.ReadToEnd();
                LoadFromXml(xml);
            }
        }

        protected override async Task LoadCoreAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string xml = await reader.ReadToEndAsync();
                LoadFromXml(xml);
            }
        }

        protected override void SaveCore(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                string xml = SaveToXml();
                writer.Write(xml);
            }
        }

        protected override async Task SaveCoreAsync(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                string xml = SaveToXml();
                await writer.WriteAsync(xml);
            }
        }

        private void LoadFromXml(string xml)
        {
            _xmlSettings = XDocument.Parse(xml);
        }

        private string SaveToXml()
        {
            EnsureLoaded();
            return _xmlSettings.ToString();
        }

        private void EnsureLoaded()
        {
            if (_xmlSettings == null)
                throw new InvalidOperationException("The settings haven't been loaded");
        }

        private static void CopyObject(object source, object destination)
        {
            var type = source.GetType();
            foreach (var property in type.GetRuntimeProperties())
            {
                if (!property.CanWrite || !property.CanRead)
                    continue;
                if (property.GetMethod.IsStatic)
                    continue;
                if (!property.GetMethod.IsPublic)
                    continue;
                if (property.GetIndexParameters().Any())
                    continue;

                object value = property.GetValue(source);
                object existingValue = null;
                if (!IsValue(property.PropertyType))
                    existingValue = property.GetValue(destination);

                if (existingValue == null)
                    property.SetValue(destination, value);
                else
                    CopyObject(value, existingValue);
            }
        }

        private static bool IsValue(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || typeInfo.IsValueType || type == typeof (string);
        }
    }
}
