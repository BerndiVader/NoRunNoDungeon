using Godot;
using System;
using System.Collections.Generic;

public class WorldUtils
{
    static World world;
    static Level currentLevel;

    public WorldUtils(World world) {
        WorldUtils.world=world;
    }

    public static Level getCurrentLevel() {
        return WorldUtils.currentLevel;
    }

    public static void setCurrentLevel(Level level) {
        currentLevel=level;
    }

    public static World getWorld() {
        return world;
    }
    public static void setworld(World world) {
        WorldUtils.world=world;
    } 

}
