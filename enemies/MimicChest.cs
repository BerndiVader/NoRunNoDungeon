using Godot;
using System;

public class MimicChest : KinematicMonster
{
    private int cooldown;
    private float shake;
    private float ShakeMax=0.6f;
    private RayCast2D rayCast2D;
    private Vector2 castTo;

    public override void _Ready()
    {
        base._Ready();

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        castTo=rayCast2D.CastTo;

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));        

        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animationController.Play("idle");
        animationController.FlipH=MathUtils.RandBool();

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo*=-1;
        }

        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if(shake!=0f)
        {
            ApplyShake();
        }

        goal(delta);
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
        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        if(animationController.Frame>1)
        {
            animationController.Play("fight");
            OnFight(victim);
        }
        Navigation(delta);
    }

    protected override void Fight(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);

        if(distance<14f)
        {
            victim.EmitSignal(STATE.damage.ToString(),this,DAMAGE_AMOUNT);
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
        Navigation(delta);
    }

    protected override void Calm(float delta)
    {
        if(animationController.Frame==2)
        {
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
            player.EmitSignal(STATE.damage.ToString(),this,DAMAGE_AMOUNT);
        }
    }

    protected override void OnCalm()
    {
        if(state!=STATE.calm)
        {
            base.OnCalm();
            rayCast2D.CastTo=castTo;
            animationController.Play("calm");
            victim=null;
            cooldown=0;            
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
