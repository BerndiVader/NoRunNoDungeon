using Godot;
using System;
using System.Collections.Generic;

public static class ResourceUtils
{
    public static PackedScene player;
    public static List<PackedScene> levels;
    public static List<PackedScene> enemies;

    public static void Init() {

        levels=new List<PackedScene>();
        enemies=new List<PackedScene>();

        player=(PackedScene)ResourceLoader.Load("res://Player.tscn");

        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level.tscn"));
        levels.Add((PackedScene)ResourceLoader.Load("res://level/Level1.tscn"));

    }

}
