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
    
    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

        damager=GetNode<Area2D>("Damager");
        damager.Connect("body_entered",this,nameof(onPlayerEntered));
        raycast=GetNode<RayCast2D>(nameof(RayCast2D));
        raycast.Enabled=true;

        GetNode<CPUParticles2D>("Particles1").Emitting=true;
        GetNode<CPUParticles2D>("Particles2").Emitting=true;
        
        EmitSignal(STATE.idle.ToString());

        if(MathUtils.randomRangeInt(1,3)==2)
        {
            FlipH();
        }

    }

    public override void _PhysicsProcess(float delta)
    {   
        goal(delta);
    }

    protected override void idle(float delta)
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

    protected override void stroll(float delta)
    {
        Vector2 direction=getDirection();
        Vector2 force=new Vector2(0,GRAVITY);

        bool left=direction.x<0f;
        bool right=direction.x>0f;

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

    protected override void onDamage(Player player=null, int amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            damager.Monitoring=false;
            base.onDamage(player, amount);
            if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
            {
                animationDirection=-1;
            }
            animationPlayer.Play("HIT");
        }
    }
    
    public override void onPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
            damager.Monitoring=false;
            base.onPassanger(player);
            animationController.Play("idle");
            animationPlayer.Play("PASSANGER");
        }
    }

    protected override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            onStroll();
        }
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
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                onIdle();
            }
        }
    }     

    private void onPlayerEntered(Node body)
    {
        damager.SetDeferred("Monitoring",false);
        body.EmitSignal(STATE.damage.ToString(),1f,this);
    }

    protected override void FlipH()
    {
        if(Scale.y<0)
        {
            LookAt(GlobalPosition+Vector2.Right);
        }
        else
        {
            LookAt(GlobalPosition+Vector2.Left);
        }
        Scale*=new Vector2(1f,-1f);
    }

    private Vector2 getDirection()
    {
        if(Scale.y>0)
        {
            return Vector2.Right;
        }
        return Vector2.Left;
    }

}
