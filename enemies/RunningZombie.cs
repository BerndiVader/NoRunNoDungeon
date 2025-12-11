using Godot;
using System;

public class RunningZombie : KinematicMonster
{
	[Export] protected float ACTIVATION_RANGE=400f;
	[Export] protected float WALK_FORCE=600f;
	[Export] protected float WALK_MIN_SPEED=10f;
	[Export] protected float WALK_MAX_SPEED=60f;
	[Export] protected float JUMP_SPEED=130f;

	protected float COOLDOWNER_TIME=1.0f;
	protected float cooldowner=0f;

	protected bool jumping = false;
	protected Vector2 snap = new Vector2(0f, 8f);

	protected RayCast2D rayCast2D;

    public override void _Ready()
	{
		base._Ready();

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

		rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
		rayCast2D.Enabled=true;

		animationController.Play("idle");
        EmitSignal(STATE.idle.ToString());
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		if (animationPlayer.IsPlaying())
		{
			Position = startOffset + (ANIMATION_OFFSET * animationDirection);
		}

		goal(delta);
	}

	protected override void Idle(float delta)
	{
        velocity += FORCE * delta;
        velocity = MoveAndSlideWithSnap(velocity, snap, Vector2.Up, false, 4, 0.785398f, true);

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
		
		if(InRange())
		{
			animationController.SpeedScale=2;
			velocity.y=-60f;
			jumping=true;
			OnStroll();
		}
		else
		{
			if(cooldowner<=0f&&MathUtils.RandBool())
			{
				FlipH();
				cooldowner=COOLDOWNER_TIME;
			}
			else
            {
                cooldowner-=delta;
            }

		}
	}

	protected override void Stroll(float delta)
	{
		Vector2 force = new Vector2(FORCE);
		
		bool isOnFloor = IsOnFloor();

		bool jump=!rayCast2D.IsColliding()&&isOnFloor;

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
			switch(MathUtils.RandomRangeInt(1,5))
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
					OnIdle();
					break;
				}
			}
		}
	}

	protected override void Fight(float delta)
	{
		throw new NotImplementedException();
	}

	protected override void Damage(float delta)
	{
		if(!animationPlayer.IsPlaying())
		{
			if(health<=0f)
			{
				OnDie();
			}
			else
			{
            	staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",false);
				animationController.SpeedScale=1;
				OnIdle();
			}
		}
	}

	protected override void Passanger(float delta)
	{
		if(!animationPlayer.IsPlaying())
		{
			base.Passanger(delta);
		}
	}

	protected override void Die(float delta)
	{
		base.Die(delta);
	}

	protected override void Calm(float delta)
	{
		throw new NotImplementedException();
	}	

	protected override void OnDamage(Player player=null, int amount=0)
	{
		if(state!=STATE.damage&&state!=STATE.die)
		{
			base.OnDamage(player, amount);
			animationDirection = Mathf.Sign(GlobalPosition.x - player.GlobalPosition.x);
			animationPlayer.Play("HIT");
		}
	}    

	public override void OnPassanger(Player player=null)
	{
		if(state!=STATE.passanger)
		{
			base.OnPassanger(player);
			animationPlayer.Play("PASSANGER");
		}
	}


	private bool InRange()
	{
		return Mathf.Abs(Player.instance.GlobalPosition.DistanceTo(GlobalPosition))<ACTIVATION_RANGE;
	}

	protected override void FlipH()
	{
		animationController.FlipH ^= true;
		rayCast2D.Position=FlipX(rayCast2D.Position);
		collisionController.Position=FlipX(collisionController.Position);
		facing=Facing();
	}

}
