using Godot;
using System;

public class FallingRocks : StaticBody2D,ISwitchable
{
    [Export] private float ActivationDistance=10f;
    [Export] private string switchID="";
    private Area2D area;
    private const float GRAVITY=600f;
    private Vector2 velocity=Vector2.Zero;
    private float shake;
    private const float ShakeMax=0.6f;
    private bool colliding=false;
    private Platform collider;
    private Vector2 force;
    private State state;

    private enum State
    {
        IDLE,
        FALLING,
        FALLEN
    }

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        area=GetNode<Area2D>("Area2D");
        area.Connect("body_entered",this,nameof(OnBodyEntered));
        area.Connect("body_exited",this,nameof(OnBodyExited));

        GetNode<Area2D>("Area2D2").Connect("body_entered",this,nameof(OnPlayerHit));
        AddToGroup(GROUPS.OBSTACLES.ToString());

        if(switchID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

        force=new Vector2(0f,GRAVITY);
        state=State.IDLE;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case State.IDLE:
                if(switchID=="")
                {
                    float distance=Mathf.Abs(GlobalPosition.x-Player.instance.GlobalPosition.x);
                    if(distance<ActivationDistance) 
                    {
                        state=State.FALLING;
                    }
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
        ApplyShake();
    }

    private void OnBodyEntered(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLATFORMS.ToString())&&body!=this)
        {
            collider=(Platform)body;
            colliding=true;
            shake=ShakeMax;
            World.instance.renderer.shake+=2d;
            state=State.FALLEN;
            AddToGroup(GROUPS.PLATFORMS.ToString());
        } 
        else if(body.IsInGroup(GROUPS.LEVEL.ToString())&&body!=this)
        {
            area.Disconnect("body_entered",this,nameof(OnBodyEntered));
            shake=ShakeMax;
            World.instance.renderer.shake+=2d;
            state=State.FALLEN;
            AddToGroup(GROUPS.LEVEL.ToString());
        }

    }

    private void OnBodyExited(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLATFORMS.ToString()))
        {
            collider=null;
            colliding=false;
        }
    }

    private void OnPlayerHit(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLAYERS.ToString())&&state==State.FALLING) 
        {
            body.EmitSignal(STATE.damage.ToString(),1f,this);
        }
    }

    private void ApplyShake()
    {
        shake=Mathf.Min(shake,ShakeMax);
        if(shake>=ShakeMax*0.5f)
        {
            float offset=(float)MathUtils.RandomRange(-shake,shake);
            Rotation=offset;
            shake*=0.9f;
        } 
        else if(shake>0f)
        {
            float offset = (float)MathUtils.RandomRange(-ShakeMax * shake, ShakeMax * shake);
            offset *= MathUtils.RandSign();
            shake=0f;
            Rotation = offset;
        }
    }

    public void SwitchCall(string id)
    {
        if(id==switchID&&state==State.IDLE)
        {
            state=State.FALLING;
        }
    }
}