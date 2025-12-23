using Godot;
using System;

public class PlayerDie : Sprite
{
    private static readonly PackedScene scene=ResourceLoader.Load<PackedScene>("res://gfx/PlayerDie.tscn");

    public static PlayerDie Create()
    {
        return scene.Instance<PlayerDie>();
    }

    float offset;
    ShaderMaterial shader;
    public bool flip;

    public override void _Ready()
    {
        offset=0f;
        shader=(ShaderMaterial)Material;
        shader.SetShaderParam("flip",flip);
    }

    public override void _PhysicsProcess(float delta)
    {
        shader.SetShaderParam("offset",offset);
        offset=Mathf.Min(offset+0.03f,1f);

        if(offset==1f)
        {
            QueueFree();
            SetPhysicsProcess(false);
        }
    }

}
