using Godot;
using System;
using System.Collections.Generic;

public static class ResourceUtils
{
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
    public static PackedScene touch;
    public static PackedScene buttons;

    public static bool isMobile;

    public static void Init() 
    {
        isMobile=OS.GetName().ToLower().Equals("android");

        levels=new List<PackedScene>();
        enemies=new List<PackedScene>();
        tilesets=new List<TileSet>();
        bgTextures=new List<Texture>();
        particles=new List<PackedScene>();
        bullets=new List<PackedScene>();
        gfx=new List<PackedScene>();
        weapons=new List<PackedScene>();

        if(isMobile)
        {
            touch=(PackedScene)ResourceLoader.Load("res://io/Touch.tscn");
            buttons=(PackedScene)ResourceLoader.Load("res://io/Buttons.tscn");
        }

        world=(PackedScene)ResourceLoader.Load("res://World.tscn");
        intro=(PackedScene)ResourceLoader.Load("res://intro/Intro.tscn");
        pause=(PackedScene)ResourceLoader.Load("res://ui/PauseUI.tscn");
        player=(PackedScene)ResourceLoader.Load("res://Player.tscn");
        background=(PackedScene)ResourceLoader.Load("res://level/Background.tscn");
        camera=ResourceLoader.Load<PackedScene>("res://PlayerCamera.tscn");

        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level1.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level2.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level3.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level4.tscn"));

        tilesets.Add((TileSet)ResourceLoader.Load("res://level/tileset1.tres"));
        tilesets.Add((TileSet)ResourceLoader.Load("res://level/tileset2.tres"));
        tilesets.Add((TileSet)ResourceLoader.Load("res://level/tileset3.tres"));
        tilesets.Add((TileSet)ResourceLoader.Load("res://level/tileset4.tres"));

        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg1.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg2.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg3.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_B/PNG/bg1.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_B/PNG/bg2.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_B/PNG/bg3.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_C/PNG/bg1.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_C/PNG/bg2.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_C/PNG/bg3.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_D/PNG/bg1.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_D/PNG/bg2.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_D/PNG/bg3.png"));

        particles.Add((PackedScene)ResourceLoader.Load("res://particles/BlockParticles.tscn"));
        particles.Add((PackedScene)ResourceLoader.Load("res://particles/CoinTakenParticles.tscn"));
        particles.Add((PackedScene)ResourceLoader.Load("res://particles/EnemieDieParticles.tscn"));
        particles.Add((PackedScene)ResourceLoader.Load("res://particles/DaggerMissParticles.tscn"));
        particles.Add((PackedScene)ResourceLoader.Load("res://particles/MadRockParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/WeaponChangeParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/JumpParticles.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/DaggerShoot.tscn"));
        particles.Add(ResourceLoader.Load<PackedScene>("res://particles/PlayerDieEffect.tscn"));

        bullets.Add((PackedScene)ResourceLoader.Load("res://bullets/TestBullet.tscn"));
        bullets.Add((PackedScene)ResourceLoader.Load("res://bullets/DaggerBullet.tscn"));

        gfx.Add((PackedScene)ResourceLoader.Load("res://gfx/ExplodeGfx1.tscn"));
        gfx.Add((PackedScene)ResourceLoader.Load("res://gfx/ExplodeGfx2.tscn"));
        gfx.Add((PackedScene)ResourceLoader.Load("res://gfx/ExplodeGfx3.tscn"));
        gfx.Add((PackedScene)ResourceLoader.Load("res://gfx/ExplodeGfx4.tscn"));
        gfx.Add((PackedScene)ResourceLoader.Load("res://gfx/ExplodeGfx5.tscn"));

        weapons.Add((PackedScene)ResourceLoader.Load("res://weapons/Sword.tscn"));
        weapons.Add((PackedScene)ResourceLoader.Load("res://weapons/Broadsword.tscn"));
        weapons.Add((PackedScene)ResourceLoader.Load("res://weapons/Dagger.tscn"));
    }

    public static InputController getInputController(Node scene)
    {
        if(isMobile)
        {
            return new MobileInput(scene);
        }
        else
        {
            return new DesktopInput();
        }
    }
}