using Godot;
using System;

public class TileExploder : Sprite
{
    private Tween tween;
    private static Shader shader=ResourceLoader.Load<Shader>("res://shaders/TileExploder.gdshader");

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        Material=new ShaderMaterial{Shader=shader};
        tween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(OnComplete));
        AddChild(tween);

        tween.InterpolateProperty(
            Material,"shader_param/progress",0f,1f,1f,
            Tween.TransitionType.Sine,Tween.EaseType.InOut
        );
        tween.Start();
    }

    private void OnComplete()
    {
        CallDeferred("queue_free");
    }

}
