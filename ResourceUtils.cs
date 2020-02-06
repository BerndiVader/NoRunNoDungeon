using Godot;
using System;
using System.Collections.Generic;

public static class ResourceUtils
{
    public static PackedScene player;
    public static List<PackedScene> levels;
    public static List<PackedScene> enemies;
    public static List<TileSet> tilesets;
    public static PackedScene background;
    public static List<Texture> bgTextures;

    public static void Init() {

        levels=new List<PackedScene>();
        enemies=new List<PackedScene>();
        tilesets=new List<TileSet>();
        bgTextures=new List<Texture>();

        player=(PackedScene)ResourceLoader.Load("res://Player.tscn");

        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level1.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level2.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level1.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level2.tscn"));

        tilesets.Add((TileSet)ResourceLoader.Load("res://level/tileset1.tres"));

        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg1.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg2.png"));
        bgTextures.Add((Texture)ResourceLoader.Load("res://image/super_pixel_cave/style_A/PNG/bg3.png"));

        background=(PackedScene)ResourceLoader.Load("res://Background.tscn");

    }

}
