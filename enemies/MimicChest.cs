using Godot;
using System;

public class MimicChest : KinematicMonster
{
    private int cooldown;
    private float shake;
    private float ShakeMax=0.6f;
    private RayCast2D rayCast2D;
    private Vector2 CASTTO;
    private Vector2 snap = new Vector2(0f, 8f);

    public override void _Ready()
    {
        base._Ready();

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));

        animationController.Play("idle");
        animationController.FlipH=MathUtils.randomRangeInt(0,1)!=0;
        EmitSignal(STATE.idle.ToString());

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo*=-1;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity += FORCE * delta;
        velocity = MoveAndSlideWithSnap(velocity, snap, Vector2.Up, false, 4, 0.785398f, true);

        int slides = GetSlideCount();
        for (int i = 0; i < slides; i++)
        {
            if (GetSlideCollision(i).Collider is Node2D node && node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform platform = node as Platform;
                velocity.x += platform.CurrentSpeed.x * 1.8f;
            }
        }

        if(shake!=0f)
        {
            applyShake();
        }

        goal(delta);
    
    }

    protected override void idle(float delta)
    {
        float distance=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);

        if(distance<14f||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId()))
        {
            cooldown=0;
            animationController.Play("attack");
            onAttack(Player.instance);
        }
        else if(cooldown>100) 
        {
            FlipH();
            cooldown=0;
        }
        cooldown++;
    }

    protected override void attack(float delta)
    {
        if(animationController.Frame>1)
        {
            animationController.Play("fight");
            onFight(victim);
        }
    }

    protected override void fight(float delta)
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
                onCalm();
            }
        }
        else
        {
            onCalm();
        }
    }

    protected override void calm(float delta)
    {
        if(animationController.Frame==2)
        {
            onIdle();
        }
    }    

    public override void onPassanger(Player player=null)
    {
        if(state!=STATE.fight)
        {
            base.onPassanger(player);
        }
        else
        {
            player.EmitSignal(STATE.damage.ToString(),DAMAGE_AMOUNT,this);
        }
    }

    protected override void onCalm()
    {
        if(state!=STATE.calm)
        {
            base.onCalm();
            rayCast2D.CastTo=CASTTO;
            animationController.Play("calm");
            victim=null;
            cooldown=0;            
        }
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

    private void applyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.02f)
        {
            float offset=(float)MathUtils.randomRange(-shake,shake);
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
