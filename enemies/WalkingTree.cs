using Godot;
using System;

public class WalkingTree : KinematicMonster
{
    [Export] private float WALK_FORCE=100f;
    [Export] private float WALK_MIN_SPEED=0f;
    [Export] private float WALK_MAX_SPEED=0f;
    private RayCast2D rayCast2D;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started", this, nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished", this, nameof(OnAnimationPlayerEnded));

        rayCast2D = GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled = true;

        SetSpawnFacing();

        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
    }

    protected override void Idle(float delta)
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

    protected override void Stroll(float delta)
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

    protected override void Damage(float delta)
    {
        if (!animationPlayer.IsPlaying())
        {
            if (health <= 0f)
            {
                OnDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                animationController.SpeedScale = 1f;
                OnIdle();
            }
        }

    }

    protected override void Passanger(float delta)
    {
        if (!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
        }
    } 

	protected override void OnDamage(Node2D node=null, int amount=0)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.OnDamage(node, 0);
            animationPlayer.Play("PASSANGER");
		}
	}

    public override void OnPassanger(Player player=null)
    {
        if (state != STATE.passanger)
        {
			base.OnDamage(player, 0);
            animationPlayer.Play("PASSANGER");

        }
    }   
            
    protected override void FlipH()
    {
        animationController.FlipH^=true;
        facing=Facing();
    }
}
