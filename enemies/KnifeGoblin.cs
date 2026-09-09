using Godot;

public class KnifeGoblin : KinematicMonster
{
    [Export] protected float ACTIVATION_DISTANCE=80f;
    [Export] protected float WALK_FORCE=600f;
    [Export] protected float WALK_MIN_SPEED=20f;
    [Export] protected float WALK_MAX_SPEED=120f;

    protected RayCast2D rayCast2D;
    protected RayCast2D playerCast;
    protected CPUParticles2D aura;
    protected MonsterWeapon weapon;


    protected float activation_distance_sqrd;


    public override void _Ready()
    {
        base._Ready();

        aura=GetNode<CPUParticles2D>("Aura");
        aura.OneShot=true;
        aura.Emitting=false;        

        activation_distance_sqrd=ACTIVATION_DISTANCE*ACTIVATION_DISTANCE;

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;

        playerCast=GetNode<RayCast2D>("PlayerCast");
        playerCast.Enabled=true;

        weapon=GetNode<MonsterWeapon>("KnifeMonster");
        if(weapon!=null)
        {
            weapon._Init();
        }

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

        if(!noSnap&&playerCast.IsColliding()&&!weapon.IsPlaying())
        {
            weapon.Attack();
        }

        if(DistanceSquaredToPlayer()<activation_distance_sqrd)
        {
            if(facing.x!=Mathf.Sign(Player.instance.GlobalPosition.x-GlobalPosition.x))
            {
                FlipH();
                OnStroll();
            }
            else if(rayCast2D.IsColliding())
            {
                OnStroll();
            }
        }
        Navigation(delta);
    }

    protected override void Stroll(float delta)
    {
        if(playerCast.IsColliding()&&DistanceSquaredToPlayer()<100f&&!weapon.IsPlaying())
        {
            weapon.Attack();
        }

        if(!rayCast2D.IsColliding())
        {
            OnIdle();
            return;
        }

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
		velocity=MoveAndSlideWithSnap(velocity,noSnap?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        noSnap=false;
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
                OnStroll();
            }
        }

        Navigation(delta);
    }   

    protected override void Passanger(float delta)
    {
        base.Passanger(delta);
    }

    protected override void OnPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
            base.OnPassanger(player);
            animationPlayer.Play("PASSANGER");
        }        
    }


    protected override void OnStroll()
    {
        onDelay=false;
        if(state!=STATE.stroll)
        {
            lastState=state;
            state=STATE.stroll;
            animationController.Play("stroll");
            goal=Stroll;
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0f,bool overrideDestroyable=false)
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
		collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
        rayCast2D.Position=FlipX(rayCast2D.Position);
        rayCast2D.CastTo=FlipX(rayCast2D.CastTo);
        playerCast.CastTo=FlipX(playerCast.CastTo);
		facing=Facing();
    }

}
