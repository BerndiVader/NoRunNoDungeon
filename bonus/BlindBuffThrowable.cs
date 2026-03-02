using Godot;
using System;

public class BlindBuffThrowable : PhysicsObject
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bonus/BlindBuffThrowable.tscn");

    [Export] private float STRENGTH=3.5f;
    [Export] private int DURATION=60;
    [Export] private Vector2 VELOCITY=Vector2.Zero;

    public static BlindBuffThrowable Create(float strength,int duration,Vector2 initialVelocity)
    {
        BlindBuffThrowable buff=PACK.Instance<BlindBuffThrowable>();
        buff.STRENGTH=strength;
        buff.DURATION=duration;
        buff.VELOCITY=initialVelocity;
        return buff;
    }


    public override void _Ready()
    {
        base._Ready();
        velocity=VELOCITY;
        GetNode<Area2D>(nameof(Area2D)).Connect("body_entered",this,nameof(OnBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
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
        BuffBlind.Create(STRENGTH,DURATION);
    }

}
