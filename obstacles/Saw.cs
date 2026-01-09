using Godot;
using System;

public class Saw : Area2D
{
    private enum DIRECTION
    {
        LEFT,
        RIGHT,
    }

    [Export] private DIRECTION direction=DIRECTION.RIGHT;
    [Export] private int Length=2;

    private AnimatedSprite animation;
    private CPUParticles2D particles;
    private Tween tween;
    private Vector2 startPosition;
    private float length;

    public override void _Ready()
    {
        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        particles=GetNode<CPUParticles2D>(nameof(CPUParticles2D));

        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_completed",this,nameof(OnTweenCompleted));
        startPosition=Position;
        length=Length*16f;
        StartTween();
    }

    private void StartTween() 
    {
        float targetX=direction==DIRECTION.RIGHT?startPosition.x+length:startPosition.x-length;
        tween.InterpolateProperty(this,"position:x", Position.x,targetX,1.0f,Tween.TransitionType.Sine,Tween.EaseType.InOut);
        tween.Start();
    }

    private void OnTweenCompleted(object obj,NodePath path)
    {
        direction=direction==DIRECTION.RIGHT?DIRECTION.LEFT:DIRECTION.RIGHT;
        UpdateDirection();
        StartTween();
    }

    private void UpdateDirection()
    {
        switch(direction) 
        {
            case DIRECTION.LEFT:
                particles.OrbitVelocity=-2f;
                break;
            case DIRECTION.RIGHT:
                particles.OrbitVelocity=2f;
                break;
        }
        animation.Play(direction.ToString());
    }


}
