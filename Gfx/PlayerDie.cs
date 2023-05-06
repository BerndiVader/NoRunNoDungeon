using Godot;
using System;

public class PlayerDie : Sprite
{

    static PackedScene scene=ResourceLoader.Load<PackedScene>("res://gfx/PlayerDie.tscn");

    public static PlayerDie create()
    {
        return scene.Instance<PlayerDie>();
    }

    float speed;
    ShaderMaterial shader;

    public override void _Ready()
    {
        speed=0f;
        shader=(ShaderMaterial)Material;
    }

    public override void _Process(float delta)
    {
        shader.SetShaderParam("offset",speed);
        speed+=0.03f;
        speed=Mathf.Min(speed,1f);

        if(speed>=1f)
        {
            QueueFree();
        }
    }

}
