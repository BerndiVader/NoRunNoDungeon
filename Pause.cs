using Godot;
using System;

public class Pause : TileMap
{
    private InputController input;

    public override void _Ready()
    {
        input=WorldUtils.world.input;
        ZIndex=1000;
    }

    public override void _Process(float delta)
    {
        if(input.getPause()) {
            WorldUtils.world.GetTree().Paused=false;
            WorldUtils.world.state=WorldUtils.world.oldState;
            CallDeferred("queue_free");
        }
    }
}
