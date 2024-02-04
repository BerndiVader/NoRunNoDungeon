using Godot;
using System;

public class EnemieDieParticles : CPUParticles2D
{
    static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/15_Impact_flesh_02.wav");

    public override void _Ready()
    {
        ExplodeGfx explode=(ExplodeGfx)ResourceUtils.gfx[MathUtils.randomRangeInt(0,4)].Instance();
        explode.Position=Position;

        SfxPlayer sfxPlayer=new SfxPlayer();
        sfxPlayer.Stream=sfx;
        sfxPlayer.Position=explode.Position;

        World.level.AddChild(sfxPlayer);
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
