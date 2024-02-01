using Godot;
using System;

#if TOOLS
[Tool]
public class PlaceholderTool : Node2D
{
    public override void _Ready()
    {
        
    }

}

#else

public class PlaceholderTool : Node2D {}

#endif