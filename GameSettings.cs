using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public static class GameSettings
{
    public class Config
    {
        public float masterVolume {get;set;}
        public float sfxVolume {get;set;}
        public float BackgroundVolume {get;set;}
        public bool fullscreen {get;set;}
        public KeyValuePair<float,float>windowSize {get;set;}
        public KeyValuePair<float,float>windowPosition {get;set;}

        [JsonIgnore]
        public int masterBus;
        [JsonIgnore]
        public int sfxBus;
        [JsonIgnore]
        public int backgroundBus;
        

        public Config()
        {
            masterBus=AudioServer.GetBusIndex("Master");
            sfxBus=AudioServer.GetBusIndex("Sfx");
            backgroundBus=AudioServer.GetBusIndex("Background");

            masterVolume=0f;
            sfxVolume=0f;
            BackgroundVolume=-8f;
            fullscreen=false;
            windowSize=new KeyValuePair<float, float>(1024f,576f);
            windowPosition=new KeyValuePair<float, float>(0f,0f);
        }

        public void setAll()
        {
            AudioServer.SetBusVolumeDb(masterBus,masterVolume);
            AudioServer.SetBusVolumeDb(sfxBus,sfxVolume);
            AudioServer.SetBusVolumeDb(backgroundBus,BackgroundVolume);
            OS.WindowFullscreen=fullscreen;
            OS.WindowPosition=new Vector2(windowPosition.Key,windowPosition.Value);
            OS.WindowSize=new Vector2(windowSize.Key,windowSize.Value);
        }

    }

    public static Config current;
    private static readonly string PATH_NAME="./gamesettings/";
    private static readonly string FILE_NAME="config.json";

    public static void init()
    {
        current=new Config();
        if(!System.IO.Directory.Exists(PATH_NAME))
        {
            System.IO.Directory.CreateDirectory(PATH_NAME);
            saveConfig(current);
        }
        else if(!System.IO.File.Exists(PATH_NAME+FILE_NAME))
        {
            saveConfig(current);
        }
        current=loadConfig();
    }

    public static void saveConfig(Config config)
    {
        string json=System.Text.Json.JsonSerializer.Serialize<Config>(config);
        System.IO.File.WriteAllText(PATH_NAME+FILE_NAME,json);
    }

    public static Config loadConfig()
    {
        string json=System.IO.File.ReadAllText(PATH_NAME+FILE_NAME);
        Config config=System.Text.Json.JsonSerializer.Deserialize<Config>(json);
        return config;
        
    }


}
