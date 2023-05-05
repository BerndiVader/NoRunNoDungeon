using Godot;
using System;

public class PlayerDie : Sprite
{

    static PackedScene scene=ResourceLoader.Load<PackedScene>("res://shaders/PlayerDie.tscn");

    public static PlayerDie create()
    {
        return scene.Instance<PlayerDie>();
    }

    float speed;

    public override void _Ready()
    {
        speed=0f;
    }

    public override void _Process(float delta)
    {
        ((ShaderMaterial)Material).SetShaderParam("offset",speed);
        speed+=0.03f;
        speed=Mathf.Min(speed,1f);

        float offset=(float)((ShaderMaterial)Material).GetShaderParam("offset");
        if(offset>=1f)
        {
            QueueFree();
        }
    }

}
