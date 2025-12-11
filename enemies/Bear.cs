using Godot;
using System;

public class Bear : KinematicMonster
{
    [Export] private new Vector2 ANIMATION_OFFSET=new Vector2(0f,0f);
    [Export] private float WALK_FORCE=100f;
    [Export] private float WALK_MIN_SPEED=2f;
    [Export] private float WALK_MAX_SPEED=50f;
    private Vector2 snap=new Vector2(0f,8f);
    private Area2D damager;
    private RayCast2D raycast;
    private CPUParticles2D particles1,particles2;
    
    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        damager=GetNode<Area2D>("Damager");
        damager.Connect("body_entered",this,nameof(OnPlayerEntered));
        raycast=GetNode<RayCast2D>(nameof(RayCast2D));
        raycast.Enabled=true;

        particles1=GetNode<CPUParticles2D>("Particles1");
        particles2=GetNode<CPUParticles2D>("Particles2");
        particles1.Emitting=true;
        particles2.Emitting=true;
        
        EmitSignal(STATE.stroll.ToString());

        if(MathUtils.RandBool())
        {
            FlipH();
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        goal(delta);
    }

    protected override void Idle(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

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
    }

    protected override void Stroll(float delta)
    {
        Vector2 force=new Vector2(0,GRAVITY);

        bool left=facing==Vector2.Left;
        bool right=facing==Vector2.Right;

        if(left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        }
        else if(right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
        {
            force.x+=WALK_FORCE;
        }
        else
        {
            velocity=StopX(velocity,delta);
        }

        velocity+=force*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        if(IsOnFloor())
        {
            velocity-=GetFloorVelocity()*delta;
        }

        if(IsOnWall()||!raycast.IsColliding())
        {
            FlipH();
        }
    }

    protected override void OnDamage(Player player=null, int amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            damager.Monitoring=false;
            base.OnDamage(player, amount);
            if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
            {
                animationDirection=-1;
            }
            animationPlayer.Play("HIT");
        }
    }
    
    public override void OnPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
            damager.Monitoring=false;
            base.OnPassanger(player);
            animationController.Play("idle");
            animationPlayer.Play("PASSANGER");
        }
    }

    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0f)
            {
                OnDie();
            }
            else
            {
                animationController.SpeedScale=1;
                damager.Monitoring=true;
                OnStroll();
            }
        }
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
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                OnIdle();
            }
        }
    }     

    private void OnPlayerEntered(Node body)
    {
        damager.SetDeferred("Monitoring",false);
        body.EmitSignal(STATE.damage.ToString(),1f,this);
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        damager.Position=FlipX(damager.Position);
        raycast.Position=FlipX(raycast.Position);
        collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
        particles1.Position=FlipX(particles1.Position);
        particles2.Position=FlipX(particles2.Position);

        facing=Facing();
    }
}
