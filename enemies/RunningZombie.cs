using Godot;
using System;

public class RunningZombie : KinematicMonster
{
	[Export] private float ACTIVATION_RANGE=400f;
	[Export] private float WALK_FORCE=600f;
	[Export] private float WALK_MIN_SPEED=10f;
	[Export] private float WALK_MAX_SPEED=60f;
	[Export] private float STOP_FORCE=1300f;
	[Export] private float JUMP_SPEED=130f;

	private bool jumping = false;
	private Vector2 snap = new Vector2(0f, 8f);

	private RayCast2D rayCast2D;

    public override void _Ready()
	{
		base._Ready();

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

		rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
		rayCast2D.Enabled=true;

		animationController.Play("idle");
        EmitSignal(STATE.idle.ToString());
	}

	public override void _PhysicsProcess(float delta)
	{
		if (animationPlayer.IsPlaying())
		{
			Position = startOffset + (ANIMATION_OFFSET * animationDirection);
		}

		goal(delta);
	}

	protected override void idle(float delta)
	{
        velocity += FORCE * delta;
        velocity = MoveAndSlideWithSnap(velocity, snap, Vector2.Up, false, 4, 0.785398f, true);

        int slides = GetSlideCount();
        for (int i = 0; i < slides; i++)
        {
            if (GetSlideCollision(i).Collider is Node2D node && node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform platform = node as Platform;
                velocity.x += platform.CurrentSpeed.x * 1.8f;
            }
        }

		if(inRange())
		{
			animationController.SpeedScale=2;
			velocity.y=-60f;
			jumping=true;
			onStroll();
		}
		else
		{
			if(MathUtils.randBool())
			{
				FlipH();
			}

		}
	}

	protected override void stroll(float delta)
	{
		Vector2 force = new Vector2(FORCE);
		
		bool isOnFloor = IsOnFloor();

		bool jump=!rayCast2D.IsColliding()&&isOnFloor;

		if(direction==Vector2.Left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
		{
			force.x-=WALK_FORCE;
		} 
		else if(direction==Vector2.Right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
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
		velocity=MoveAndSlideWithSnap(velocity,jumping?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);

		if(isOnFloor)
		{
			Vector2 floorVelocity=GetFloorVelocity();
			if(floorVelocity!=Vector2.Zero)
			{
				MoveAndCollide(-floorVelocity*delta);
			}
			jumping=false;
		}

		if(IsOnWall())
		{
			FlipH();
		}

		if(jump&&!jumping) 
		{
			switch(MathUtils.randomRangeInt(1,5))
			{
				case 1:
				case 2:
				{
					velocity.y=-JUMP_SPEED;
					jumping=true;
					break;
				}
				case 3:
				case 4:
				{
					FlipH();
					break;
				}
				case 5:
				{
					onIdle();
					break;
				}
			}
		}
	}

	protected override void fight(float delta)
	{
		throw new NotImplementedException();
	}

	protected override void damage(float delta)
	{
		if(!animationPlayer.IsPlaying())
		{
			if(health<=0f)
			{
				onDie();
			}
			else
			{
            	staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",false);
				animationController.SpeedScale=1;
				onIdle();
			}
		}
	}

	protected override void passanger(float delta)
	{
		if(!animationPlayer.IsPlaying())
		{
			base.passanger(delta);
		}
	}

	protected override void die(float delta)
	{
		base.die(delta);
	}

	protected override void calm(float delta)
	{
		throw new NotImplementedException();
	}	

	protected override void onDamage(Player player=null, int amount=0)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.onDamage(player, amount);
			animationDirection = Mathf.Sign(GlobalPosition.x - player.GlobalPosition.x);
			animationPlayer.Play("HIT");
		}
	}    

	public override void onPassanger(Player player=null)
	{
		if(state!=STATE.passanger)
		{
			base.onPassanger(player);
			animationPlayer.Play("PASSANGER");
		}
	}


	private bool inRange()
	{
		return Mathf.Abs(Player.instance.GlobalPosition.DistanceTo(GlobalPosition))<ACTIVATION_RANGE;
	}

	protected override void FlipH()
	{
		animationController.FlipH ^= true;
		direction = animationController.FlipH ? Vector2.Left : Vector2.Right;
		rayCast2D.Position=FlipX(rayCast2D.Position);
		collisionController.Position=FlipX(collisionController.Position);
	}

}
