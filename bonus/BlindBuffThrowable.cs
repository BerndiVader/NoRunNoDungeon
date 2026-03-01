using Godot;
using System;

public class BlindBuffThrowable : PhysicsObject
{
    [Export] private float strength=3.5f;
    [Export] private int duration=60;

    public static BlindBuffThrowable Create(float strength,int duration)
    {
        return null;
    }


    public override void _Ready()
    {
        base._Ready();

        GetNode<Area2D>(nameof(Area2D)).Connect("body_entered",this,nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node body) 
    {
        if(body is Player)
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            particles.audio.Stream=CoinTakenParticles.sfxSmall;
            World.level.AddChild(particles);
            CallDeferred("queue_free");
            Apply();
        }
    }

    public void Apply()
    {
        BuffBlind.Create(strength,duration);
    }

}
