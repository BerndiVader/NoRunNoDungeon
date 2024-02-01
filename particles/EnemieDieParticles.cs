using Godot;
using System;

public class EnemieDieParticles : CPUParticles2D
{
    public override void _Ready()
    {
        ExplodeGfx explode=(ExplodeGfx)ResourceUtils.gfx[MathUtils.randomRangeInt(0,4)].Instance();
        explode.Position=Position;
        World.level.AddChild(explode);
    }

    public override void _Process(float delta) 
    {
        if(!Emitting) 
        {
            QueueFree();
        }
    }
}
