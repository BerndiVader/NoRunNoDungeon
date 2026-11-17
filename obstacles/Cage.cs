using Godot;
using System;

public class Cage : KinematicBody2D
{
    [Export]private float GRAVITY=500f;
    [Export] protected float STOP_FORCE=1300f;
    [Export] protected float FRICTION=0.01f;

    private bool wasVisible=false;
    private Vector2 velocity=Vector2.Zero;
    private Vector2 FORCE;
    protected readonly Vector2 SNAP=new Vector2(0f,8f);
    private bool noSnap=false;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Connect("screen_exited",this,nameof(OnScreenExited));
        notifier.Connect("screen_entered",this,nameof(OnScreenEntered));

        AddUserSignal(STATE.damage.ToString());
        Connect(STATE.damage.ToString(),this,nameof(OnDamage));        

        FORCE=new Vector2(0f,GRAVITY);
    }

    public override void _PhysicsProcess(float delta)
    {
        Navigation(delta);
    }

    private void Navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,noSnap?Vector2.Zero:SNAP,Vector2.Up,false,4,0.785398f,true);
        noSnap=false;

        int slides=GetSlideCount();
        if(slides>0)
        {
            bool stop=false;
            for(int i=0;i<slides;i++)
            {
                if(GetSlideCollision(i).Collider is Platform platform)
                {
                    velocity.x=platform.Velocity.x;
                    break;
                } 
                else if(!stop)
                {
                    velocity=StopX(velocity,delta);
                    stop=true;
                }
            }
        }
        else
        {
            velocity=StopX(velocity,delta);
        }
    }

    private Vector2 StopX(Vector2 velocity,float delta)
    {
        velocity.x=Mathf.Max(Mathf.Abs(velocity.x)-STOP_FORCE*delta,0f)*Mathf.Sign(velocity.x);
        return velocity;
    }

    protected void OnDamage(Node2D node=null, float amount=0f,bool overrideDestroyable=false)
    {
        Renderer.instance.Shake(1f);
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
