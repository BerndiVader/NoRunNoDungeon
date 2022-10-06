using Godot;
using System;

public class IntroMap : TileMap
{
    float Speed=7f;
    bool reverse=true;
    Tween tween;
    Tween colorTween;
    Color color=new Color(0,0,0,0);

    Vector2 Movement=Vector2.Right*2080f;


    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(color);

        tween=new Tween();
        colorTween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(onFinishedTween));
        colorTween.Connect("tween_all_completed",this,nameof(onFinishedColorTween));
        AddChild(tween);
        AddChild(colorTween);
        tweenIn();
        colorTweenIn();
        tween.Start();
        colorTween.Start();
    }

    void tweenIn()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-Movement,Speed,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }

    void tweenOut()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-Movement,Speed,Tween.TransitionType.Sine,Tween.EaseType.Out,0);
    }

    void colorTweenIn()
    {
        colorTween.InterpolateMethod(this,nameof(colorTweening),color,new Color((float)MathUtils.randomRange(0,1),(float)MathUtils.randomRange(0,1),(float)MathUtils.randomRange(0,1),1),2,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }
    void colorTweenOut()
    {
        colorTween.InterpolateMethod(this,nameof(colorTweening),color,new Color(0,0,0,1),2,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }


    void tweening(Vector2 delta)
    {
        Position=delta;
    }

    void colorTweening(Color delta)
    {
        color=delta;
        VisualServer.SetDefaultClearColor(color);
    }

    void onFinishedTween()
    {
        Position=Vector2.Zero;
        tweenOut();
        tween.Start();
    }

    void onFinishedColorTween()
    {
        reverse=!reverse;
        if(reverse) 
        {
            colorTweenIn();
        } 
        else 
        {
            colorTweenOut();
        }
        colorTween.Start();
    }

}
