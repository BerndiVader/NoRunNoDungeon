using Godot;
using System;

public class BackgroundLayer : ParallaxLayer
{
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
    }

}
