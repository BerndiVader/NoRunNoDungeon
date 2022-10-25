using Godot;
using System;

public class MadRock : KinematicBody2D
{
    private VisibilityNotifier2D notifier;
    private float GRAVITY=1200f,distance=1000f*1000f;
    private int time;
    private Vector2 force,velocity,startPosition;
    private State state,nextState;
    private PackedScene particles=ResourceUtils.particles[(int)PARTICLES.MADROCK];

    private enum State
    {
        onIdle,
        onFalling,
        onLift,
        onWaiting
    }

    public override void _Ready()
    {
        AddToGroup(GROUPS.OBSTACLES.ToString());

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        force=new Vector2(0f,GRAVITY);
        velocity=Vector2.Zero;
        startPosition=Position;

        onIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case State.onIdle:
            {
                idle(delta);
                break;
            }
            case State.onFalling:
            {
                falling(delta);
                break;
            }
            case State.onLift:
            {
                lift(delta);
                break;
            }
            case State.onWaiting:
                waiting(delta);
                break;
        }

    }

    private void idle(float delta)
    {
        if(GlobalPosition.DistanceSquaredTo(World.instance.player.GlobalPosition)<distance)
        {
            onFalling();
        }

    }

    private void falling(float delta)
    {
        velocity+=force*delta;
        KinematicCollision2D collision=MoveAndCollide(velocity*delta);
        if(collision!=null)
        {
            createParticles();
            onWaiting(State.onLift);
        }

    }

    private void lift(float delta)
    {
        velocity.y=-100f;
        MoveAndCollide(velocity*delta);
        if(Position.y<=startPosition.y)
        {
            Position=startPosition;
            onWaiting(State.onIdle);
        }


    }

    private void waiting(float delta)
    {
        int current=(int)OS.GetTicksMsec()/1000;
        if(current-time>0)
        {
            Call(nextState.ToString());
        }


    }

    private void onIdle()
    {
        if(state!=State.onIdle)
        {
            state=State.onIdle;
        }

    }

    private void onFalling()
    {
        if(state!=State.onFalling)
        {
            state=State.onFalling;
            velocity=Vector2.Zero;
        }

    }

    private void onLift()
    {
        if(state!=State.onLift)
        {
            state=State.onLift;
        }

    }

    private void onWaiting(State next)
    {
        if(state!=State.onWaiting)
        {
            nextState=next;
            state=State.onWaiting;
            time=(int)OS.GetTicksMsec()/1000;
        }
    }

    private void createParticles()
    {
        MadRockParticles particle=(MadRockParticles)particles.Instance();
        particle.Position=World.instance.level.ToLocal(GlobalPosition+new Vector2(0f,16f));
        World.instance.level.AddChild(particle);
    }

    private void onExitedScreen()
    {
        QueueFree();
    }

}
