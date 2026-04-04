using Godot;
using System;

public class SpeedBuffThrowable : BuffThrowable
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bonus/SpeedBuffThrowable.tscn");

    [Export] private float STRENGTH=0.25f;
    [Export] private int DURATION=20;
    [Export] private Vector2 VELOCITY=Vector2.Zero;

    private float lifetime;
    private bool useLifetime;

    public static SpeedBuffThrowable Create(float strength,int duration,Vector2 initialVelocity,float lifetime=-1f)
    {
        SpeedBuffThrowable buff=PACK.Instance<SpeedBuffThrowable>();
        buff.STRENGTH=strength;
        buff.DURATION=duration;
        buff.VELOCITY=initialVelocity;
        buff.lifetime=lifetime;
        buff.useLifetime=lifetime>-1f;
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
        if(useLifetime)
        {
            lifetime-=delta;
            if(lifetime<=0)
            {
                QueueFree();
            }
        }
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
        SpeedBuff.Create(STRENGTH,DURATION);
    }

}
