using Godot;
using System;

public class WalkingTree : KinematicMonster
{
    [Export] private float WALK_FORCE=100f;
    [Export] private float WALK_MIN_SPEED=2f;
    [Export] private float WALK_MAX_SPEED=50f;
    [Export] private float STOP_FORCE = 1300f;    
    
    private Vector2 snap=new Vector2(0f,8f);
    private RayCast2D rayCast2D;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect("animation_started", this, nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished", this, nameof(onAnimationPlayerEnded));

        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled = true;

        animationController.Play("stroll");
        EmitSignal(STATE.stroll.ToString());

    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
    }

    protected override void stroll(float delta)
    {
        Vector2 direction = getDirection();
        Vector2 force = new Vector2(0, GRAVITY);

        bool left = direction.x < 0f;
        bool right = direction.x > 0f;

        if (left && velocity.x <= WALK_MIN_SPEED && velocity.x > -WALK_MAX_SPEED)
        {
            force.x -= WALK_FORCE;
        }
        else if (right && velocity.x >= -WALK_MIN_SPEED && velocity.x < WALK_MAX_SPEED)
        {
            force.x += WALK_FORCE;
        }
        else
        {
            float xLength = Mathf.Abs(velocity.x) - (STOP_FORCE * delta);
            if (xLength < 0f)
            {
                xLength = 0f;
            }
            velocity.x = xLength * Mathf.Sign(velocity.x);
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
            if (health <= 0)
            {
                onDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                animationController.SpeedScale = 1;
                onStroll();
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

	protected override void onDamage(Player player, int amount)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.onDamage(player, 0);
			if(GlobalPosition.x-player.GlobalPosition.x<0)
			{
				animationDirection=-1;
			}
			animationPlayer.Play("PASSANGER");
		}
	}    

	public override void onPassanger(Player player)
	{
		if(state!=STATE.passanger)
		{
			base.onPassanger(player);
			animationPlayer.Play("PASSANGER");
		}
	}       
        
    private Vector2 getDirection()
    {
        return Vector2.Left;
    }

    protected override void FlipH()
    {
        throw new NotImplementedException();
    }
}
