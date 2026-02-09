using Godot;
using Godot.Collections;

public class CloningZombie : KinematicMonster
{
	[Export] protected float ACTIVATION_RANGE=400f;
	[Export] protected float WALK_FORCE=600f;
	[Export] protected float WALK_MIN_SPEED=10f;
	[Export] protected float WALK_MAX_SPEED=60f;
	[Export] protected float JUMP_SPEED=130f;
	[Export] protected float COOLDOWNER_TIME=1.0f;

	protected float cooldowner=0f;
	protected bool jumping=false;
    protected bool hasCloned=false;

	protected RayCast2D rayCast2D;

    public override void _Ready()
    {
        base._Ready();

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

		rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
		rayCast2D.Enabled=true;

		SetSpawnFacing();
		OnIdle();
    }

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		goal(delta);
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

		velocity+=force*delta;
		velocity=MoveAndSlideWithSnap(velocity,justDamaged?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        justDamaged=false;

        if(IsOnFloor())
        {
            jumping=false;
        }

        if(!jumping&&!rayCast2D.IsColliding())
        {
            OnIdle();
        }
        else if(ShouldClone())
        {
            OnAlert();
        }
        else if(LookingTo(Player.instance.GlobalPosition)&&DistanceToPlayer()<20f)
        {
            if(!jumping&&rayCast2D.IsColliding())
            {
                FlipH();
            }
        }
           
    }

    protected override void Idle(float delta)
    {
        Navigation(delta);

        if(ShouldClone())
        {
            OnAlert();
        }
        else if(LookingTo(Player.instance.GlobalPosition)&&DistanceToPlayer()<50f)
        {
            FlipH();
            OnStroll();
        }
        else if(DistanceToPlayer()<20f&&!rayCast2D.IsColliding())
        {
            velocity.y=-JUMP_SPEED;
            jumping=true;
            justDamaged=true;
            OnStroll();
        }

    }

    // announce cloning process
    protected override void Alert(float delta)
    {
        if(!animationController.Playing)
        {
            OnCalm();
        }
        Navigation(delta);
    }

    // do cloning process
    protected override void Calm(float delta)
    {
        if(!animationController.Playing)
        {
            CloningZombie clone=(CloningZombie)Duplicate((int)DuplicateFlags.UseInstancing);
            World.level.AddChild(clone);
            clone.velocity+=-facing*DEFAULT_DAMAGE_FORCE;
            velocity+=facing*DEFAULT_DAMAGE_FORCE;

            if(clone.FACING==clone.direction)
            {
                clone.FlipH();
            }

            Dust dust=ResourceUtils.dust.Instance<Dust>();
            dust.type=Dust.TYPE.JUMP;
            dust.Position=new Vector2(Position.x,Position.y+7f);
            World.level.AddChild(dust);

            forcedState=false;
            hasCloned=true;
            OnIdle();
        }
        Navigation(delta);
    }

    protected override void Damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                OnDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",false);
                OnIdle();
            }
        }
        Navigation(delta);
    }

    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
        }
        Navigation(delta);
    }

    public override void OnPassanger(Player player=null)
    {
        if(forcedState)
        {
            return;
        }
        if(state!=STATE.passanger)
        {
            base.OnPassanger(player);
            animationPlayer.Play("PASSANGER");
        }        
    }

    protected override void OnDamage(Node2D node=null,float amount=0)
    {
        if(forcedState)
        {
            return;
        }
        if(state!=STATE.damage&&state!=STATE.die)
        {
            if(hasCloned)
            {
                base.OnDamage(node,amount);
                animationPlayer.Play("HIT");
            }
            else
            {
                OnAlert();
            }
        }
    }

    /*
    OnAlert: announce cloning event.
    */
    protected override void OnAlert()
    {
        if(forcedState)
        {
            return;
        }
        if(state!=STATE.alert)
        {
            forcedState=true;
            base.OnAlert();
            animationController.Play("alert");
        }
    }

    /*
    OnCalm: do cloning event.
    */
    protected override void OnCalm()
    {
        if(state!=STATE.calm)
        {
            base.OnCalm();
            animationController.Play("calm");
        }
    }

    private bool LookingTo(Vector2 globalposition)
    {
        if(Mathf.Abs(globalposition.y-GlobalPosition.y)<=16f)
        {
            Vector2 direction=GlobalPosition.DirectionTo(globalposition);
            return Mathf.Sign(direction.x)==facing.x;
        }
        else
        {
            return false;
        }
    }

    private bool ShouldClone()
    {
        return !hasCloned&&DistanceToPlayer()<10f;
    }

	protected override void FlipH()
	{
		animationController.FlipH^=true;
		rayCast2D.Position=FlipX(rayCast2D.Position);
		collisionController.Position=FlipX(collisionController.Position);
		facing=Facing();
	}

}
