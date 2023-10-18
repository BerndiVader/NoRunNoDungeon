using Godot;
using System;

public class Touch : Sprite
{
    public Stick stick;
    public Vector2 oPosition;

    public override void _Ready()
    {
        stick=GetNode<Stick>(nameof(Stick));
        SetProcess(true);
        SetPhysicsProcess(false);
        SetProcessInput(false);
    }

    public override void _Process(float delta)
    {
        Scale=PlayerCamera.instance.Zoom;
        Position=PlayerCamera.instance.relativePosition()+oPosition*Scale;
    }

}
