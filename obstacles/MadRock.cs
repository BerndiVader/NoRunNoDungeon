using Godot;
using System;

public class MadRock : KinematicBody2D
{
    private const float GRAVITY=1200f;
    private const float DISTANCE=10000000f;
    
    private int time;
    private Vector2 force,velocity,startPosition;
    private State state,nextState;

    private enum State
    {
        OnIdle,
        OnFalling,
        OnLift,
        OnWaiting,
        Unkown
    }

    protected delegate void Goal(float delta);
    protected Goal goal;

    public override void _Ready()
    {
        AddToGroup(GROUPS.OBSTACLES.ToString());

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        force=new Vector2(0f,GRAVITY);
        velocity=Vector2.Zero;
        startPosition=Position;

        state=State.Unkown;
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
    }

    private void Idle(float delta)
    {
        if(GlobalPosition.DistanceSquaredTo(Player.instance.GlobalPosition)<DISTANCE)
        {
            OnFalling();
        }
    }

    private void Falling(float delta)
    {
        velocity+=force*delta;
        KinematicCollision2D collision=MoveAndCollide(velocity*delta);
        if(collision!=null)
        {
            CreateParticles();
            OnWaiting(State.OnLift);
        }
    }

    private void Lift(float delta)
    {
        velocity.y=-100f;
        MoveAndCollide(velocity*delta);
        if(Position.y<=startPosition.y)
        {
            Position=startPosition;
            OnWaiting(State.OnIdle);
        }
    }

    private void Waiting(float delta)
    {
        int current=(int)OS.GetTicksMsec()/1000;
        if(current-time>0)
        {
            Call(nextState.ToString());
        }
    }

    private void OnIdle()
    {
        if(state!=State.OnIdle)
        {
            state=State.OnIdle;
            goal=Idle;
        }
    }

    private void OnFalling()
    {
        if(state!=State.OnFalling)
        {
            state=State.OnFalling;
            velocity=Vector2.Zero;
            goal=Falling;
        }
    }

    private void OnLift()
    {
        if(state!=State.OnLift)
        {
            state=State.OnLift;
            goal=Lift;
        }
    }

    private void OnWaiting(State next)
    {
        if(state!=State.OnWaiting)
        {
            nextState=next;
            state=State.OnWaiting;
            time=(int)OS.GetTicksMsec()/1000;
            goal=Waiting;
        }
    }

    private void CreateParticles()
    {
        MadRockParticles particle=ResourceUtils.particles[(int)PARTICLES.MADROCK].Instance<MadRockParticles>();
        particle.Position=Position+new Vector2(0f,16f);
        World.level.AddChild(particle);
    }

}
