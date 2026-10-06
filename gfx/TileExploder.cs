using Godot;
using System;

public class TileExploder : Sprite
{
    private static readonly Shader SHADER=ResourceLoader.Load<Shader>("res://shaders/TileExploder.gdshader");

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        Material=new ShaderMaterial{Shader=SHADER};
        SceneTreeTween tween=GetTree().CreateTween();

        tween.TweenProperty(Material,"shader_param/progress",1f,1f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
        tween.TweenCallback(this,nameof(OnComplete));

    }

    private void OnComplete()
    {
        CallDeferred("queue_free");
    }

}
