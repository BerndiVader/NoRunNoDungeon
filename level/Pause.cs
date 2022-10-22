using Godot;
using System;

public class Pause : TileMap
{
    public override void _Ready()
    {
        ZIndex=1000;
    }

    public override void _Process(float delta)
    {
        if(World.instance.input.getPause()) {
            World.instance.GetTree().Paused=false;
            World.instance.state=World.instance.oldState;
            QueueFree();
        }
    }
}
