using Godot;
using System;

public class SocketPlatform : Platform
{

    private const float SHAKE_START=0.2f;
    private const float SHAKE_MAX=2f;
    private const float SHAKE_DECAY=0.5f;

    [Export] private float DISTANCE_TO_GROUND=0f;
    [Export] private float MAX_MOVE_DISTANCE=0f;
    [Export] private float SPEED=50f;
    [Export] private Vector2 START_DIRECTION=Vector2.Up;
    [Export] private bool ONE_TIME=false;

    [Export] private bool USE_SHAKE=false;

    [Export] private float PARTICLE_MARGIN=-2f;
    [Export] private float RAY_START_LENGTH=30f;
    [Export] private float RAY_END_LENGTH=30f;


    private CollisionShape2D socketCollision;
    private Sprite socketSprite,sprite;
    private CPUParticles2D particles1,particles2;
    private Tween tween;

    private float particlesGroundY;
    private Vector2 GROUND_POSITION;
    private bool moveUp,used=false;
    private bool active=false;
    private bool particlesInit=false;
    private float shake=0f;

    private float distanceToPlayerSqrt;

    public override void _Ready()
    {
        base._Ready();

        GROUND_POSITION=new Vector2(Position.x,Position.y+DISTANCE_TO_GROUND);
        distanceToPlayerSqrt=DISTANCE_TO_PLAYER*DISTANCE_TO_PLAYER;

        particles1=GetNode<CPUParticles2D>(nameof(particles1));
        particles2=GetNode<CPUParticles2D>(nameof(particles2));

        sprite=GetNode<Sprite>(nameof(sprite));

        socketSprite=GetNode<Sprite>(nameof(socketSprite));
        socketCollision=GetNode<CollisionShape2D>(nameof(socketCollision));
        socketCollision.Shape=new RectangleShape2D();

        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_all_completed",this,nameof(TweenCompleted));
        moveUp=START_DIRECTION==Vector2.Up;

        if(platformState==PLATFORMSTATE.SWITCH||switchID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!particlesInit)
        {
            particlesInit=true;
            InitParticles();

        }

        Velocity=(Position-lastPosition)/delta;
        if(lastPosition!=Position)
        {
            UpdateSize();
        }
        lastPosition=Position;

        if(USE_SHAKE)
        {
            UpdateShake();
        }

        if(active||used)
        {
            return;
        }

        switch(platformState)
        {
            case PLATFORMSTATE.DISTANCE:
                if(GlobalPosition.DistanceSquaredTo(Player.instance.GlobalPosition)<distanceToPlayerSqrt)
                {
                    used=ONE_TIME;
                    StartTween(moveUp);
                    moveUp=!moveUp;
                }
                break;
            case PLATFORMSTATE.NORMAL:
                used=ONE_TIME;
                StartTween(moveUp);
                moveUp=!moveUp;
                break;
            case PLATFORMSTATE.ONETIME:
                StartTween(moveUp);
                moveUp=!moveUp;
                used=true;
                break;
            case PLATFORMSTATE.ONPLAYER:
                if(playerOn)
                {
                    used=ONE_TIME;
                    StartTween(moveUp);
                    moveUp=!moveUp;
                }
                break;
        }

    }

    private void UpdateSize()
    {
        Rect2 rect=socketSprite.RegionRect;
        rect.Size=new Vector2(rect.Size.x,Mathf.Max(0f,GROUND_POSITION.y-Position.y));
        socketSprite.RegionRect=rect;

        RectangleShape2D shape=(RectangleShape2D)socketCollision.Shape;
        shape.Extents=new Vector2(rect.Size.x-8f,rect.Size.y)*0.5f;
        socketCollision.Position=socketSprite.Position+rect.Position+rect.Size*0.5f;        
        
        UpdateParticlesPosition();
    }

    private void InitParticles()
    {
        FindAndSetGround();
        UpdateSize();
        UpdateParticlesPosition();
    }

    private void UpdateParticlesPosition()
    {
        float y=particlesGroundY-GlobalPosition.y;
        particles1.Position=new Vector2(particles1.Position.x,y);
        particles2.Position=new Vector2(particles2.Position.x,y);
    }

    private void FindAndSetGround()
    {
        float y=GROUND_POSITION.y-Position.y;
        Vector2 start=ToGlobal(new Vector2(0f,-RAY_START_LENGTH));
        Vector2 end=ToGlobal(new Vector2(0f,y+RAY_END_LENGTH));

        var hit=GetWorld2d().DirectSpaceState.IntersectRay(start,end,new Godot.Collections.Array(this),2);
        if(hit.Count>0)
        {
            particlesGroundY=((Vector2)hit["position"]).y-PARTICLE_MARGIN;
        }
        else
        {
            particlesGroundY=ToGlobal(new Vector2(0f,y)).y;
        }
    }

    private void UpdateShake()
    {
        if(active)
        {
            shake=Mathf.Min(Mathf.Max(shake,SHAKE_START),SHAKE_MAX);
            Vector2 offset=new Vector2
            (
                MathUtils.RandomRange(-shake,shake),
                MathUtils.RandomRange(-shake,shake)

            );
            sprite.Offset=offset;
            socketSprite.Offset=offset;
            shake*=SHAKE_DECAY;
        }
        else if(socketSprite.Offset!=Vector2.Zero||sprite.Offset!=Vector2.Zero)
        {
            socketSprite.Offset=Vector2.Zero;
            sprite.Offset=Vector2.Zero;
            shake=0f;
        }
    }

    private void StartTween(bool up)
    {
        Vector2 destination=up?startPosition+Vector2.Up*MAX_MOVE_DISTANCE:GROUND_POSITION;

        float distance=Position.DistanceTo(destination);
        if(distance<=0f||SPEED<=0f)
        {
            return;
        }
        active=true;

        bumpTween.StopAll();
        tween.StopAll();
        tween.InterpolateProperty(this,"position",Position,destination,
        distance/SPEED,Tween.TransitionType.Sine,Tween.EaseType.Out);
        tween.Start();
        particles1.Emitting=particles2.Emitting=true;
        
    }

    protected override bool BumpBlocked()
    {
        return active;
    }    

    private void TweenCompleted()
    {
        active=false;
        particles1.Emitting=particles2.Emitting=false;
    }

    public override void SwitchCall(string id)
    {
        if(switchID==id)
        {
            if(!active&&!used)
            {
                used=ONE_TIME;
                StartTween(moveUp);
                moveUp=!moveUp;
            }
            else if(HIDE_USAGE==PLATFORMSTATE.SWITCH)
            {
                SwitchHiding();
            }
        }
    }    

}
