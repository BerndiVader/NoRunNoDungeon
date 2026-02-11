using Godot;
using System;

public class PlayerDieEffect : CPUParticles2D
{
    private static readonly AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/Negative/Retro Negative Short 23.wav");

    public static void Create()
    {
        PlayerDieEffect effect=ResourceUtils.particles[(int)PARTICLES.PLAYERDIE].Instance<PlayerDieEffect>();
        effect.ScaleAmount*=PlayerCamera.instance.Scale.x;
        effect.Position=PlayerCamera.instance.GetCameraScreenCenter();

        Renderer.instance.AddChild(effect);
    }

    public override void _Ready()
    {
        SetProcessInput(false);
        SetProcess(false);

        Renderer.instance.PlaySfx(sfx,Position);

        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!Emitting)
        {
            Player.instance.Die();
            SetPhysicsProcess(false);
            QueueFree();
        }
    }

}
