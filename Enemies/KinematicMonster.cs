using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{

    [Signal]
    public delegate void Die();
    [Signal]
    public delegate void Attack(Player player);
    [Signal]
    public delegate void Fight(Player player);
    [Signal]
    public delegate void Calm();
    [Signal]
    public delegate void Idle();
    
    public STATE state;
    protected Player victim;
    protected Placeholder parent;
    protected AnimatedSprite animationController;

    public override void _Ready()
    {
        Connect("Die",this,nameof(onDie));
        Connect("Attack",this,nameof(onAttack));
        Connect("Fight",this,nameof(onFight));
        Connect("Calm",this,nameof(onCalm));
        Connect("Idle",this,nameof(onIdle));

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

    }
    public virtual void calm(float delta)
    {

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
        state=STATE.DIE;
    }
    public virtual void onAttack(Player player)
    {
        state=STATE.ATTACK;
        victim=player;
    }
    public virtual void onFight(Player player)
    {
        state=STATE.FIGHT;
        victim=player;
    }
    public virtual void onCalm()
    {
        state=STATE.CALM;
    }
    public virtual void onIdle()
    {
        state=STATE.IDLE;
    }

    public virtual Vector2 getPosition()
    {
        return parent!=null?parent.Position+Position:Position;
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


}
