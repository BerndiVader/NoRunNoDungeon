using Godot;
using System;

public class PlayerDieEffect : Particles
{
    static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/Negative/Retro Negative Short 23.wav");

    public static void Create()
    {
        PlayerDieEffect effect=ResourceUtils.particles[(int)PARTICLES.PLAYERDIE].Instance<PlayerDieEffect>();
        effect.ScaleAmount*=PlayerCamera.instance.Scale.x;
        effect.Position=PlayerCamera.instance.GetCameraScreenCenter();
        SfxPlayer audio=new SfxPlayer();
        audio.Stream=sfx;
        audio.Position=effect.Position;

        World.instance.renderer.AddChild(audio);
        World.instance.renderer.AddChild(effect);
    }

    public PlayerDieEffect():base() {}

    public override void _Ready()
    {
        base._Ready();
        SetProcessInput(false);
        SetPhysicsProcess(false);
        OneShot=true;
        Emitting=true;
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            Player.instance.Die();
            QueueFree();
        }
    }

}
