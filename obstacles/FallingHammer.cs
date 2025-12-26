using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public class FallingHammer : Area2D,ISwitchable
{
    private static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/8_Atk_Magic_SFX/13_Ice_explosion_01.wav");
    private enum HAMMERSTATE
    {
        IDLE,
        FALLING,
        BACK,
    }
    private enum MODE
    {
        DEFAULT,
        SWITCHABLE,
        DISTANCE
    }

    [Export] MODE mode=MODE.DEFAULT;
    [Export] string switchID="";
    [Export] float activationRange=50f;
    [Export] bool oneTime=false;
    [Export] bool hitMonsters=false;
    [Export] bool destroyables=false;
    [Export] float maxFallSpeed=4f;

    private RayCast2D raycast;
    private Tween tween;
    private float startRotation;
    private float targetRotation=0f;
    private HAMMERSTATE state=HAMMERSTATE.IDLE;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        raycast=GetNode<RayCast2D>(nameof(RayCast2D));
        raycast.AddException(GetNode<StaticBody2D>(nameof(StaticBody2D)));

        Connect("body_entered",this,nameof(BodyEntered));
        startRotation=Rotation;

        tween=new Tween();
        AddChild(tween);

        switch(mode)
        {
            case MODE.DEFAULT:
                Start();
                break;
            case MODE.SWITCHABLE:
                AddToGroup(GROUPS.SWITCHABLES.ToString());
                break;
            case MODE.DISTANCE:
                break;
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        if(mode==MODE.DISTANCE&&state==HAMMERSTATE.IDLE)
        {
            float distance=raycast.GlobalPosition.DistanceTo(Player.instance.GlobalPosition);
            Vector2 direction=raycast.GlobalPosition.DirectionTo(Player.instance.GlobalPosition);

            if(activationRange>distance&&Mathf.Sign(direction.x)==-1&&Mathf.Sign(direction.y)==1)
            {
                Start();
            }
        }
        if(state==HAMMERSTATE.FALLING&&raycast.IsColliding())
        {
            CreateParticles();
            Stop();
            if(destroyables)
            {

                Vector2 point=raycast.GetCollisionPoint();
                Vector2 direction=raycast.GlobalTransform*raycast.CastTo-raycast.GlobalPosition;
                Vector2 testPoint=point+direction.Normalized()*1.0f;

                Physics2DDirectSpaceState space=GetWorld2d().DirectSpaceState;
                var hits=space.IntersectPoint(testPoint,2,null,288,true,true);
                foreach(var obj in hits)
                {
                    if(obj is Dictionary hit)
                    {
                        if(hit["collider"] is Destroyables destroyables)
                        {
                            destroyables.EmitSignal(STATE.damage.ToString(),this,1f);
                        }
                    }
                }
            }
        }
    }

    private void Tweening(float value)
    {
        float delta=value-Rotation;
        float maxStep=maxFallSpeed*GetProcessDeltaTime();

        if(Mathf.Abs(delta)>maxStep)
        {
            Rotation+=Mathf.Sign(delta)*maxStep;
        }
        else
        {
            Rotation=value;
        }
    }

    private void Start()
    {
        state=HAMMERSTATE.FALLING;
        Rotation=startRotation;
        targetRotation=0f;

        tween.InterpolateMethod(
            this,
            nameof(Tweening),
            startRotation,
            -Mathf.Pi*2f,
            1f,
            Tween.TransitionType.Circ,
            Tween.EaseType.In
        );
        tween.Start();
    }

    private void Stop()
    {
        state=HAMMERSTATE.BACK;
        tween.StopAll();
        tween.InterpolateProperty(this,"rotation",Rotation,startRotation,2f,Tween.TransitionType.Quad,Tween.EaseType.Out);
        tween.Connect("tween_all_completed", this, nameof(OnBackComplete));
        tween.Start();
    }

    private void OnBackComplete()
    {
        state=HAMMERSTATE.IDLE;
        tween.Disconnect("tween_all_completed", this, nameof(OnBackComplete));
        if(mode==MODE.DEFAULT) 
        {
            Start();
        }
    }

    private void BodyEntered(Node body)
    {
        if(state==HAMMERSTATE.FALLING&&body.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            Player player=(Player)body;
            int slides=player.GetSlideCount();
            if(slides>0)
            {
                for(int i=0;i<slides;i++)
                {
                    KinematicCollision2D collision=player.GetSlideCollision(i);
                    var collider=collision.Collider;
                    if(collider is Level||collider is Platform)
                    {
                        player.EmitSignal(STATE.damage.ToString(),this,1f);
                    }
                }
            }
        }
    }

    private void CreateParticles()
    {
        MadRockParticles particles=ResourceUtils.particles[(int)PARTICLES.MADROCK].Instance<MadRockParticles>();
        particles.Position=World.level.ToLocal(raycast.GetCollisionPoint());
        World.instance.renderer.PlaySfx(sfx,Position);
        World.instance.renderer.Shake(1f);
        World.level.AddChild(particles);
    }

    public void SwitchCall(string id)
    {
        if(switchID==id&&mode==MODE.SWITCHABLE&&state==HAMMERSTATE.IDLE)
        {
            Start();
        }
    }
   
}
