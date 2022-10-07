using Godot;
using System;

public class AutoLoad : Node
{
    public override void _Ready()
    {
        OS.WindowSize=new Vector2(1024,576);
        new Worker();
        ResourceUtils.Init();
        World.Init(GetTree().Root);
    }

}
