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
        speed=World.level!=null?World.level.direction*(World.level.speed*0.5f):Vector2.Zero;
        ScrollBaseOffset+=speed*delta;
    }
}
