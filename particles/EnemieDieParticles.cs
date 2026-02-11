using Godot;
using System;

public class EnemieDieParticles : CPUParticles2D
{
    private static readonly AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/15_Impact_flesh_02.wav");

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);

        ExplodeGfx explode=(ExplodeGfx)ResourceUtils.gfx[MathUtils.RandomRange(0,ResourceUtils.gfx.Count)].Instance();
        explode.Position=Position;

        SfxPlayer sfxPlayer=new SfxPlayer
        {
            Stream=sfx,
            Position=explode.Position
        };

        World.level.AddChild(sfxPlayer);
        World.level.AddChild(explode);

        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting)
        {
            SetPhysicsProcess(false);
            QueueFree();
        }
    }
}
