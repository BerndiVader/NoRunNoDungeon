using Godot;
using System;
using System.Runtime.InteropServices;

public class Saw : Area2D
{
    private static readonly AudioStream SAW_FX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/saw.wav");
    private static readonly AudioStream HIT_FX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/17_orc_atk_sword_2.wav");

    private enum DIRECTION
    {
        LEFT,
        RIGHT,
    }

    [Export] private DIRECTION direction=DIRECTION.RIGHT;
    [Export] private int LENGTH=2;
    [Export] private float SPEED=120f;

    private Vector2 startPosition;
    private float length;
    private float lastX;
    private bool wasVisible=false;

    private AnimatedSprite animation;
    private CPUParticles2D partLeft,partRight;
    private Tween tween;
    private AudioStreamPlayer2D sfxPlayer;

    public override void _Ready()
    {

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(OnScreenExited));
        notifier2D.Connect("screen_entered",this,nameof(OnScreenEntered));
        AddChild(notifier2D);        

        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        partRight=GetNode<CPUParticles2D>(nameof(CPUParticles2D));
        partLeft=GetNode<CPUParticles2D>("CPUParticles2D2");
        sfxPlayer=GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));

        Connect("body_entered",this,nameof(BodyEntered));
        Connect("body_exited",this,nameof(BodyExited));

        AddUserSignal(STATE.damage.ToString());
        Connect(STATE.damage.ToString(),this,nameof(OnDamage));

        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_completed",this,nameof(OnTweenCompleted));
        startPosition=Position;
        length=LENGTH*16f;

        sfxPlayer.Connect("finished",this,nameof(SfxEnd));
        sfxPlayer.Stream=SAW_FX;
        sfxPlayer.MaxDistance=ResourceUtils.MAX_SFX_DISTANCE;
        sfxPlayer.Play();

        animation.Playing=true;
        StartTween();
    }

    private void StartTween() 
    {
        SetActiveParticles();
        animation.Play(direction.ToString());

        float from=Position.x;
        float to=direction==DIRECTION.RIGHT?startPosition.x+length:startPosition.x-length;
        float duration=Mathf.Abs(from-to)/SPEED;
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

        sfxPlayer.VolumeDb=Mathf.Lerp(-10f,10f,Mathf.Clamp(factor,0f,1f));
        animation.SpeedScale=8f*factor;

        CPUParticles2D particles=ActiveParticles();
        if(particles.Emitting)
        {
            particles.SpeedScale=1.2f*factor;
            particles.Lifetime=MathUtils.MinMax(0.01f,0.35f,0.35f*factor);
            particles.OrbitVelocity=direction==DIRECTION.LEFT?-2f*factor:2f*factor;
        }

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
        return partLeft.Emitting?partLeft:partRight;
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

    private void OnDamage(Node damager,float amount)
    {
        SfxPlayer sfx=new SfxPlayer
        {
            Stream=HIT_FX,
            Position=Position
        };
        World.level.AddChild(sfx);
    }

    private void OnScreenEntered()
    {
        wasVisible=true;
    }

    private void OnScreenExited()
    {
        if(wasVisible)
        {
            World.OnObjectExitedScreen(this);
        }
    }

}
