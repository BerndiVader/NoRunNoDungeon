using Godot;
using System;

public class JumpBuffThrowable : BuffThrowable
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bonus/JumpBuffThrowable.tscn");

    [Export] private float STRENGTH=0.25f;
    [Export] private int DURATION=20;

    private float lifetime;
    private bool useLifetime;

    public static JumpBuffThrowable Create(float strength,int duration,Vector2 initialVelocity,float lifetime=-1f)
    {
        JumpBuffThrowable buff=PACK.Instance<JumpBuffThrowable>();
        buff.STRENGTH=strength;
        buff.DURATION=duration;
        buff.VELOCITY=initialVelocity;
        buff.lifetime=lifetime;
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
        JumpBuff.Create(STRENGTH,DURATION);
    }    

}
