using Godot;

public class KnifeGoblin : KinematicMonster
{
    [Export] private float ACTIVATION_DISTANCE=80f;
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=20f;
    [Export] private float WALK_MAX_SPEED=120f;

    private RayCast2D rayCast2D;
    private MonsterWeapon weapon;


    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;

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
        if(DistanceToPlayer()<ACTIVATION_DISTANCE)
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
		velocity=MoveAndSlideWithSnap(velocity,justDamaged?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        
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
 
    private float DistanceToPlayer()
    {
        return GlobalPosition.DistanceTo(Player.instance.GlobalPosition);
    }

    protected override void FlipH()
    {
		animationController.FlipH^=true;
		collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
        rayCast2D.Position=FlipX(rayCast2D.Position);
        rayCast2D.CastTo=FlipX(rayCast2D.CastTo);
		facing=Facing();
    }

}
