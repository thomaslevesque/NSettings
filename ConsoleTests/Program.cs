using System;
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
            var provider = GetJsonProvider();
            provider.Load();

            var settings = provider.GetSettings<ServerSettings>();

            settings.ServerUrl = "http://www.foobar.com/";
            settings.ProxySettings = new ProxySettings
            {
                Host = "www.foobarproxy.com",
                Port = 80
            };
            provider.Save();

            DumpSettings(settings);

            Console.ReadLine();

            provider.Load();
            DumpSettings(settings);
            Console.ReadLine();

        }

        static void DumpSettings(ServerSettings settings)
        {
            Console.WriteLine("ServerUrl    : {0}", settings.ServerUrl);
            Console.WriteLine("ProxySettings:");
            if (settings.ProxySettings != null)
            {
                Console.WriteLine("\tHost    : {0}", settings.ProxySettings.Host);
                Console.WriteLine("\tPort    : {0}", settings.ProxySettings.Port);
                Console.WriteLine("\tUserName: {0}", settings.ProxySettings.UserName);
                Console.WriteLine("\tPassword: {0}", settings.ProxySettings.Password);
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
    }

    public class ProxySettings
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
