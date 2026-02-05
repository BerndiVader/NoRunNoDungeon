using Godot;
using System;

public class IntroMap : TileMap
{
    private const float SPEED=4f;
    
    private bool reverse=true;
    private Tween tween;
    private Tween colorTween;
    private Color color=new Color(0,0,0,0);
    private Color shaderColor=new Color(1f,0.74902f,0f,1f);
    private Vector2 Movement=Vector2.Right*2080f;
    private Intro intro;
	private ShaderMaterial speedTrailsTop,speedTrailsBottom;


    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(color);

        intro=GetParent<Intro>();
		speedTrailsTop=(ShaderMaterial)intro.GetNode<Sprite>("SpeedTrailsTop").Material;
		speedTrailsBottom=(ShaderMaterial)intro.GetNode<Sprite>("SpeedTrailsBottom").Material;        

        tween=new Tween();
        colorTween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(OnFinishedTween));
        colorTween.Connect("tween_all_completed",this,nameof(OnFinishedColorTween));
        AddChild(tween);
        AddChild(colorTween);
        TweenIn();
        ColorTweenIn();
        tween.Start();
        colorTween.Start();
    }

    private void TweenIn()
    {
        tween.InterpolateMethod(this,nameof(Tweening),Position,Position-Movement,SPEED,Tween.TransitionType.Sine,Tween.EaseType.InOut,0);
    }

    private void TweenOut()
    {
        tween.InterpolateMethod(this,nameof(Tweening),Position,Position-Movement,SPEED,Tween.TransitionType.Sine,Tween.EaseType.Out,0);
    }

    private void ColorTweenIn()
    {
        colorTween.InterpolateMethod(this,nameof(ColorTweening),color,new Color((float)MathUtils.RandomRange(0,1),(float)MathUtils.RandomRange(0d,1d),(float)MathUtils.RandomRange(0d,1d),1f),2f,Tween.TransitionType.Sine,Tween.EaseType.InOut,0f);
    }
    private void ColorTweenOut()
    {
        colorTween.InterpolateMethod(this,nameof(ColorTweening),color,new Color(0f,0f,0f,1f),2f,Tween.TransitionType.Sine,Tween.EaseType.InOut,0f);
    }


    private void Tweening(Vector2 delta)
    {
        float alpha=Mathf.InverseLerp(1f,0f,1f/(1f+(Position.x-delta.x)));
        shaderColor.a=alpha;

        speedTrailsTop.SetShaderParam("color_b",shaderColor);
        speedTrailsBottom.SetShaderParam("color_b",shaderColor);
        
        Position=delta;
    }

    private void ColorTweening(Color delta)
    {
        color=delta;
        VisualServer.SetDefaultClearColor(color);
    }

    private void OnFinishedTween()
    {
        Position=Vector2.Zero;
        TweenOut();
        tween.Start();
    }

    private void OnFinishedColorTween()
    {
        reverse=!reverse;
        if(reverse) 
        {
            ColorTweenIn();
        } 
        else 
        {
            ColorTweenOut();
        }
        colorTween.Start();
    }

}
