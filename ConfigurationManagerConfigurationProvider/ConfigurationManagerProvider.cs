using ConfigurationProviders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManagerConfigurationProvider
{
    public class ConfigurationManagerProvider : IConfigurationProvider
    {
        public object LoadSetting(string settingName, Type propertyType)
        {
            string value = ConfigurationManager.AppSettings[settingName];
            return value != null ? Convert.ChangeType(value, propertyType) : null;
        }

        public void SaveSetting(string settingName, object value)
        {
            // Equivalent functionality as writing to ConfigurationManager at runtime is not straightforward.
        }
    }
}
