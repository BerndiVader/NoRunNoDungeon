using Godot;
using System;

public class RunningZombie : KinematicMonster
{
	[Export] public float ACTIVATION_RANGE=400f;
	[Export] public float GRAVITY=500f;
	[Export] public float WALK_FORCE=600f;
	[Export] public float WALK_MIN_SPEED=10f;
	[Export] public float WALK_MAX_SPEED=60f;
	[Export] public float STOP_FORCE=1300f;
	[Export] public float JUMP_SPEED=130f;
	[Export] public float JUMP_MAX_AIRBORNE_TIME=0.2f;

	Vector2 velocity=new Vector2(0f,0f);
	Vector2 direction=new Vector2(0f,0f);
	float onAirTime=100f;
	bool jumping=false;
	bool prevJumpPressed=false;

	RayCast2D rayCast2D;

	public override void _Ready()
	{
		base._Ready();

		animationPlayer=GetNode("AnimationPlayer") as AnimationPlayer;
		animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

		rayCast2D=GetNode("RayCast2D") as RayCast2D;
		rayCast2D.Enabled=true;

		animationController.Play("default");
		state=STATE.IDLE;
		lastState=state;

		direction=new Vector2(-1,0);
	}

	public override void _PhysicsProcess(float delta)
	{
		if(animationPlayer.IsPlaying())
		{
			Position=startOffset+(ANIMATION_OFFSET*animationDirection);
		}
		tick(delta);
	}

	protected override void idle(float delta)
	{
		Vector2 force=new Vector2(0,GRAVITY);
		velocity+=force*delta;

		KinematicCollision2D collision=MoveAndCollide(velocity*delta);

		if(IsOnFloor())
		{
			velocity+=GetFloorVelocity()*delta;
		}

		if(collision!=null)
		{
			Node2D node=(Node2D)collision.Collider;
			velocity=velocity.Bounce(collision.Normal)*0.01f;

			if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
			{
				Platform collider=(Platform)node;
				velocity.x+=collider.CurrentSpeed.x*1.8f;
			}
		}

		if(inRange())
		{
			lastState=state;
			state=STATE.ATTACK;
			animationController.Play("run");
			animationController.SpeedScale=2;
			velocity.y=-60f;
			jumping=true;
		}
	}

	protected override void attack(float delta)
	{
		Vector2 force=new Vector2(0,GRAVITY);

		bool left=direction.x==-1;
		bool right=direction.x==1;
		bool jump=!rayCast2D.IsColliding();

		bool stop=true;

		if(left)
		{
			if(velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED) 
			{
				force.x-=WALK_FORCE;
				stop=false;
			}
		} 
		else if(right)
		{
			if(velocity.x>=-(WALK_MIN_SPEED)&&velocity.x<(WALK_MAX_SPEED)) 
			{
				force.x+=WALK_FORCE;
				stop=false;
			}
		}

		if(stop)
		{
			float vSign=Mathf.Sign(velocity.x);
			float vLen=Mathf.Abs(velocity.x);

			vLen-=STOP_FORCE*delta;
			if(vLen<0f) vLen=0f;
			velocity.x=vLen*vSign;
		}

		velocity+=force*delta;

		Vector2 snap=jumping?new Vector2(0f,0f):new Vector2(0f,8f);
		velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

		if(IsOnFloor())
		{
			velocity-=GetFloorVelocity()*delta;
			onAirTime=0f;
		}

		if(jumping&&velocity.y>0f) 
		{
			jumping=false;
		}

		if(IsOnWall())
		{
			direction=new Vector2(direction.x*-1,0);
			FlipH();
		}

		if(onAirTime<JUMP_MAX_AIRBORNE_TIME&&jump&&!prevJumpPressed&&!jumping) 
		{
			int decide=MathUtils.randomRangeInt(1,3);
			switch(decide)
			{
				case 1:
				{
					velocity.y=-JUMP_SPEED;
					jumping=true;
					break;
				}
				case 2:
				{
					direction=new Vector2(direction.x*-1,0);
					FlipH();
					break;
				}
			}
		}

		onAirTime+=delta;
		prevJumpPressed=jump;

		onAirTime+=delta;
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
				EmitSignal(SIGNALS.Die.ToString());
			}
			else
			{
				state=lastState;
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

	protected override void onDamage(Player player, int amount)
	{
		if(state!=STATE.DAMAGE&&state!=STATE.DIE)
		{
			base.onDamage(player, amount);
			if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
			{
				animationDirection=-1;
			}
			animationPlayer.Play("HIT");
		}
	}    

	protected override void onPassanger(Player player)
	{
		base.onPassanger(player);
		animationPlayer.Play("PASSANGER");
	}


	bool inRange()
	{
		return Mathf.Abs(WorldUtils.world.player.GlobalPosition.DistanceTo(GlobalPosition))<ACTIVATION_RANGE;
	}

	void FlipH()
	{
		animationController.FlipH^=true;
		Vector2 position=rayCast2D.Position;
		position.x*=-1;
		rayCast2D.Position=position;
		position=collisionController.Position;
		position.x*=-1;
		collisionController.Position=position;
	}

	protected override void calm(float delta)
	{
		throw new NotImplementedException();
	}
}
