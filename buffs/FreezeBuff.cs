using Godot;
using System;

public class FreezeBuff : Buff
{
    public const int DENY_SPEED_BIT=1<<0;
    public const int DENY_JUMP_BIT=1<<1;
    public const int DENY_DASH_BIT=1<<2;

    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/FreezeBuff.tscn");

    protected float DURATION;
    private float duration;
    private float appliedSpeed,appliedJump;
    private bool prevDash;

    private bool jump,speed,dash;

    public static void Create(int mask=DENY_SPEED_BIT|DENY_JUMP_BIT|DENY_DASH_BIT,float duration=120f)
    {
        FreezeBuff buff=pack.Instance<FreezeBuff>();

        buff.speed=(mask&DENY_SPEED_BIT)!=0;
        buff.jump=(mask&DENY_JUMP_BIT)!=0;
        buff.dash=(mask&DENY_DASH_BIT)!=0;
        buff.DURATION=duration;
        buff.weakRef=new WeakReference<Buff>(buff);

        Buff target=Player.instance.FindBuff(buff);
        if(IsInstanceValid(target)&&target.IsInsideTree())
        {
            buff.Replace(target);
        }
        else
        {
            buff.Apply();
        }

    }    

    public override void _Ready()
    {
        duration=0f;
        SetProcess(false);
        SetProcessInput(false);
        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(float delta)
    {
        duration+=delta;
        if(duration>=DURATION)
        {
            Player.buffs.Remove(weakRef);
            Reset();
            SetPhysicsProcess(false);
            QueueFree();
        }
    }

    public void Reset()
    {
        if(speed)
        {
            Player.instance.SpeedModifier=MathUtils.MinMax(0f,2f,Player.instance.SpeedModifier-appliedSpeed);
        }
        if(jump)
        {
            Player.instance.JumpModifier=MathUtils.MinMax(0f,2f,Player.instance.JumpModifier-appliedJump);
        }
        if(dash)
        {
            Player.instance.DashDenied=prevDash;
        }
    }

    public override void Apply()
    {
        HUD.instance.AddChild(this);
        Player.buffs.Add(weakRef);
        if(speed)
        {
            float cur=Player.instance.SpeedModifier;
            Player.instance.SpeedModifier=MathUtils.MinMax(0f,2f,cur-9999f);
            appliedSpeed=Player.instance.SpeedModifier-cur;
        }
        if(jump)
        {
            float cur=Player.instance.JumpModifier;
            Player.instance.JumpModifier=MathUtils.MinMax(0f,2f,cur-9999f);
            appliedJump=Player.instance.JumpModifier-cur;
        }
        if(dash)
        {
            prevDash=Player.instance.DashDenied;
            Player.instance.DashDenied=true;
        }
    }

    public override void Replace(Buff buff)
    {
        if(buff is FreezeBuff freeze)
        {
            if(IsInstanceValid(freeze))
            {
                freeze.Reset();
                Player.instance.RemoveBuff(freeze.weakRef);
                freeze.SetPhysicsProcess(false);
                freeze.QueueFree();
            }
        }
        Apply();
    }

}
