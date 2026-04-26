using Godot;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class GameSettings
{
    public class Config
    {
        public float MasterVolume {get;set;}
        public float SfxVolume {get;set;}
        public float BackgroundVolume {get;set;}
        public bool Fullscreen {get;set;}
        public bool Vsync {get;set;}
        public bool Glow {get;set;}
        public bool Light {get;set;}
        public bool Rumble {get;set;}
        public Viewport.UsageEnum Usage {get;set;}
        public float WindowSizeX{get;set;}
        public float WindowSizeY{get;set;}
        public float WindowPositionX {get;set;}
        public float WindowPositionY {get;set;}
        public bool LowProcessorUsageMode {get;set;}
        public int TargetFps {get;set;}
        public bool KeepScreenOn {get;set;}
        public InputCandit Input {get;set;}

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

            MasterVolume=-12f;
            SfxVolume=0f;
            BackgroundVolume=-8f;

            Fullscreen=false;
            WindowSizeX=1024f;
            WindowSizeY=576f;
            WindowPositionX=0f;
            WindowPositionY=0f;
            Vsync=false;
            Usage=Viewport.UsageEnum.Usage3d;
            Light=true;
            Glow=true;
            Rumble=true;
            LowProcessorUsageMode=false;
            KeepScreenOn=true;
            TargetFps=0;

            if(GameSettings.isMobile)
            {
                Input=InputCandit.Touchpad();
            }
            else
            {
                Input=InputCandit.Keyboard();
            }
        }

        public void SetAll(Node node)
        {
            SetAll();
            node.GetTree().Root.Usage=Usage;
        }

        private void SetAll()
        {
            AudioServer.SetBusVolumeDb(masterBus,MasterVolume);
            AudioServer.SetBusVolumeDb(sfxBus,SfxVolume);
            AudioServer.SetBusVolumeDb(backgroundBus,BackgroundVolume);
            OS.WindowFullscreen=Fullscreen;
            if(!Fullscreen)
            {
                OS.WindowPosition=new Vector2(WindowPositionX,WindowPositionY);
                OS.WindowSize=new Vector2(WindowSizeX,WindowSizeY);
            }
            OS.VsyncEnabled=Vsync;
            OS.LowProcessorUsageMode=LowProcessorUsageMode;
            OS.KeepScreenOn=KeepScreenOn;
            Engine.TargetFps=TargetFps;
        }

        public Config Clone()
        {
            string json=JsonConvert.SerializeObject(this);
            Config config=JsonConvert.DeserializeObject<Config>(json);
            return config;
        }        

    }

    public class InputCandit
    {
        public enum TYPE
        {
            UNKNOWN,
            KEYBOARD,
            TOUCHPAD,
            JOYPAD
        }

        public string name;
        public TYPE type;
        public int deviceID;

        public static InputCandit Keyboard()
        {
            return new InputCandit
            {
                type=TYPE.KEYBOARD,
                deviceID=0,
                name="Keyboard"
            };
        }

        public static InputCandit Touchpad()
        {
            return new InputCandit
            {
                type=TYPE.TOUCHPAD,
                deviceID=0,
                name="Touchpad"
            };
        }
    }

    public static bool isMobile;

    public static Config current;
    private static readonly List<InputCandit>inputCandits=new List<InputCandit>();

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

    public static List<InputCandit> AvailInputs()
    {
        inputCandits.Clear();
        DefaultInputs();

        foreach(int id in Input.GetConnectedJoypads())
        {
            if(Input.IsJoyKnown(id))
            {
                inputCandits.Add(
                    new InputCandit
                    {
                        deviceID=id,
                        type=InputCandit.TYPE.JOYPAD,
                        name=Input.GetJoyName(id)+":"+id,
                    }
                );
            }
        }

        return inputCandits;
    }

    private static void DefaultInputs()
    {
        if(!isMobile)
        {
            inputCandits.Add(InputCandit.Keyboard());
        }
        else
        {
            inputCandits.Add(InputCandit.Touchpad());
        }
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
