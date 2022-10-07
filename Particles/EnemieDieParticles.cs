using Godot;
using System;

public class EnemieDieParticles : CPUParticles2D
{
    public override void _Ready()
    {
        ExplodeGfx explode=(ExplodeGfx)ResourceUtils.gfx[MathUtils.randomRangeInt(0,4)].Instance();
        explode.Position=Position;
        World.instance.level.AddChild(explode);
    }

    public override void _Process(float delta) 
    {
        if(!Emitting) 
        {
            SetProcess(false);
            QueueFree();
        }
    }

    public void _enteredTree()
    {
        this.Emitting=true;
        SetProcess(true);
    }
}
