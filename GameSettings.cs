using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class GameSettings
{
    public class Config
    {
        public float masterVolume {get;set;}
        public float sfxVolume {get;set;}
        public float BackgroundVolume {get;set;}
        public bool fullscreen {get;set;}
        public bool vsync {get;set;}
        public Viewport.UsageEnum usage {get;set;}
        public Tuple<float,float> windowSize {get;set;}
        public Tuple<float,float> windowPosition {get;set;}

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

            masterVolume=-12f;
            sfxVolume=0f;
            BackgroundVolume=-8f;

            fullscreen=false;
            windowSize=new Tuple<float, float>(1024f,576f);
            windowPosition=new Tuple<float,float>(0f,0f);
            vsync=false;
            usage=Viewport.UsageEnum.Usage3d;
        }

        public void setAll(Node node)
        {
            setAll();
            node.GetTree().Root.Usage=usage;
        }

        private void setAll()
        {
            AudioServer.SetBusVolumeDb(masterBus,masterVolume);
            AudioServer.SetBusVolumeDb(sfxBus,sfxVolume);
            AudioServer.SetBusVolumeDb(backgroundBus,BackgroundVolume);
            OS.WindowFullscreen=fullscreen;
            if(!fullscreen)
            {
                OS.WindowPosition=new Vector2(windowPosition.Item1,windowPosition.Item2);
                OS.WindowSize=new Vector2(windowSize.Item1,windowSize.Item2);
            }
            OS.VsyncEnabled=vsync;
            
        }

        public Config clone()
        {
            string json=JsonSerializer.Serialize<Config>(this);
            Config config=JsonSerializer.Deserialize<Config>(json);
            return config;
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
        string json=JsonSerializer.Serialize<Config>(config);
        System.IO.File.WriteAllText(PATH_NAME+FILE_NAME,json);
    }

    public static Config loadConfig()
    {
        string json=System.IO.File.ReadAllText(PATH_NAME+FILE_NAME);
        Config config=JsonSerializer.Deserialize<Config>(json);
        return config;
        
    }

    public static void defaultConfig()
    {
        current=new Config();
    }

}
