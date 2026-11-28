using Godot;
using System;

public class Prisoner : KinematicMonster
{

	[Export] protected float WALK_FORCE=600f;
	[Export] protected float WALK_MIN_SPEED=10f;
	[Export] protected float WALK_MAX_SPEED=60f;

    private bool wasOnScreen=false;

    public override void _Ready()
    {
        base._Ready();
        staticBody.QueueFree();

        CollisionMask=2u;

		SetSpawnFacing();
		OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        goal(delta);
    }

	protected override void Idle(float delta)
	{
        if(IsOnFloor())
        {
            OnStroll();
        }

		Navigation(delta);
	}

    protected override void Stroll(float delta)
    {
		Vector2 force=new Vector2(FORCE);
		if(facing==Vector2.Left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
		{
			force.x-=WALK_FORCE;
		}
		else if(facing==Vector2.Right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
		{
			force.x+=WALK_FORCE;
		}
		else
		{
			velocity=StopX(velocity,delta);
		}

		if(IsOnWall())
		{
			FlipH();
		}

        if(IsInsideView())
        {
            wasOnScreen=true;
        }
        else if(wasOnScreen)
        {
            wasOnScreen=false;
            CallDeferred("queue_free");
        }

		velocity+=force*delta;
		velocity=MoveAndSlideWithSnap(velocity,noSnap?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        noSnap=false;
    }

    protected override void OnDamage(Node2D node=null,float amount=0,bool overrideDestroyable=false)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            noSnap=true;

            if(node==null)
            {
                node=this;
            }
            else if(node is Player)
            {
                attacker=Player.instance;
            }

            animationDirection=Mathf.Sign(GlobalPosition.x-node.GlobalPosition.x);
            velocity.x+=DAMAGE_FORCE.x*animationDirection;
            velocity.y+=DAMAGE_FORCE.y;

            lastState=state;
            state=STATE.damage;       

            health-=99f;
            goal=Damage; 

        }
    }


    protected override void FlipH()
    {
		animationController.FlipH^=true;
		collisionController.Position=FlipX(collisionController.Position);
		facing=Facing();
    }

    protected override void OnScreenExited()
    {
        CallDeferred("queue_free");
    }

    private bool IsInsideView()
    {
        Viewport viewport=GetViewport();
        Transform2D toWorld=viewport.CanvasTransform.AffineInverse();
        Rect2 screen=viewport.GetVisibleRect();
        Vector2 min=toWorld*screen.Position;
        Vector2 max=toWorld*screen.End;
        return new Rect2(min,max-min).HasPoint(GlobalPosition);
    }

}
