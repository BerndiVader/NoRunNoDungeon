using Godot;
using System;

public class RichTextLabel : Godot.RichTextLabel
{
    Tween tween;
    Vector2 position;
    float diff;

    public override void _Ready()
    {

        position=new Vector2(RectPosition);
        diff=-20;

        tween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(tweenComplete));
        tween.InterpolateMethod(this,nameof(tweening),RectPosition,new Vector2(position.x,position.y+diff),1,Tween.TransitionType.Back,Tween.EaseType.InOut,0);

        AddChild(tween);
        tween.Start();

    }

    public override void _Process(float delta)
    {
        
    }

    void tweening(Vector2 delta)
    {
        RectPosition=delta;
    }

    void tweenComplete()
    {
        diff=diff*-1;
        tween.InterpolateMethod(this,nameof(tweening),RectPosition,new Vector2(position.x,position.y+diff),1,Tween.TransitionType.Back,Tween.EaseType.InOut,0);
        tween.Start();
    }


}
