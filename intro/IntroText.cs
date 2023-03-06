using Godot;
using System;

public class IntroText : Godot.RichTextLabel
{
    private Tween tween;
    private Vector2 position;
    private float diff;

    public override void _Ready()
    {

        position=new Vector2(RectPosition);
        diff=-20;

        tween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(onTweenComplete));
        tween.InterpolateMethod(this,nameof(tweening),RectPosition,new Vector2(position.x,position.y+diff),1,Tween.TransitionType.Back,Tween.EaseType.InOut,0);

        AddChild(tween);
        tween.Start();

    }

    public override void _Process(float delta)
    {
        
    }

    private void tweening(Vector2 delta)
    {
        RectPosition=delta;
    }

    private void onTweenComplete()
    {
        diff=diff*-1;
        tween.InterpolateMethod(this,nameof(tweening),RectPosition,new Vector2(position.x,position.y+diff),1,Tween.TransitionType.Back,Tween.EaseType.InOut,0);
        tween.Start();
    }


}
