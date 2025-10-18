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
	[Export] private float JUMP_MAX_AIRBORNE_TIME=0.2f;

	private bool jumping=false;

	private RayCast2D rayCast2D;

	public override void _Ready()
	{
		base._Ready();

		animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

		rayCast2D=GetNode<RayCast2D>("RayCast2D");
		rayCast2D.Enabled=true;

		animationController.Play("idle");
        EmitSignal(STATE.idle.ToString());
	}

	public override void _PhysicsProcess(float delta)
	{
		if(animationPlayer.IsPlaying())
		{
			Position=startOffset+(ANIMATION_OFFSET*animationDirection);
		}

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

		if(inRange())
		{
			animationController.SpeedScale=2;
			velocity.y=-60f;
			jumping=true;
			onStroll();
		}
		else
		{
			if(MathUtils.randomRangeInt(0,1)==1)
			{
				FlipH();
			}

		}
	}

	protected override void stroll(float delta)
	{
		Vector2 force = new Vector2(0, GRAVITY);
		
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

		Vector2 snap=jumping?new Vector2(0f,0f):new Vector2(0f,8f);
		velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

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
			if(health<=0)
			{
				onDie();
			}
			else
			{
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

	protected override void onDamage(Player player, int amount)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.onDamage(player, amount);
			animationDirection = Mathf.Sign(GlobalPosition.x - player.GlobalPosition.x);
			animationPlayer.Play("HIT");
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


	private bool inRange()
	{
		return Mathf.Abs(Player.instance.GlobalPosition.DistanceTo(GlobalPosition))<ACTIVATION_RANGE;
	}

	protected override void FlipH()
	{
		animationController.FlipH ^= true;
		direction.x = animationController.FlipH ? -1f : 1f;
		rayCast2D.Position=FlipX(rayCast2D.Position);
		collisionController.Position=FlipX(collisionController.Position);
	}

}
