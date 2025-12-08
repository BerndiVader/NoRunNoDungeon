using Godot;
using System;

public class WalkingTree : KinematicMonster
{
    [Export] private float WALK_FORCE=100f;
    [Export] private float WALK_MIN_SPEED=0f;
    [Export] private float WALK_MAX_SPEED=0f;
    
    private Vector2 snap=new Vector2(0f,8f);
    private RayCast2D rayCast2D;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started", this, nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished", this, nameof(onAnimationPlayerEnded));

        rayCast2D = GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled = true;

        EmitSignal(STATE.idle.ToString());

        if(MathUtils.randBool())
        {
            FlipH();
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
    }

    protected override void idle(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                var collision=GetSlideCollision(i);
                if(collision.Collider is Platform platform&&collision.Normal==Vector2.Up)
                {
                    velocity.x=platform.CurrentSpeed.x;
                } else
                {
                    velocity=StopX(velocity,delta);
                }
            }    
        }
        else
        {
            velocity=StopX(velocity,delta);
        }
    }

    protected override void stroll(float delta)
    {
        Vector2 force = new Vector2(FORCE);

        bool left = direction.x < 0f;
        bool right = direction.x > 0f;

        if (left && velocity.x <= WALK_MIN_SPEED && velocity.x > -WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        }
        else if (right && velocity.x >= -WALK_MIN_SPEED && velocity.x < WALK_MAX_SPEED)
        {
            force.x += WALK_FORCE;
        }
        else
        {
            velocity=StopX(velocity,delta);
        }

        velocity += force * delta;
        velocity = MoveAndSlideWithSnap(velocity, snap, Vector2.Up, false, 4, 0.785398f, true);

        if (IsOnFloor())
        {
            velocity -= GetFloorVelocity() * delta;
        }

    }

    protected override void damage(float delta)
    {
        if (!animationPlayer.IsPlaying())
        {
            if (health <= 0f)
            {
                onDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                animationController.SpeedScale = 1f;
                onIdle();
            }
        }

    }

    protected override void passanger(float delta)
    {
        if (!animationPlayer.IsPlaying())
        {
            base.passanger(delta);
        }
    } 

	protected override void onDamage(Player player=null, int amount=0)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.onDamage(player, 0);
            animationPlayer.Play("PASSANGER");
		}
	}

    public override void onPassanger(Player player=null)
    {
        if (state != STATE.passanger)
        {
			base.onDamage(player, 0);
            animationPlayer.Play("PASSANGER");

        }
    }   
            
    protected override void FlipH()
    {
        animationController.FlipH = !animationController.FlipH;
    }
}
