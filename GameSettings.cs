using Godot;
using System;
using Newtonsoft.Json;

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
        public float windowSizeX{get;set;}
        public float windowSizeY{get;set;}
        public float windowPositionX {get;set;}
        public float windowPositionY {get;set;}

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
            windowSizeX=1024f;
            windowSizeY=576f;
            windowPositionX=0f;
            windowPositionY=0f;
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
                OS.WindowPosition=new Vector2(windowPositionX,windowPositionY);
                OS.WindowSize=new Vector2(windowSizeX,windowSizeY);
            }
            OS.VsyncEnabled=vsync;
            
        }

        public Config clone()
        {
            string json=JsonConvert.SerializeObject(this);
            Config config=JsonConvert.DeserializeObject<Config>(json);
            return config;
        }        

    }

    public static Boolean isMobile;

    public static Config current;
    private static string ROOT_NAME;
    private static string CONFIG_DIR=>ROOT_NAME+"gamesettings/";
    private static string CONFIG_NAME=>"config.json";

    public static void Init()
    {
        isMobile=OS.GetName().Equals("android",StringComparison.OrdinalIgnoreCase);
        ROOT_NAME=isMobile?"user://":"res://";

        current=new Config();
        Directory dir=new Directory();
        if(!dir.DirExists(CONFIG_DIR))
        {
            dir.MakeDir(CONFIG_DIR);
            SaveConfig(current);
        }
        else if(!dir.FileExists(CONFIG_DIR+CONFIG_NAME))
        {
            SaveConfig(current);
        }
        SaveConfig(current);
        current=LoadConfig();
    }

    public static void SaveConfig(Config config)
    {
        File file=new File();
        file.Open(CONFIG_DIR+CONFIG_NAME,File.ModeFlags.Write);
        file.StoreString(JsonConvert.SerializeObject(config));
        file.Close();
    }

    public static Config LoadConfig()
    {
        try
        {
            File file=new File();
            if(file.FileExists(CONFIG_DIR+CONFIG_NAME))
            {
                file.Open(CONFIG_DIR+CONFIG_NAME,File.ModeFlags.Read);
                Config config=JsonConvert.DeserializeObject<Config>(file.GetAsText());
                file.Close();
                return config;
            }
        }
        catch(Exception e)
        {
            GD.Print(e);
        }
        return new Config();
    }

    public static void DefaultConfig()
    {
        current=new Config();
    }

}
