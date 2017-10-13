using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConfigFile : MonoBehaviour
{

    private static string CONFIG_PATH;

    private static Dictionary<string, string> config = new Dictionary<string, string>();

    public static string getPassword()
    {
        if (config.ContainsKey("password"))
        {
            return config["password"];
        }
        Debug.LogError("Couldn't get password!");
        return "";
    }

    public static void setPassword(string password)
    {
        if (config.ContainsKey("password"))
            config.Remove("password");
        config.Add("password", password);
    }

    public static string getBaseURL()
    {
        if (config.ContainsKey("url"))
        {
            return config["url"];
        }
        return "";
    }

    public static void setBaseURL(string url)
    {
        if (config.ContainsKey("url"))
            config.Remove("url");
        Debug.LogError("Couldn't get url!");
        config.Add("url", url);
    }

    public static void refreshFromConfigFile()
    {
        using (StreamReader sr = new StreamReader(CONFIG_PATH))
        {
            string[] line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Split('=');

                if (config.ContainsKey(line[0]))
                    config.Remove(line[0]);

                config.Add(line[0], line[1]);
            }
        }
    }

    public static void saveConfig()
    {
        if (File.Exists(CONFIG_PATH)) File.Delete(CONFIG_PATH);

        StreamWriter sw = File.CreateText(CONFIG_PATH);
        foreach(KeyValuePair<string,string> kvp in config)
        {
            sw.WriteLine(kvp.Key + "=" + kvp.Value);
        }
        sw.Close();
    }

    public void Start()
    {
        CONFIG_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar + "config.txt";
        if (!File.Exists(CONFIG_PATH))
        {
            StreamWriter sw = File.CreateText(CONFIG_PATH);
            sw.WriteLine("password=ENTERPASSWORDHERE");
            sw.WriteLine("url=http://orfdms.azurewebsites.net");
            sw.Close();
        }
        refreshFromConfigFile();
    }
}
