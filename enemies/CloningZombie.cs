using Godot;

public class CloningZombie : KinematicMonster
{
	[Export] protected float ACTIVATION_RANGE=400f;
	[Export] protected float WALK_FORCE=600f;
	[Export] protected float WALK_MIN_SPEED=10f;
	[Export] protected float WALK_MAX_SPEED=60f;
	[Export] protected float JUMP_SPEED=130f;

	protected float COOLDOWNER_TIME=1.0f;
	protected float cooldowner=0f;
	protected bool jumping=false;
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

    protected override void Navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,justDamaged?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        justDamaged=false;

        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                if(GetSlideCollision(i).Collider is Platform platform)
                {
                    velocity.x=platform.CurrentSpeed.x;
                }
                else
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

    protected override void Idle(float delta)
    {
        Navigation(delta);
    }

    protected override void Damage(float delta)
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

    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
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


    protected override void OnDamage(Node2D node=null,float amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node,amount);
            if(GlobalPosition.x-node.GlobalPosition.x<0)
            {
                animationDirection=-1;
            }
            justDamaged=true;
            velocity.x+=DAMAGE_FORCE.x*animationDirection;
            velocity.y+=DAMAGE_FORCE.y;
            animationPlayer.Play("HIT");
        }
    }

	protected override void FlipH()
	{
		animationController.FlipH^=true;
		rayCast2D.Position=FlipX(rayCast2D.Position);
		collisionController.Position=FlipX(collisionController.Position);
		facing=Facing();
	}

}
