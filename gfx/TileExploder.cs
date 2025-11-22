using Godot;
using System;

public class TileExploder : Sprite
{
    Tween tween;

    public override void _Ready()
    {
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
