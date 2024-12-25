using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationProviders
{
    public interface IConfigurationProvider
    {
        object LoadSetting(string settingName, Type propertyType);
        void SaveSetting(string settingName, object value);
    }
}
