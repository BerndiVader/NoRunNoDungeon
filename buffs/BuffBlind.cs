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
        if(target!=null&&target.IsInsideTree())
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
            Vector2 screen_pos=Player.instance.GlobalPosition;
            if(PlayerCamera.instance.Zoom!=Vector2.One)
            {
                Vector2 cam_center=PlayerCamera.instance.GetCameraScreenCenter();
                Vector2 viewport_size=World.RESOLUTION;
                screen_pos=(screen_pos-cam_center)/PlayerCamera.instance.Zoom+(viewport_size/2);
            }
            mat.SetShaderParam("mask_pos",screen_pos);
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
