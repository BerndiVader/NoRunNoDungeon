using Godot;
using System;

public class Background : ParallaxBackground
{
    [Export] private Vector2 Movement=new Vector2(-10,0);

    public override void _Process(float delta) 
    {
        ScrollOffset+=(Movement*delta);
    }
}
