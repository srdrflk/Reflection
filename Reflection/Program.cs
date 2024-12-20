using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;

// note :  before running the application create a appsettings.json file (as mentioned at line 50)   
// file path : ..\Reflection\bin\Debug\net8.0\appsettings.json

public class FileConfigurationItemAttribute : Attribute
{
    public string SettingName { get; }

    public FileConfigurationItemAttribute(string settingName)
    {
        SettingName = settingName;
    }
}

public class ConfigurationManagerConfigurationItemAttribute : Attribute
{
    public string SettingName { get; }

    public ConfigurationManagerConfigurationItemAttribute(string settingName)
    {
        SettingName = settingName;
    }
}


public class ConfigurationSettings : ConfigurationComponentBase
{
    [FileConfigurationItemAttribute("ConnectionString")]
    public string ConnectionString { get; set; }

    [ConfigurationManagerConfigurationItemAttribute("MaxItems")]
    public int MaxItems { get; set; }
}
public abstract class ConfigurationComponentBase
{
    public void LoadSettings()
    {
        foreach (var property in GetType().GetProperties())
        {
            foreach (Attribute attribute in property.GetCustomAttributes())
            {
                if (attribute is FileConfigurationItemAttribute fileAttr)
                {
                    string json = File.ReadAllText("appsettings.json");
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (dict.TryGetValue(fileAttr.SettingName, out object value))
                    {
                        property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
                    }
                }

                if (attribute is ConfigurationManagerConfigurationItemAttribute configManagerAttr)
                {
                    string value = ConfigurationManager.AppSettings[configManagerAttr.SettingName];
                    if (value != null)
                    {
                        property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
        }
    }

    public void SaveSettings()
    {
        var settings = new Dictionary<string, object>();
        foreach (var property in GetType().GetProperties())
        {
            foreach (Attribute attribute in property.GetCustomAttributes())
            {
                object value = property.GetValue(this);
                if (attribute is FileConfigurationItemAttribute fileAttr)
                {
                    settings[fileAttr.SettingName] = value;
                }
            }
        }
        string json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("appsettings.json", json);
    }
}


class Program
{
    static void Main(string[] args)
    {
        ConfigurationSettings settings = new ConfigurationSettings();
        settings.LoadSettings();

        Console.WriteLine($"ConnectionString: {settings.ConnectionString}");
        Console.WriteLine($"MaxItems: {settings.MaxItems}");

        // Modify settings
        settings.ConnectionString = "SomeOtherValue";
        settings.MaxItems = 30;

        settings.SaveSettings();
    }
}