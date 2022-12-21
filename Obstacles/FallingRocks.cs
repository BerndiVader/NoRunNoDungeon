using Godot;
using System;

public class FallingRocks : StaticBody2D
{
    [Export] private float ActivationDistance=10f;
    private Area2D area;
    private float GRAVITY=600f;
    private Vector2 velocity=new Vector2(0f,0f);
    private float shake;
    private float ShakeMax=0.6f;
    private bool colliding=false;
    private Platform collider;
    private Vector2 force;
    private State state;

    private enum State
    {
        IDLE=0,
        FALLING=1,
        FALLEN=2
    }

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        area=GetNode<Area2D>("Area2D");
        area.Connect("body_entered",this,nameof(onBodyEntered));
        area.Connect("body_exited",this,nameof(onBodyExited));

        GetNode<Area2D>("Area2D2").Connect("body_entered",this,nameof(onPlayerHit));

        AddToGroup(GROUPS.LEVEL.ToString());

        force=new Vector2(0f,GRAVITY);

        state=State.IDLE;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case State.IDLE:
                float distance=Mathf.Abs(GlobalPosition.x-Player.instance.GlobalPosition.x);
                if(distance<ActivationDistance) 
                {
                    state=State.FALLING;
                }
                break;
            case State.FALLING:
                if(colliding)
                {
                    velocity=collider.CurrentSpeed;
                }
                else
                {
                    velocity+=force*delta;
                }
                Translate(velocity*delta);
                break;
            case State.FALLEN:
                if(colliding)
                {
                    velocity=collider.CurrentSpeed;
                    Translate(velocity*delta);
                }
                break;
        }
        applyShake();
    }

    private void onBodyEntered(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLATFORMS.ToString()))
        {
            collider=(Platform)body;
            colliding=true;
            shake=0.5f;
            World.instance.renderer.shake+=2;
        } 
        else if(body.IsInGroup(GROUPS.LEVEL.ToString())&&body!=this)
        {
            state=State.FALLEN;
            area.Disconnect("body_entered",this,nameof(onBodyEntered));
            shake=0.5f;
            World.instance.renderer.shake+=2;
        }

    }

    private void onBodyExited(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLATFORMS.ToString()))
        {
            collider=null;
            colliding=false;
        }
    }

    private void onPlayerHit(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLAYERS.ToString())&&state==State.FALLING) 
        {
            body.EmitSignal(STATE.damage.ToString(),1f,this);
        }
    }

    private void applyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.02f)
        {
            float offset=(float)MathUtils.randomRange(-shake,shake);
            Rotation=offset;
            shake*=0.9f;
        } 
        else if(shake>0f)
        {
            shake=0f;
            Rotation=0;
        }
    }

}