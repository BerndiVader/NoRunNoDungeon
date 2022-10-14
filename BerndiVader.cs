using Godot;
using System;

public class BerndiVader : Node2D
{
	private Sprite logo;
	private Klammer klammerLinks;
	private Klammer klammerRechts;
	private AudioStreamPlayer2D audio;
	private Tween zoomTween,fadeTween;
	private Vector2 centerPosition;
	private Vector2 leftPosition,rightPosition;
	private Vector2 currentRight,currentLeft;
	private float speed=240f;

	public override void _Ready()
	{
		centerPosition=new Vector2(256f,149f);
		leftPosition=new Vector2(-5f,149f);
		rightPosition=new Vector2(517f,149f);

		currentRight=new Vector2(centerPosition);
		currentLeft=new Vector2(centerPosition);

		logo=GetNode<Sprite>("logo");
		logo.SelfModulate=new Color(0,0,0);
		klammerLinks=GetNode<Klammer>("k1");
		klammerRechts=GetNode<Klammer>("k2");
		audio=GetNode<AudioStreamPlayer2D>("Audio");

		zoomTween=new Tween();
		zoomTween.Connect("tween_all_completed",this,nameof(zoomOutFinished));
		zoomTween.InterpolateMethod(this,nameof(zoomOutTweening),logo.Scale.x,200f,2,Tween.TransitionType.Quad,Tween.EaseType.Out,0.5f);
		AddChild(zoomTween);

		fadeTween=new Tween();
		fadeTween.InterpolateMethod(this,nameof(fadeTweening),logo.Modulate,new Color(0,0,0),2,Tween.TransitionType.Elastic,Tween.EaseType.InOut,0.5f);
		AddChild(fadeTween);

		SetProcess(false);
	}

	public override void _PhysicsProcess(float delta)
	{
		klammerLinks.Position=klammerLinks.Position.LinearInterpolate(currentLeft,delta*3);
		klammerRechts.Position=klammerRechts.Position.LinearInterpolate(currentRight,delta*3);

		if(klammerLinks.Position.x>250&&speed>0&&!audio.Playing)
		{
			audio.Play();
			logo.SelfModulate=new Color(1,1,1);
			currentLeft=leftPosition;
			currentRight=rightPosition;
			fadeTween.Start();
		}

		if(klammerLinks.Position.x<30&&audio.Playing&&!zoomTween.IsActive())
		{
			zoomTween.Start();
		}

	}

	private void zoomOutFinished()
	{
		QueueFree();
		World.changeScene(ResourceUtils.intro);
	}

	private void zoomOutTweening(float delta)
	{
		logo.Scale=new Vector2(delta,1);
	}

	private void fadeTweening(Color delta)
	{
		logo.Modulate=delta;
	}
}
