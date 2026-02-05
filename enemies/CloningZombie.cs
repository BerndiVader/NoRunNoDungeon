using Godot;

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

    protected override void Idle(float delta)
    {
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
