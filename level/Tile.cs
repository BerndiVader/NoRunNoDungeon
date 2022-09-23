using Godot;
using System;

public class Tile : Node
{
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        
    }

}
