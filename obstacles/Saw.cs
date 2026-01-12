using Godot;
using System;

public class Saw : Area2D
{

    private static readonly AudioStream sawFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/saw.wav");

    private enum DIRECTION
    {
        LEFT,
        RIGHT,
    }

    [Export] private DIRECTION direction=DIRECTION.RIGHT;
    [Export] private int Length=2;
    [Export] private float Speed=120f;

    private AnimatedSprite animation;
    private CPUParticles2D partLeft,partRight;
    private Tween tween;
    private AudioStreamPlayer2D sfxPlayer;
    private Vector2 startPosition;
    private float length;
    private float lastX;

    public override void _Ready()
    {

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);        

        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        partRight=GetNode<CPUParticles2D>(nameof(CPUParticles2D));
        partLeft=GetNode<CPUParticles2D>("CPUParticles2D2");
        sfxPlayer=GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));

        Connect("body_entered",this,nameof(BodyEntered));
        Connect("body_exited",this,nameof(BodyExited));

        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_completed",this,nameof(OnTweenCompleted));
        startPosition=Position;
        length=Length*16f;

        sfxPlayer.Connect("finished",this,nameof(SfxEnd));
        sfxPlayer.Stream=sawFx;
        sfxPlayer.Play();

        CPUParticles2D particles=ActiveParticles();
        animation.Playing=true;
        StartTween();
    }

    private void StartTween() 
    {
        SetActiveParticles();
        animation.Play(direction.ToString());

        float from=Position.x;
        float to=direction==DIRECTION.RIGHT?startPosition.x+length:startPosition.x-length;
        float duration=Mathf.Abs(from-to)/Speed;
        tween.InterpolateMethod(
            this,
            nameof(Tweening),
            from,
            to,
            duration,
            Tween.TransitionType.Sine,
            Tween.EaseType.InOut
        );
        tween.Start();
    }

    private void OnTweenCompleted(object obj,NodePath path)
    {
        direction=direction==DIRECTION.RIGHT?DIRECTION.LEFT:DIRECTION.RIGHT;
        StartTween();
    }

    private void Tweening(float x) 
    {
        float delta=GetPhysicsProcessDeltaTime();
        float velocity=delta>0f?Mathf.Abs(x-lastX)/delta:0f;
        float factor=velocity/120f;

        sfxPlayer.VolumeDb=Mathf.Lerp(-20f,0f,Mathf.Clamp(factor,0f,1f));
        CPUParticles2D particles=ActiveParticles();

        animation.SpeedScale=8f*factor;
        particles.SpeedScale=1.2f*factor;
        particles.Lifetime=MathUtils.MinMax(0.01f,0.35f,0.35f*factor);
        particles.OrbitVelocity=direction==DIRECTION.LEFT?-2f*factor:2f*factor;

        Position=new Vector2(x,Position.y);
        lastX=x;
    }

    private void SetActiveParticles()
    {
        partLeft.Emitting=direction==DIRECTION.LEFT;
        partRight.Emitting=direction==DIRECTION.RIGHT;

        switch(direction)
        {
            case DIRECTION.LEFT:
                partRight.Lifetime=0.01f;
                partRight.SpeedScale=1f;                
                break;
            case DIRECTION.RIGHT:
                partLeft.Lifetime=0.01f;
                partLeft.SpeedScale=1f;            
                break;
        }

    }    

    private CPUParticles2D ActiveParticles()
    {
        return direction==DIRECTION.LEFT?partLeft:partRight;
    }

    private void BodyEntered(Node node)
    {
        if(node is Player)
        {
            SetDeferred("monitoring",false);
            node.EmitSignal(STATE.damage.ToString(),this,1f);
        }
    }

    private void BodyExited(Node node)
    {
        if(node is Player)
        {
            SetDeferred("monitoring",true);
        }
    } 

    private void SfxEnd()
    {
        sfxPlayer.Play();
    }

}
