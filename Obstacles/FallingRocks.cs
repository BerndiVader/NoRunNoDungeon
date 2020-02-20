using Godot;
using System;

public class FallingRocks : StaticBody2D
{
    [Export] public float ActivationDistance=100f;
    protected VisibilityNotifier2D notifier;
    protected Area2D area,groundControl;
    protected int state=0;
    protected float GRAVITY=600f;
    protected Vector2 velocity=new Vector2(0f,0f);
    protected float shake;
    protected float ShakeMax=0.6f;
    bool colliding=false;
    Platform collider;



    public override void _Ready()
    {

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        notifier=new VisibilityNotifier2D();
        notifier.Connect("screen_entered",this,nameof(enteredScreen));
        notifier.Connect("screen_exited",this,nameof(exitedScreen));
        AddChild(notifier);

        SetCollisionLayerBit(1,true);     

        area=(Area2D)GetNode("Area2D");
        area.Connect("body_entered",this,nameof(onBodyEntered));
        area.Connect("body_exited",this,nameof(onBodyExited));

        groundControl=(Area2D)GetNode("Area2D2");
        groundControl.Connect("body_entered",this,nameof(onPlayerHit));

        AddToGroup("Level");

    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case 0:
                Player player=WorldUtils.world.player;
                Vector2 gamePos=Position+WorldUtils.world.level.Position;
                gamePos.y=player.Position.y;
                float distance=player.Position.DistanceTo(gamePos);
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
                GlobalTranslate(velocity*delta);
                break;
            case 2:
                break;
        }

        applyShake();

    }

    void enteredScreen()
    {
        SetPhysicsProcess(true);
    }

    void exitedScreen() 
    {
        SetPhysicsProcess(false);
        CallDeferred("queue_free");
    }

    void onBodyEntered(Node2D body)
    {
        if(body.IsInGroup("Platforms"))
        {
            collider=(Platform)body;
            colliding=true;
            shake=0.5f;
            WorldUtils.world.renderer.shake+=2;
        } 
        else if(body.IsInGroup("Level")&&body!=this)
        {
            state=2;
            area.Disconnect("body_entered",this,nameof(onBodyEntered));
            shake=0.5f;
            WorldUtils.world.renderer.shake+=2;
        }

    }

    void onBodyExited(Node2D body)
    {
        if(body.IsInGroup("Platforms"))
        {
            collider=null;
            colliding=false;
        }
    }

    void onPlayerHit(Node2D body)
    {
        if(body.IsInGroup("Players")) 
        {
            WorldUtils.world.CallDeferred("restartGame",true);
        }
    }

    void applyShake()
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
