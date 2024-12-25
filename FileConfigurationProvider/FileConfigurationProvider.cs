using ConfigurationProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileConfigurationProvider
{
    public class FileConfigurationProvider : IConfigurationProvider
    {
        public object LoadSetting(string settingName, Type propertyType)
        {
            string json = File.ReadAllText("appSettings.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dict != null && dict.TryGetValue(settingName, out object value) ? Convert.ChangeType(value, propertyType) : null;
        }

        public void SaveSetting(string settingName, object value)
        {
            string json = File.ReadAllText("appSettings.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            dict[settingName] = value;
            json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText("appSettings.json", json);
        }
    }
}
