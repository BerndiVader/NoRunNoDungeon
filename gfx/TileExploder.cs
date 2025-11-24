using Godot;
using System;

public class TileExploder : Sprite
{
    private Tween tween;
    private static Shader shader=ResourceLoader.Load<Shader>("res://shaders/TileExploder.gdshader");

    public override void _Ready()
    {
        Material=new ShaderMaterial{Shader=shader};
        tween=new Tween();
        AddChild(tween);

        tween.InterpolateProperty(
            Material,"shader_param/progress",0f,1f,1f,
            Tween.TransitionType.Sine,Tween.EaseType.InOut
        );
        tween.Start();
    }

    public override void _Process(float delta)
    {
        if(!tween.IsActive())
        {
            CallDeferred("queue_free");
        }
    }


}
