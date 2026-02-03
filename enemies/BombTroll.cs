using System.Collections.Generic;
using Godot;

public class BombTroll : KinematicMonster
{
    private static PackedScene bombPack=Cannon.bombPack;

    [Export] private float ACTIVATION_DISTANCE=80f;
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=10f;
    [Export] private float WALK_MAX_SPEED=60f;
    [Export] private double THROW_DELAY_MS=1000d;
    [Export] private float CANNON_INITIAL_FORCE=100f;
    [Export] private Dictionary<string,object>CANNONBALL_SETTINGS=Cannonball.GetDefaults();

    private RayCast2D rayCast2D;
    private AnimatedSprite bomb;
    private double timestamp;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;

        bomb=GetNode<AnimatedSprite>(nameof(Sprite));
        bomb.Visible=false;
        bomb.Stop();

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
            else if(!rayCast2D.IsColliding())
            {
                OnAlert();
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

    protected override void Alert(float delta)
    {
        if(Time.GetTicksMsec()-timestamp>=THROW_DELAY_MS)
        {
            ThrowBomb();
            bomb.Visible=false;
            bomb.Stop();
            OnIdle();
        }
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
        Navigation(delta);
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

    protected override void OnAlert()
    {
        onDelay=false;
        if(state!=STATE.alert)
        {
            lastState=state;
            state=STATE.alert;
            bomb.Visible=true;
            bomb.Play("default");
            timestamp=Time.GetTicksMsec();
            goal=Alert;
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

    private void ThrowBomb()
    {
        Cannonball ball=bombPack.Instance<Cannonball>();
        ball.Position=World.level.ToLocal(bomb.GlobalPosition);
        ball.SetDirection(facing);
        if((bool)CANNONBALL_SETTINGS["USE_SETTINGS"])
        {
            ball.SetOptions(CANNONBALL_SETTINGS);
        }
        ball.Scale=new Vector2(0.7f,0.7f);
        ball.INITIAL_FORCE=CANNON_INITIAL_FORCE;
        World.level.AddChild(ball);
    }

    protected override void FlipH()
    {
		animationController.FlipH^=true;
		collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
        rayCast2D.Position=FlipX(rayCast2D.Position);
        rayCast2D.CastTo=FlipX(rayCast2D.CastTo);
        bomb.Position=FlipX(bomb.Position);
		facing=Facing();
    }

}
