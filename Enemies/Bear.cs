using Godot;
using System;

public class Bear : KinematicMonster
{
    [Export] private float WALK_FORCE=100f;
    [Export] private float WALK_MIN_SPEED=2f;
    [Export] private float WALK_MAX_SPEED=50f;
    [Export] private float STOP_FORCE=1300f;
    private Vector2 snap=new Vector2(0f,8f);
    private Area2D damager;
    private RayCast2D raycast;
    
    public override void _Ready()
    {
        base._Ready();

        damager=GetNode<Area2D>("Damager");
        damager.Connect("body_entered",this,nameof(onPlayerEntered));
        raycast=GetNode<RayCast2D>(nameof(RayCast2D));
        raycast.Enabled=true;

        GetNode<CPUParticles2D>("Particles1").Emitting=true;
        GetNode<CPUParticles2D>("Particles2").Emitting=true;
        
        EmitSignal(STATE.idle.ToString());

        if(MathUtils.randomRangeInt(1,3)==2)
        {
            flipH();
        }

    }

    public override void _PhysicsProcess(float delta)
    {   
        goal(delta);
    }

    protected override void idle(float delta)
    {
		velocity+=force*delta;
		KinematicCollision2D collision=MoveAndCollide(velocity*delta);

		if(collision!=null)
		{
			velocity=velocity.Bounce(collision.Normal)*friction;

			Node2D node=(Node2D)collision.Collider;
			if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
			{
				Platform collider=(Platform)node;
				velocity.x+=collider.CurrentSpeed.x*1.8f;
			}
		}
        else
        {
            EmitSignal(STATE.stroll.ToString());
        }
    }

    protected override void stroll(float delta)
    {
        Vector2 direction=getDirection();
        Vector2 force=new Vector2(0,GRAVITY);

        bool left=direction.x<0f;
        bool right=direction.x>0f;

        if(left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        }
        else if(right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
        {
            force.x+=WALK_FORCE;
        }
        else
        {
            float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
            if(xLength<0f) {
                xLength=0f;
            }
            velocity.x=xLength*Mathf.Sign(velocity.x);
        }

        velocity+=force*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        if(IsOnFloor())
        {
            velocity-=GetFloorVelocity()*delta;
        }

        if(IsOnWall()||!raycast.IsColliding())
        {
            flipH();
        }
    }

    protected override void onDamage(Player player, int amount)
    {
       damager.SetDeferred("Monitoring",false);
       base.onDamage(player, amount);
    }

    private void onPlayerEntered(Node body)
    {
        damager.SetDeferred("Monitoring",false);
        body.EmitSignal(STATE.damage.ToString(),1f,this);
    }

    private void flipH()
    {
        if(Scale.y<0)
        {
            LookAt(GlobalPosition+Vector2.Right);
        }
        else
        {
            LookAt(GlobalPosition+Vector2.Left);
        }
        Scale*=new Vector2(1f,-1f);
    }

    private Vector2 getDirection()
    {
        if(Scale.y>0)
        {
            return Vector2.Right;
        }
        return Vector2.Left;
    }

}
