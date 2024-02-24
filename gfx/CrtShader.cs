using Godot;
using System;

public class CrtShader : ColorRect
{
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
    }

}
