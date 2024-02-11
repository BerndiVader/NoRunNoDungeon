using Godot;
using System;

public class IntroMap : TileMap
{
    private float Speed=7f;
    private bool reverse=true;
    private Tween tween;
    private Tween colorTween;
    private Color color=new Color(0,0,0,0);
    private Vector2 Movement=Vector2.Right*2080f;
    private Intro intro;

    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(color);

        intro=GetParent<Intro>();

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

    private void tweenIn()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-Movement,Speed,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }

    private void tweenOut()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-Movement,Speed,Tween.TransitionType.Sine,Tween.EaseType.Out,0);
    }

    private void colorTweenIn()
    {
        colorTween.InterpolateMethod(this,nameof(colorTweening),color,new Color((float)MathUtils.randomRange(0,1),(float)MathUtils.randomRange(0,1),(float)MathUtils.randomRange(0,1),1),2,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }
    private void colorTweenOut()
    {
        colorTween.InterpolateMethod(this,nameof(colorTweening),color,new Color(0,0,0,1),2,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }


    private void tweening(Vector2 delta)
    {
        float speed=Position.x-delta.x;
        speed=Mathf.Round(200-speed*20);
        
        ((ShaderMaterial)intro.speedTrailsTop.Material).SetShaderParam("Speed",speed);
        ((ShaderMaterial)intro.speedTrailsBottom.Material).SetShaderParam("Speed",speed);
        Position=delta;
    }

    private void colorTweening(Color delta)
    {
        color=delta;
        VisualServer.SetDefaultClearColor(color);
    }

    private void onFinishedTween()
    {
        Position=Vector2.Zero;
        tweenOut();
        tween.Start();
    }

    private void onFinishedColorTween()
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
