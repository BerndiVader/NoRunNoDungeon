using Godot;
using System;

public class Background : ParallaxBackground
{
    private Vector2 speed=Vector2.Zero;

    public override void _Ready()
    {
        ScrollIgnoreCameraZoom=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        speed.x=World.level!=null?-World.level.speed*0.1f:0f;
        ScrollBaseOffset+=speed*delta;
    }
}
