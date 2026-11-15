using Godot;
using System;

public class FreezeBuffThrowable : BuffThrowable
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bonus/FreezeBuffThrowable.tscn");

    [Export] private int DURATION=60;
    [Export(PropertyHint.Flags,"Speed,Jump,Dash")] private int mask=FreezeBuff.DENY_SPEED_BIT|FreezeBuff.DENY_JUMP_BIT|FreezeBuff.DENY_DASH_BIT;
    private float lifetime;
    private bool useLifetime;

    public static FreezeBuffThrowable Create(float strength,int duration,int mask,Vector2 initialVelocity,float lifetime=-1f)
    {
        FreezeBuffThrowable buff=PACK.Instance<FreezeBuffThrowable>();
        buff.DURATION=duration;
        buff.VELOCITY=initialVelocity;
        buff.lifetime=lifetime;
        buff.mask=mask;
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
        FreezeBuff.Create(mask,DURATION);
    }

}
