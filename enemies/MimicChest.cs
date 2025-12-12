using Godot;
using System;

public class MimicChest : KinematicMonster
{
    private int cooldown;
    private float shake;
    private float ShakeMax=0.6f;
    private RayCast2D rayCast2D;
    private Vector2 CASTTO;

    public override void _Ready()
    {
        base._Ready();

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));        

        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animationController.Play("idle");
        animationController.FlipH=MathUtils.RandomRangeInt(0,1)!=0;

        EmitSignal(STATE.idle.ToString());

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo*=-1;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if(shake!=0f)
        {
            ApplyShake();
        }

        goal(delta);
        Navigation(delta);
    }

    protected override void Idle(float delta)
    {
        float distance=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);

        if(distance<14f||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId()))
        {
            cooldown=0;
            animationController.Play("attack");
            OnAttack(Player.instance);
        }
        else if(cooldown>100) 
        {
            FlipH();
            cooldown=0;
        }
        cooldown++;
    }

    protected override void Attack(float delta)
    {
        if(animationController.Frame>1)
        {
            animationController.Play("fight");
            OnFight(victim);
        }
    }

    protected override void Fight(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);

        if(distance<14f)
        {
            victim.EmitSignal(STATE.damage.ToString(),DAMAGE_AMOUNT,this);
        }
        else if(distance<100f)
        {
            Vector2 direction=new Vector2(GlobalPosition.DirectionTo(victim.GlobalPosition));
            rayCast2D.CastTo=direction*distance;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                shake=0.1f;
            }
            else
            {
                OnCalm();
            }
        }
        else
        {
            OnCalm();
        }
    }

    protected override void Calm(float delta)
    {
        if(animationController.Frame==2)
        {
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
        if(state!=STATE.attack)
        {
            if(state!=STATE.passanger)
            {
                base.OnPassanger(player);
                animationPlayer.Play("PASSANGER");
            }
        }
        else
        {
            player.EmitSignal(STATE.damage.ToString(),DAMAGE_AMOUNT,this);
        }
    }

    protected override void OnCalm()
    {
        if(state!=STATE.calm)
        {
            base.OnCalm();
            rayCast2D.CastTo=CASTTO;
            animationController.Play("calm");
            victim=null;
            cooldown=0;            
        }
    }

    protected override void Navigation(float delta)
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

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
        facing=Facing();
    }

    private void ApplyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.02f)
        {
            float offset=(float)MathUtils.RandomRange(-shake,shake);
            Rotation=offset;
            shake*=0.9f;
        } 
        else if(shake>0f)
        {
            shake=0f;
            Rotation=0;
        }
    }


}
