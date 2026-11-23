using Godot;
using System;

public class Cage : KinematicMonster
{
    private const float SHAKE_MIN=0.2f;
    private const float SHAKE_MAX=5f;
    private const float SHAKE_DECAY=0.8f;

    [Export]private bool USE_PHYSICS=true;
    [Export]private bool ONE_WAY_COLL=false;

    private AnimatedSprite princess;
    private float princessTimer=0f;
    
    private float shake=0f;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        princess=GetNode<AnimatedSprite>(nameof(princess));
        princess.Playing=true;


        if(ONE_WAY_COLL)
        {
            CollisionShape2D collision=staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D));
            collision.OneWayCollision=true;
            collision.OneWayCollisionMargin=5f;
        }


        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        UpdateShake();
        goal(delta);
    }

    protected override void Idle(float delta)
    {
        princessTimer+=delta;
        if(princessTimer>5f)
        {
            princessTimer=0f;
            if(MathUtils.RandBool())
            {
                princess.FlipH^=true;
            }
        }
        Navigation(delta);
    }

    protected override void Damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            collisionController.SetDeferred("disabled",false);

            if(health<=0f)
            {
                OnDie();
            }
            else
            {
                OnIdle();
            }            
        }
        Navigation(delta);
    }

    protected override void Navigation(float delta)
    {
        if(USE_PHYSICS)
        {
            base.Navigation(delta);
        }
    }

    protected override void OnIdle()
    {
        if(state!=STATE.idle)
        {
            animationPlayer.Play("SETUP");
            base.OnIdle();
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0f,bool overrideDestroyable=false)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            if(node==null)
            {
                node=this;
            }
            else if(node is Player)
            {
                attacker=Player.instance;
            }

            Renderer.instance.Shake(1f);
            shake=SHAKE_MAX;

            lastState=state;
            state=STATE.damage;

            if(DESTORYABLE||overrideDestroyable)
            {
                health-=amount;
                if(amount!=0f&&health>0f)
                {
                    HealthNotifier(health);
                }
            }

            collisionController.SetDeferred("disabled",true);
            animationPlayer.Play("HIT");
            goal=Damage;
        }
        
    }

    private void UpdateShake()
    {
        if(state==STATE.damage)
        {
            Vector2 offset=new Vector2
            (
                MathUtils.RandomRange(-shake,shake),
                MathUtils.RandomRange(-shake,shake)

            );
            animationController.Offset=offset;
            
            shake=Mathf.Max(shake*SHAKE_DECAY,SHAKE_MIN);
        }
        else if(animationController.Offset!=Vector2.Zero)
        {
            animationController.Offset=Vector2.Zero;
            shake=0f;
        }
    }

}
