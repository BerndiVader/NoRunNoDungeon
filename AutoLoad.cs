using Godot;
using System;
using System.Reflection;

public class AutoLoad : Node
{
    public override void _Ready()
    {
        if(!OS.IsDebugBuild())
        {
            OS.VsyncEnabled=true;
            OS.WindowFullscreen=true;
        }
        else
        {
    	    OS.WindowSize=new Vector2(1024,576);
        }
        new Worker();
        ResourceUtils.Init();
        World.Init(GetTree().Root);
        QueueFree();
    }

}
