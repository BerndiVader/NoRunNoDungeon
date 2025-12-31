using Godot;
using Godot.Collections;

public class FallingHammer : Area2D,ISwitchable
{
    private static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/8_Atk_Magic_SFX/13_Ice_explosion_01.wav");
    private enum HAMMERSTATE
    {
        IDLE,
        FALLING,
        BACK,
        PLAYEDONCE
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
    [Export] bool fallOnly=false;
    [Export] bool hitMonsters=false;
    [Export] bool destroyables=false;

    private RayCast2D raycast;
    private Tween tween;
    private float oRotation;
    private bool playedOnce=false;
    private HAMMERSTATE state=HAMMERSTATE.IDLE;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        raycast=GetNode<RayCast2D>(nameof(RayCast2D));
        raycast.AddException(GetNode<StaticBody2D>(nameof(StaticBody2D)));

        Connect("body_entered",this,nameof(BodyEntered));
        oRotation=Rotation;

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
            StopFall();
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
        switch(state)
        {
            case HAMMERSTATE.FALLING:
                Rotation=value;
                break;
            case HAMMERSTATE.BACK:
                Rotation=value;
                break;
        }
    }

    private void Start()
    {
        if(oneTime&&playedOnce)
        {
            return;
        }

        tween.StopAll();        
        playedOnce=true;
        state=HAMMERSTATE.FALLING;

        tween.InterpolateMethod(
            this,
            nameof(Tweening),
            Rotation,
            -Mathf.Pi*2f,
            1f,
            Tween.TransitionType.Circ,
            Tween.EaseType.In
        );

        tween.Connect("tween_all_completed",this,nameof(OnFallCompete));
        tween.Start();
    }

    private void StopFall()
    {
        tween.Disconnect("tween_all_completed",this,nameof(OnFallCompete));
        tween.StopAll();

        if(oneTime&&fallOnly)
        {
            state=HAMMERSTATE.PLAYEDONCE;
            SetPhysicsProcess(false);
        }
        else
        {
            state=HAMMERSTATE.BACK;
            tween.InterpolateMethod(
                this,
                nameof(Tweening),
                Rotation,
                oRotation,
                2f,
                Tween.TransitionType.Quad,
                Tween.EaseType.Out
            );
            tween.Connect("tween_all_completed",this,nameof(OnBackComplete));
            tween.Start();
        }

    }

    private void OnBackComplete()
    {
        tween.Disconnect("tween_all_completed", this, nameof(OnBackComplete));
        tween.StopAll();

        if(!oneTime) 
        {
            state=HAMMERSTATE.IDLE;
            if(mode==MODE.DEFAULT)
            {
                Start();
            }
        }
        else
        {
            state=HAMMERSTATE.PLAYEDONCE;
        }
    }

    private void OnFallCompete()
    {
        tween.Disconnect("tween_all_completed",this,nameof(OnFallCompete));
        StopFall();
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
                        StopFall();
                        player.EmitSignal(STATE.damage.ToString(),this,1f);
                        break;
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
        if(switchID==id&&mode==MODE.SWITCHABLE&&state!=HAMMERSTATE.FALLING)
        {
            if(state==HAMMERSTATE.BACK)
            {
                tween.Disconnect("tween_all_completed",this,nameof(OnBackComplete));
                tween.StopAll();
                state=HAMMERSTATE.IDLE;
            }
            Start();
        }
    }
   
}
