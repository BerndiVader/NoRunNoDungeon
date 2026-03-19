using Godot;
using System;

public class JumpBuff : Buff
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/JumpBuff.tscn");

    protected float DURATION;
    private float duration;
    protected float size;

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
            Player.instance.JumpModifier-=size;
            QueueFree();
        }        
    }


    public override void Apply()
    {
        HUD.instance.AddChild(this);
        Player.buffs.Add(weakRef);
        Player.instance.JumpModifier+=size;        
    }

    public override void Replace(Buff buff)
    {
        Apply();
    }

}
