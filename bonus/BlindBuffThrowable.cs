using Godot;
using System;

public class BlindBuffThrowable : BuffThrowable
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bonus/BlindBuffThrowable.tscn");

    [Export] private float STRENGTH=1f;
    [Export] private int DURATION=60;
    [Export] private float DARKNESS=1f;

    private float BASE_STRENGTH=3.5f;
    private float lifetime;
    private bool useLifetime;

    public static BlindBuffThrowable Create(float strength,int duration,float darkness,Vector2 initialVelocity,float lifetime=-1f)
    {
        BlindBuffThrowable buff=PACK.Instance<BlindBuffThrowable>();
        buff.STRENGTH=MathUtils.MinMax(0f,1f,strength);
        buff.DURATION=duration;
        buff.VELOCITY=initialVelocity;
        buff.lifetime=lifetime;
        buff.DARKNESS=darkness;
        buff.useLifetime=lifetime>-1f;
        return buff;
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

    public override void Apply()
    {
        BuffBlind.Create(BASE_STRENGTH*STRENGTH,DARKNESS,DURATION);
    }

}
