using Godot;
using System;

public class SocketPlatform : Platform
{
    [Export] private float DISTANCE_TO_GROUND=0f;
    [Export] private float MAX_MOVE_DISTANCE=0f;
    [Export] private float SPEED=50f;
    [Export] private Vector2 START_DIRECTION=Vector2.Up;
    [Export] private bool ONE_TIME=false;

    [Export] private float PARTICLE_MARGIN=-2f;
    [Export] private float RAY_START_LENGTH=30f;
    [Export] private float RAY_END_LENGTH=30f;


    private CollisionShape2D socketCollision;
    private Sprite socketSprite;
    private CPUParticles2D particles1,particles2;
    private Tween tween;

    private float particlesGroundY;
    private Vector2 GROUND_POSITION;
    private bool moveUp,used=false;

    private float distanceToPlayerSqrt;

    public override void _Ready()
    {
        base._Ready();

        GROUND_POSITION=new Vector2(Position.x,Position.y+DISTANCE_TO_GROUND);
        distanceToPlayerSqrt=DISTANCE_TO_PLAYER*DISTANCE_TO_PLAYER;

        particles1=GetNode<CPUParticles2D>(nameof(particles1));
        particles2=GetNode<CPUParticles2D>(nameof(particles2));
        SetParticlesGlobalY();

        socketSprite=GetNode<Sprite>(nameof(socketSprite));
        socketCollision=GetNode<CollisionShape2D>(nameof(socketCollision));
        socketCollision.Shape=new RectangleShape2D();

        UpdateSize();
        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_all_completed",this,nameof(TweenCompleted));
        moveUp=START_DIRECTION==Vector2.Up;

    }

    public override void _PhysicsProcess(float delta)
    {
        if(!tween.IsActive()&&!used&&GlobalPosition.DistanceSquaredTo(Player.instance.GlobalPosition)<distanceToPlayerSqrt)
        {
            if(ONE_TIME)
            {
                used=true;
            }
            
            StartTween(moveUp);
            moveUp=!moveUp;
        }

        if(lastPosition!=Position)
        {
            UpdateSize();
        }
        lastPosition=Position;

    }


    private void UpdateSize()
    {
        float distanceToGround=Mathf.Max(0f,GROUND_POSITION.y-Position.y);

        Rect2 rect=socketSprite.RegionRect;
        rect.Size=new Vector2(rect.Size.x,distanceToGround);
        socketSprite.RegionRect=rect;

        RectangleShape2D shape=(RectangleShape2D)socketCollision.Shape;
        shape.Extents=new Vector2(rect.Size.x-8f,rect.Size.y)*0.5f;
        socketCollision.Position=socketSprite.Position+rect.Position+rect.Size*0.5f;        
        
        UpdateParticlesPosition();
    }

    private void UpdateParticlesPosition()
    {
        float localY=particlesGroundY-GlobalPosition.y;
        particles1.Position=new Vector2(particles1.Position.x,localY);
        particles2.Position=new Vector2(particles2.Position.x,localY);
    }

    private void SetParticlesGlobalY()
    {
        float localGroundY=GROUND_POSITION.y-Position.y;
        Vector2 rayStart=ToGlobal(new Vector2(0f,-RAY_START_LENGTH));
        Vector2 rayEnd=ToGlobal(new Vector2(0f,localGroundY+RAY_END_LENGTH));

        var hit=GetWorld2d().DirectSpaceState.IntersectRay(rayStart,rayEnd,new Godot.Collections.Array(this),2);
        if(hit.Count>0)
        {
            particlesGroundY=((Vector2)hit["position"]).y-PARTICLE_MARGIN;
        }
        else
        {
            particlesGroundY=ToGlobal(new Vector2(0f,localGroundY)).y;
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

        tween.StopAll();
        tween.InterpolateProperty(this,"position",Position,destination,
        distance/SPEED,Tween.TransitionType.Sine,Tween.EaseType.Out);
        tween.Start();
        particles1.Emitting=particles2.Emitting=true;
        
    }

    private void TweenCompleted()
    {
        particles1.Emitting=particles2.Emitting=false;
    }

}
