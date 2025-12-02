using Godot;
using System;

public class Touch : Sprite
{
    public Stick stick;
    public Vector2 oPosition;

    public override void _Ready()
    {
        stick=GetNode<Stick>(nameof(Stick));
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
    }

}
