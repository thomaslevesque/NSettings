using System;

namespace NSettings
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SettingsSectionAttribute : Attribute
    {
        private readonly string _name;

        public SettingsSectionAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
