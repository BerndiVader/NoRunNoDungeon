using Godot;
using System;

public class EnemieDieParticles : Particles
{
    static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/15_Impact_flesh_02.wav");

    public override void _Ready()
    {
        base._Ready();
        ExplodeGfx explode=(ExplodeGfx)ResourceUtils.gfx[MathUtils.RandomRange(0,ResourceUtils.gfx.Count)].Instance();
        explode.Position=Position;

        SfxPlayer sfxPlayer=new SfxPlayer();
        sfxPlayer.Stream=sfx;
        sfxPlayer.Position=explode.Position;

        World.level.AddChild(sfxPlayer);
        World.level.AddChild(explode);

        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            QueueFree();
            SetPhysicsProcess(false);
        }
    }
}
