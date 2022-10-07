using Godot;
using System;

public class Pause : TileMap
{
    private InputController input;

    public override void _Ready()
    {
        input=World.instance.input;
        ZIndex=1000;
    }

    public override void _Process(float delta)
    {
        if(input.getPause()) {
            World.instance.GetTree().Paused=false;
            World.instance.state=World.instance.oldState;
            QueueFree();
        }
    }
}
