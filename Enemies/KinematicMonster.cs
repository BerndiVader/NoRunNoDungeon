using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    [Export] public Vector2 ANIMATION_OFFSET=Vector2.Zero;
    [Signal]
    public delegate void Die();
    [Signal]
    public delegate void Attack(Player player);
    [Signal]
    public delegate void Fight(Player player);
    [Signal]
    public delegate void Damage(Player player,float amount);
    [Signal]
    public delegate void Passanger(Player player);
    [Signal]
    public delegate void Calm();
    [Signal]
    public delegate void Idle();
    
    public STATE state,lastState;
    protected Player victim,attacker;
    protected float damageAmount;
    protected Placeholder parent;
    public AnimatedSprite animationController;
    protected Godot.AnimationPlayer animationPlayer;
    protected Vector2 startOffset=Vector2.Zero;
    protected int animationDirection=1,health=1;

    public override void _Ready()
    {
        Connect("Passanger",this,nameof(onPassanger));
        Connect("Die",this,nameof(onDie));
        Connect("Attack",this,nameof(onAttack));
        Connect("Fight",this,nameof(onFight));
        Connect("Calm",this,nameof(onCalm));
        Connect("Idle",this,nameof(onIdle));
        Connect("Damage",this,nameof(onDamage));

        AddToGroup("Enemies",true);

        victim=null;
    }

    public virtual void tick(float delta)
    {
        switch(state)
        {
            case STATE.IDLE:
            {
                idle(delta);
                break;
            }
            case STATE.ATTACK:
            {
                attack(delta);
                break;
            }
            case STATE.FIGHT:
            {
                fight(delta);
                break;
            }
            case STATE.DAMAGE:
            {
                damage(delta);
                break;
            }
            case STATE.PASSANGER:
            {
                passanger(delta);
                break;
            }
            case STATE.CALM:
            {
                calm(delta);
                break;
            }
            case STATE.DIE:
            {
                die(delta);
                break;
            }
        }
    }

    public virtual void idle(float delta)
    {

    }
    public virtual void attack(float delta)
    {

    }
    public virtual void fight(float delta)
    {
        state=lastState;
    }
    public virtual void passanger(float delta)
    {
        state=health<=0?state=STATE.DIE:state=lastState;
    }
    public virtual void calm(float delta)
    {

    }
    public virtual void damage(float delta)
    {
        lastState=state;
        state=STATE.DIE;

    }

    public virtual void die(float delta)
    {
        EnemieDieParticles particles=(EnemieDieParticles)ResourceUtils.particles[(int)PARTICLES.ENEMIEDIEPARTICLES].Instance();
        particles.Texture=animationController.Frames.GetFrame(animationController.Animation,animationController.Frame);

        CollisionShape2D collision=GetNode<CollisionShape2D>("CollisionShape2D");
        RectangleShape2D shape2D=(RectangleShape2D)collision.Shape;
        Vector2 position=getPosition()+collision.Position;
        position.y+=shape2D.Extents.y;
        particles.Position=position;

        WorldUtils.world.level.CallDeferred("add_child",particles);
        _Free();
    }

    public virtual void onDie()
    {
        lastState=state;
        state=STATE.DIE;
    }
    public virtual void onAttack(Player player)
    {
        lastState=state;
        state=STATE.ATTACK;
        victim=player;
    }
    public virtual void onFight(Player player)
    {
        lastState=state;
        state=STATE.FIGHT;
        victim=player;
    }
    public virtual void onDamage(Player player,int amount)
    {
        lastState=state;
        state=STATE.DAMAGE;
        attacker=player;
        damageAmount=amount;
        health-=amount;
    }
    public virtual void onPassanger(Player player)
    {
        lastState=state;
        state=STATE.PASSANGER;
        attacker=player;
        health--;
    }
    public virtual void onCalm()
    {
        lastState=state;
        state=STATE.CALM;
    }
    public virtual void onIdle()
    {
        lastState=state;
        state=STATE.IDLE;
    }

    public virtual Vector2 getPosition()
    {
        return WorldUtils.world.level.ToLocal(GlobalPosition);
//        return parent!=null?parent.Position+Position:Position;
    }

    public virtual void _Free()
    {
        if(parent!=null)
        {
            parent.CallDeferred("queue_free");
        }
        else
        {
            CallDeferred("queue_free");
        }
    }
    public void animationPlayerStarts(String name)
    {
        startOffset=Position;
    }
    public void animationPlayerEnded(String name)
    {
        startOffset=Position;
        ANIMATION_OFFSET=Vector2.Zero;
        animationDirection=1;
    }

}
