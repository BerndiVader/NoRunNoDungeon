using Godot;
using System;

public class FallingRocks : StaticBody2D
{
    [Export] private float ActivationDistance=100f;
    private Area2D area;
    private int state=0;
    private float GRAVITY=600f;
    private Vector2 velocity=new Vector2(0f,0f);
    private float shake;
    private float ShakeMax=0.6f;
    private bool colliding=false;
    private Platform collider;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        area=GetNode<Area2D>("Area2D");
        area.Connect("body_entered",this,nameof(onBodyEntered));
        area.Connect("body_exited",this,nameof(onBodyExited));

        GetNode<Area2D>("Area2D2").Connect("body_entered",this,nameof(onPlayerHit));

        AddToGroup(GROUPS.LEVEL.ToString());

    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case 0:
                Vector2 playerPos=World.instance.player.GlobalPosition;
                Vector2 gamePos=GlobalPosition;
                gamePos.y=playerPos.y;
                float distance=playerPos.DistanceTo(gamePos);
                if(distance<ActivationDistance) state=1;
                break;
            case 1:
                Vector2 force=new Vector2(0,GRAVITY);
                if(colliding)
                {
                    velocity=collider.CurrentSpeed;
                    force=Vector2.Zero;
                }
                velocity+=force*delta;
                Translate(velocity*delta);
                break;
            case 2:
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
            state=2;
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
        if(body.IsInGroup(GROUPS.PLAYERS.ToString())) 
        {
            World.instance.player.EmitSignal(SIGNALS.Damage.ToString(),1f,this);
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

    private void onExitedScreen()
    {
        QueueFree();
    }


}