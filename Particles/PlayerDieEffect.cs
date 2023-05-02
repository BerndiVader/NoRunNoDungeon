using Godot;
using System;

public class PlayerDieEffect : CPUParticles2D
{
    public static void create()
    {
        PlayerDieEffect effect=ResourceUtils.particles[(int)PARTICLES.PLAYERDIE].Instance<PlayerDieEffect>();
        effect.ScaleAmount*=PlayerCamera.instance.Scale.x;
        effect.Position=PlayerCamera.instance.GetCameraScreenCenter();
        World.instance.renderer.AddChild(effect);
    }

    public PlayerDieEffect():base() {}

    public override void _Ready()
    {
        SetProcessInput(false);
        SetPhysicsProcess(false);
        OneShot=true;
        Emitting=true;
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
            Player.instance.die();
        }
    }

}
