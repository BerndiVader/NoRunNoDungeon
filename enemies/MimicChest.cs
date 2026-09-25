using Godot;
using System;

public class MimicChest : KinematicMonster
{
    [Export] private float SHAKE_MAX=0.6f;
    [Export] private float DAMAGE_RANGE=14f;
 
    private RayCast2D rayCast2D;
    private CPUParticles2D aura;
    private ShaderMaterial shader;

    private Vector2 castTo;
    private int cooldown=0;
    private float shake;
    private float damageRangeSqrd;

    public override void _Ready()
    {
        base._Ready();

        damageRangeSqrd=DAMAGE_RANGE*DAMAGE_RANGE;

        shader=(ShaderMaterial)animationController.Material;

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        

        aura=GetNode<CPUParticles2D>("Aura");
        aura.Emitting=false;

		animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
		animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
		animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));        

        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animationController.Play("idle");
        animationController.FlipH=MathUtils.RandBool();

        castTo=rayCast2D.CastTo;

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
        shader.SetShaderParam("direction",Mathf.Sign(rayCast2D.CastTo.x));

    }

    protected override void Idle(float delta)
    {
        if(GlobalPosition.DistanceSquaredTo(Player.instance.GlobalPosition)<damageRangeSqrd
        ||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId()))
        {
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
            aura.Restart();
            OnFight(victim);
        }
        Navigation(delta);
    }

    protected override void Fight(float delta)
    {
        float distance=GlobalPosition.DistanceSquaredTo(victim.GlobalPosition);

        if(distance<damageRangeSqrd)
        {
            victim.EmitSignal(STATE.damage.ToString(),this,DAMAGE_AMOUNT,false);
        }
        else if(distance<10000f)
        {
            Vector2 direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
            rayCast2D.CastTo=direction*Mathf.Sqrt(distance);

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
        base.Passanger(delta);
	}      

    protected override void OnPassanger(Player player=null)
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
            player.EmitSignal(STATE.damage.ToString(),this,DAMAGE_AMOUNT,false);
        }
    }

    protected override void OnCalm()
    {
        if(state!=STATE.calm)
        {
            base.OnCalm();
            aura.Emitting=false;
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
        shake=Math.Min(shake,SHAKE_MAX);
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
