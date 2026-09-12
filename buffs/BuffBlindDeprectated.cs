using System;
using Godot;

public class BuffBlindDeprecated : Buff
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/BuffBlindDeprecated.tscn");

    protected float DURATION;
    private float duration;
    protected float size;
    protected float darkness;

    ShaderMaterial shader;

    public static void Create(float size=3f,float darkness=1f,float duration=120f)
    {
        BuffBlindDeprecated buff=pack.Instance<BuffBlindDeprecated>();
        buff.size=size;
        buff.DURATION=duration;
        buff.darkness=darkness;
        buff.shader=(ShaderMaterial)buff.Material;
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

        shader.SetShaderParam("mask_radius",138.5f*size);
        shader.SetShaderParam("dizzy_strength",8.0f*size);
        shader.SetShaderParam("dizzy_speed",2.0f*size);
        shader.SetShaderParam("darkness",0.95f*darkness);
        shader.SetShaderParam("mask_pos",World.RESOLUTION*0.5f);

    }

    public override void _PhysicsProcess(float delta)
    {
        duration+=delta;
        if(duration>=DURATION)
        {
            Player.buffs.Remove(weakRef);
            QueueFree();
        }
        else if(size<2.5f)
        {
            Vector2 pos=Player.instance.GlobalPosition;
            if(PlayerCamera.instance.Zoom!=Vector2.One)
            {
                Vector2 center=World.RESOLUTION*0.5f;
                pos=(pos-PlayerCamera.instance.GetCameraScreenCenter())/PlayerCamera.instance.Zoom+center;
            }
            shader.SetShaderParam("mask_pos",pos);
        }
    }

    public override void Apply()
    {
        HUD.instance.AddChild(this);
        Player.buffs.Add(weakRef);
    }

    public override void Replace(Buff buff)
    {
        if(buff is BuffBlindDeprecated blind)
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
