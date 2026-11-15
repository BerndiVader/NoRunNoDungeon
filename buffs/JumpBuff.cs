using Godot;
using System;

public class JumpBuff : Buff
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/JumpBuff.tscn");

    protected float DURATION;
    private float duration;
    protected float size;
    private float applied;

    public static void Create(float size=0.25f,float duration=120f)
    {
        JumpBuff buff=pack.Instance<JumpBuff>();
        buff.size=size;
        buff.DURATION=duration;
        buff.weakRef=new WeakReference<Buff>(buff);
        buff.Apply();
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
            Player.instance.JumpModifier=MathUtils.MinMax(0f,2f,Player.instance.JumpModifier-applied);
            SetPhysicsProcess(false);
            QueueFree();
        }        
    }


    public override void Apply()
    {
        HUD.instance.AddChild(this);
        Player.buffs.Add(weakRef);
        float current=Player.instance.JumpModifier;
        Player.instance.JumpModifier=MathUtils.MinMax(0f,2f,current+size);
        applied=Player.instance.JumpModifier-current;
    }

    public override void Replace(Buff buff)
    {
        Apply();
    }

}
