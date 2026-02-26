using System;
using Godot;

public class BuffBlind : Sprite
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://buffs/BuffBlind.tscn");

    private float DURATION;
    private float duration;
    private float size;
    private WeakReference<Node>weakRef;

    ShaderMaterial mat;

    public static void Create(float size=3f,float duration=120f)
    {
        BuffBlind buff=pack.Instance<BuffBlind>();
        buff.size=size;
        buff.DURATION=duration;
        buff.mat=(ShaderMaterial)buff.Material;
        buff.weakRef=new WeakReference<Node>(buff);

        HUD.instance.AddChild(buff);
        Player.buffs.Add(buff.weakRef);
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

}
