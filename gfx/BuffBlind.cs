using System;
using Godot;

public class BuffBlind : Sprite
{
    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://gfx/BuffBlind.tscn");

    private Vector2 scale=Vector2.One;
    private float DURATION;
    private float duration;
    private WeakReference<Node>weakRef;

    ShaderMaterial mat;

    public static void Create(Vector2 scale,float duration)
    {
        if(scale==null)
        {
            scale=Vector2.One;
        }

        BuffBlind buff=pack.Instance<BuffBlind>();
        buff.scale=scale;
        buff.DURATION=duration;
        HUD.instance.AddChild(buff);
        buff.weakRef=new WeakReference<Node>(buff);
        buff.mat=(ShaderMaterial)buff.Material;
        Player.buffs.Add(buff.weakRef);

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
            mat.SetShaderParam("mask_radius",138.5f*2);
            mat.SetShaderParam("mask_pos",screen_pos);
        }
    }

}
