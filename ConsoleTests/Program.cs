using System;
using System.Collections.Generic;
using NSettings;
using NSettings.Desktop;
using NSettings.Json;
using NSettings.Xml;

namespace ConsoleTests
{
    class Program
    {
        static void Main()
        {
            Func<ISettingsProvider> providerFactory = GetXmlProvider;

            var settings = CreateSettings(providerFactory);
            DumpSettings(settings);
            Console.ReadLine();

            settings = ReadSettings(providerFactory);
            DumpSettings(settings);
            Console.ReadLine();

        }

        private static ServerSettings CreateSettings(Func<ISettingsProvider> providerFactory)
        {
            var provider = providerFactory();
            provider.Load();

            var settings = provider.GetSettings<ServerSettings>();

            settings.ServerUrl = "http://www.foobar.com/";
            settings.ProxySettings = new ProxySettings
            {
                Host = "www.foobarproxy.com",
                Port = 80
            };
            settings.Items = new List<Item>()
            {
                new Item { Id = 1, Name = "test" },
                new Item { Id = 42, Name = "blah" }
            };
            provider.Save();
            return settings;
        }

        private static ServerSettings ReadSettings(Func<ISettingsProvider> providerFactory)
        {
            var provider = providerFactory();
            provider.Load();
            return provider.GetSettings<ServerSettings>();
        }

        static void DumpSettings(ServerSettings settings)
        {
            Console.WriteLine("ServerUrl    : {0}", settings.ServerUrl);
            Console.WriteLine("ProxySettings:");
            if (settings.ProxySettings != null)
            {
                Console.WriteLine($"\tHost    : {settings.ProxySettings.Host}");
                Console.WriteLine($"\tPort    : {settings.ProxySettings.Port}");
                Console.WriteLine($"\tUserName: {settings.ProxySettings.UserName}");
                Console.WriteLine($"\tPassword: {settings.ProxySettings.Password}");
            }
            if (settings.Items != null)
            {
                Console.WriteLine("Items     :");
                foreach (var item in settings.Items)
                {
                    Console.WriteLine($"\t- {{ Id = {item.Id}, Name = {item.Name} }}");
                }
            }
        }

        static ISettingsProvider GetJsonProvider()
        {
            var storage = new FileStreamStorageProvider("config.json");
            var provider = new JsonSettingsProvider(storage);
            return provider;
        }

        static ISettingsProvider GetXmlProvider()
        {
            var storage = new FileStreamStorageProvider("config.xml");
            var provider = new XmlSettingsProvider(storage);
            return provider;
        }
    }

    public class ServerSettings
    {
        public string ServerUrl { get; set; }
        public ProxySettings ProxySettings { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }

    public class ProxySettings
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
