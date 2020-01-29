using Godot;
using System;
using System.Collections.Generic;

public class WorldUtils
{
    static World world;

    public WorldUtils(World world) {
        WorldUtils.world=world;
    }

    public static World getWorld() {
        return world;
    }
    public static void setworld(World world) {
        WorldUtils.world=world;
    }

}