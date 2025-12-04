using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ResourceUtils
{
    private static readonly bool DEBUG=true;
    public static PackedScene world;
    public static PackedScene intro;
    public static PackedScene pause;
    public static PackedScene player;
    public static PackedScene camera;
    public static List<PackedScene> levels;
    public static List<PackedScene> enemies;
    public static List<PackedScene> bullets;
    public static List<PackedScene> weapons;
    public static List<TileSet> tilesets;
    public static List<PackedScene> particles;
    public static List<PackedScene> gfx;
    public static PackedScene background;
    public static List<Texture> bgTextures;
    public static List<AudioStreamMP3>ingameMusic;
    public static PackedScene touch;
    public static PackedScene buttons;

    public static void Init() 
    {
        levels=new List<PackedScene>();
        enemies=new List<PackedScene>();
        tilesets=new List<TileSet>();
        bgTextures=new List<Texture>();
        particles=new List<PackedScene>();
        bullets=new List<PackedScene>();
        gfx=new List<PackedScene>();
        weapons=new List<PackedScene>();
        ingameMusic=new List<AudioStreamMP3>();

        bool pck=ProjectSettings.LoadResourcePack("res://nodungeon.pck");
        if(pck)
        {
            Console.WriteLine("Found external main dlc, using it.");
        }
        else
        {
            Console.WriteLine("No external main dlc found.");
        }

        if(!GameSettings.isMobile)
        {
            Console.WriteLine("Check for addons...");
            Directory dir=new Directory();
            List<string>files=new List<string>();

            if(dir.DirExists("res://dlcs"))
            {
                dir.Open("res://dlcs");
                dir.ListDirBegin();

                string file=dir.GetNext();
                while(file!="")
                {
                    if(file.EndsWith(".pck"))
                    {
                        files.Add("res://dlcs/"+file);
                    }
                    file=dir.GetNext();
                }
                dir.ListDirEnd();
            }
            else
            {
                dir.MakeDir("res://dlcs");
            }
            if(files.Count>0)
            {
                files.ForEach(file=>
                {
                    if(ProjectSettings.LoadResourcePack(file,false))
                    {
                        Console.WriteLine("Loaded "+file);
                    }
                    else
                    {
                        Console.WriteLine("Failed to load "+file);
                    }
                });
            }
            else
            {
                Console.WriteLine("No addons found.");
            }
        }

        if(GameSettings.isMobile)
        {
            Console.WriteLine("Found Mobile, loading touch input");
            touch=ResourceLoader.Load<PackedScene>("res://io/Touch.tscn");
            buttons=ResourceLoader.Load<PackedScene>("res://io/Buttons.tscn");
        }

        Console.WriteLine("Loading Resources...");
        world=ResourceLoader.Load<PackedScene>("res://World.tscn");
        intro=ResourceLoader.Load<PackedScene>("res://intro/Intro.tscn");
        pause=ResourceLoader.Load<PackedScene>("res://ui/PauseUI.tscn");
        player=ResourceLoader.Load<PackedScene>("res://Player.tscn");
        background=ResourceLoader.Load<PackedScene>("res://level/Background.tscn");
        camera=ResourceLoader.Load<PackedScene>("res://PlayerCamera.tscn");

        Console.WriteLine("Loading levels...");
        if(DEBUG)
        {
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/Level.tscn"));
        }
        else
        {
            levels.Add(ResourceLoader.Load<PackedScene>("res://level/one/1.tscn"));
        }

        Console.WriteLine("Loading tilesets...");
        tilesets.Add(ResourceLoader.Load<TileSet>("res://level/tileset1.tres"));
        tilesets.Add(ResourceLoader.Load<TileSet>("res://level/tileset2.tres"));
        tilesets.Add(ResourceLoader.Load<TileSet>("res://level/tileset3.tres"));
        tilesets.Add(ResourceLoader.Load<TileSet>("res://level/tileset4.tres"));

        Console.WriteLine("Loading background textures...");
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_A/PNG/bg1.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_A/PNG/bg2.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_A/PNG/bg3.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_B/PNG/bg1.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_B/PNG/bg2.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_B/PNG/bg3.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_C/PNG/bg1.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_C/PNG/bg2.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_C/PNG/bg3.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_D/PNG/bg1.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_D/PNG/bg2.png"));
        bgTextures.Add(ResourceLoader.Load<Texture>("res://image/super_pixel_cave/style_D/PNG/bg3.png"));

        Console.WriteLine("Loading particles...");
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/BlockParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/CoinTakenParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/EnemieDieParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/DaggerMissParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/MadRockParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/WeaponChangeParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/JumpParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/DaggerShoot.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/PlayerDieEffect.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/ExplodeParticles.tscn"));

        Console.WriteLine("Loading bullets...");
        bullets.Add((PackedScene)ResourceLoader.Load("res://bullets/SkullBullet.tscn"));
        bullets.Add((PackedScene)ResourceLoader.Load("res://bullets/DaggerBullet.tscn"));

        Console.WriteLine("Loading visual effects...");
        gfx.Add(ResourceLoader.Load<PackedScene>("res://gfx/ExplodeGfx1.tscn"));
        gfx.Add(ResourceLoader.Load<PackedScene>("res://gfx/ExplodeGfx2.tscn"));
        gfx.Add(ResourceLoader.Load<PackedScene>("res://gfx/ExplodeGfx3.tscn"));
        gfx.Add(ResourceLoader.Load<PackedScene>("res://gfx/ExplodeGfx4.tscn"));
        gfx.Add(ResourceLoader.Load<PackedScene>("res://gfx/ExplodeGfx5.tscn"));

        Console.WriteLine("Loading weapons...");
        weapons.Add(ResourceLoader.Load<PackedScene>("res://weapons/Sword.tscn"));
        weapons.Add(ResourceLoader.Load<PackedScene>("res://weapons/Broadsword.tscn"));
        weapons.Add(ResourceLoader.Load<PackedScene>("res://weapons/Dagger.tscn"));

        Console.WriteLine("Loading ingame musics...");
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 01 Stronghold Of The Barbarians.mp3"));
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 02 The Lightning - Riven Crag Of Lord Doom.mp3"));
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 03 Orcs And Goblins Emerge From The Bogswamps.mp3"));
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 05 Battle Theme I.mp3"));
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 09 The Rift Of Ever - Burning Fire.mp3"));
        ingameMusic.Add(ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/music/Dark Age 10 Battle Theme II - 1.mp3"));


        Console.WriteLine("Done!");
    }

    public static InputController getInputController(Node scene)
    {
        if(GameSettings.isMobile)
        {
            return new MobileInput(scene);
        }
        else
        {
            return new DesktopInput();
        }
    }
}