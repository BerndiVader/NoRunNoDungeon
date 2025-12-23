using Godot;
using System;

public class Background : ParallaxBackground
{
    private Vector2 speed=Vector2.Zero;

    public override void _PhysicsProcess(float delta) 
    {
        speed.x=World.level!=null?-World.level.Speed*0.1f:-10f;
        ScrollBaseOffset+=speed*delta;
    }
}
