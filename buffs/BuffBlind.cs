using System;
using Godot;

public class BuffBlind : Buff
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/BuffBlind.tscn");

    protected float DURATION;
    private float duration;
    protected float size;

    ShaderMaterial mat;

    public static void Create(float size=3f,float duration=120f)
    {
        BuffBlind buff=pack.Instance<BuffBlind>();
        buff.size=size;
        buff.DURATION=duration;
        buff.mat=(ShaderMaterial)buff.Material;
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

        mat.SetShaderParam("mask_radius",138.5f*size);
    }

    public override void _PhysicsProcess(float delta)
    {
        duration+=delta;
        if(duration>=DURATION)
        {
            Player.buffs.Remove(weakRef);
            QueueFree();
        }
        else
        {
            Vector2 screenPos;
            if(size>=2.5f)
            {
                screenPos=World.RESOLUTION*0.5f;
            }
            else
            {
                screenPos=Player.instance.GlobalPosition;
                if(PlayerCamera.instance.Zoom!=Vector2.One)
                {
                    Vector2 scrCenter=World.RESOLUTION*0.5f;
                    screenPos=(screenPos-PlayerCamera.instance.GetCameraScreenCenter())/PlayerCamera.instance.Zoom+scrCenter;
                }
            }
            mat.SetShaderParam("mask_pos",screenPos);
            mat.SetShaderParam("time",OS.GetTicksMsec()/1000.0f);
        }
    }

    public override void Apply()
    {
        HUD.instance.AddChild(this);
        Player.buffs.Add(weakRef);
    }

    public override void Replace(Buff buff)
    {
        if(buff is BuffBlind blind)
        {
            if(IsInstanceValid(blind))
            {
                Player.instance.RemoveBuff(blind.weakRef);
                blind.QueueFree();
            }
        }
        Apply();
    }
}
