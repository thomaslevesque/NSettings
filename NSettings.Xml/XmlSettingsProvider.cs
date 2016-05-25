using System;
using System.Collections;
using System.Collections.Generic;
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
                if (!IsValueOrPrimitiveType(property.PropertyType))
                    existingValue = property.GetValue(destination);

                if (existingValue == null)
                    property.SetValue(destination, value);
                else
                    CopyObject(value, existingValue);
            }

            if (source is IList)
            {
                CopyNonGenericIList((IList)source, (IList)destination);
            }
            else if (type.GetTypeInfo().ImplementedInterfaces.Any(IsICollectionOfT))
            {
                CopyGenericICollection(source, destination);
            }
        }

        private static bool IsValueOrPrimitiveType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || typeInfo.IsValueType || type == typeof (string);
        }

        private static bool IsICollectionOfT(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
                return false;
            var genericTypeDef = typeInfo.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(ICollection<>))
                return true;
            return false;
        }

        private static void CopyNonGenericIList(IList source, IList destination)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }

        private static void CopyGenericICollection(object source, object destination)
        {
            var iCollectionOfT = source.GetType().GetTypeInfo().ImplementedInterfaces.First(IsICollectionOfT);
            var elementType = iCollectionOfT.GenericTypeArguments[0];
            var wrapperType = typeof(GenericListNonGenericCollectionWrapper<>).MakeGenericType(elementType);
            var ctor = wrapperType.GetTypeInfo().DeclaredConstructors.First();
            var sourceList = (IList) ctor.Invoke(new[] { source });
            var destinationList = (IList) ctor.Invoke(new[] { destination });
            CopyNonGenericIList(sourceList, destinationList);
        }

        private class GenericListNonGenericCollectionWrapper<T> : IList
        {
            private readonly ICollection<T> _innerCollection;

            public GenericListNonGenericCollectionWrapper(ICollection<T> innerCollection)
            {
                _innerCollection = innerCollection;
            }

            public IEnumerator GetEnumerator()
            {
                return _innerCollection.GetEnumerator();
            }

            public void CopyTo(Array array, int index)
            {
                _innerCollection.CopyTo((T[])array, index);
            }

            public int Count => _innerCollection.Count;

            public bool IsSynchronized => false;

            public object SyncRoot { get; } = new object();

            public int Add(object value)
            {
                _innerCollection.Add((T)value);
                return 0;
            }

            public void Clear()
            {
                _innerCollection.Clear();
            }

            public bool Contains(object value)
            {
                return _innerCollection.Contains((T) value);
            }

            public int IndexOf(object value)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            public void Remove(object value)
            {
                _innerCollection.Remove((T) value);
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public bool IsFixedSize => false;
            public bool IsReadOnly => _innerCollection.IsReadOnly;

            public object this[int index]
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }
        }
    }
}
