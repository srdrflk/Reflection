using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using ConfigurationProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    private Assembly LoadAssembly()
    {
        try
        {
            return Assembly.Load("FileConfigurationProvider");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load the ConfigurationProviders assembly: " + ex.Message);
            return null;
        }
    }

    public void LoadSettings()
    {
        var assembly = LoadAssembly();

        var properties = GetType().GetProperties();
        var attribute0 = properties.FirstOrDefault().GetCustomAttributes();
        var attribute1 = properties[1].GetCustomAttributes();

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            foreach (Attribute attribute in property.GetCustomAttributes())
            {
                if (attribute is FileConfigurationItemAttribute)
                {
                    string providerTypeName = "FileConfigurationProvider." + attribute.GetType().Name.Replace("ItemAttribute", "") + "Provider";
                    Type providerType = assembly.GetType(providerTypeName);

                    if (providerType == null)
                    {
                        Console.WriteLine($"Type {providerTypeName} not found.");
                        continue;
                    }

                    IConfigurationProvider provider = Activator.CreateInstance(providerType) as IConfigurationProvider;
                    object settingValue = provider?.LoadSetting(property.Name, property.PropertyType);
                    if (settingValue != null)
                    {
                        property.SetValue(this, settingValue);
                    }
                }


                if (attribute is ConfigurationManagerConfigurationItemAttribute configManagerAttr)
                {
                    string json = File.ReadAllText("appSettings.json");
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (dict.TryGetValue(configManagerAttr.SettingName, out object value))
                    {
                        property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
        }
    }

    public void SaveSettings()
    {
        var assembly = LoadAssembly();
        var settings = new Dictionary<string, object>();

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            foreach (Attribute attribute in property.GetCustomAttributes())
            {

                if (attribute is FileConfigurationItemAttribute fileAttr)
                {
                    string providerTypeName = "FileConfigurationProvider." + attribute.GetType().Name.Replace("ItemAttribute", "") + "Provider";
                    Type providerType = assembly.GetType(providerTypeName);

                    if (providerType == null)
                    {
                        Console.WriteLine($"Type {providerTypeName} not found.");
                        continue;
                    }

                    IConfigurationProvider provider = Activator.CreateInstance(providerType) as IConfigurationProvider;
                    settings[fileAttr.SettingName] = property.GetValue(this);
                    provider?.SaveSetting(property.Name, settings[fileAttr.SettingName]);
                }

                if (attribute is ConfigurationManagerConfigurationItemAttribute configManagerAttr)
                {
                    object value = property.GetValue(this);
                    settings[configManagerAttr.SettingName] = value;
                    string json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText("appSettings.json", json);
                }
                
            }
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
            settings.ConnectionString = "Task2Value";
            settings.MaxItems = 30;

            settings.SaveSettings();
        }
    }
}