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
    [Signal]
    public delegate void Stroll();
    
    protected Player victim,attacker;
    [Export] public float damageAmount=1f;
    protected Placeholder parent;
    protected VisibilityNotifier2D notifier2D;
    protected Godot.AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected Vector2 startOffset=Vector2.Zero;
    protected int animationDirection=1,health=1;

    public STATE state,lastState;
    public AnimatedSprite animationController;

    public override void _Ready()
    {
        Connect(SIGNALS.Passanger.ToString(),this,nameof(onPassanger));
        Connect(SIGNALS.Die.ToString(),this,nameof(onDie));
        Connect(SIGNALS.Attack.ToString(),this,nameof(onAttack));
        Connect(SIGNALS.Fight.ToString(),this,nameof(onFight));
        Connect(SIGNALS.Calm.ToString(),this,nameof(onCalm));
        Connect(SIGNALS.Idle.ToString(),this,nameof(onIdle));
        Connect(SIGNALS.Damage.ToString(),this,nameof(onDamage));
        Connect(SIGNALS.Stroll.ToString(),this,nameof(onStroll));

        AddToGroup(GROUPS.ENEMIES.ToString());

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(exitedScreen));
        AddChild(notifier2D);

        victim=null;
        collisionController=(CollisionShape2D)GetNode("CollisionShape2D");
        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
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
            case STATE.STROLL:
            {
                stroll(delta);
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
    public virtual void stroll(float delta)
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

        RectangleShape2D shape2D=(RectangleShape2D)collisionController.Shape;
        Vector2 position=getPosition()+collisionController.Position;
        position.y+=shape2D.Extents.y;
        particles.Position=position;

        WorldUtils.world.level.CallDeferred("add_child",particles);
        CallDeferred("queue_free");
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
        WorldUtils.world.renderer.shake=2d;

        GetNode<StaticBody2D>("StaticBody2D").GetNode<CollisionShape2D>("CollisionShape2D").CallDeferred("set","disabled",true);
        lastState=state;
        state=STATE.DAMAGE;
        attacker=player;
        damageAmount=amount;
        health-=amount;
    }

    public virtual void onPassanger(Player player)
    {
        WorldUtils.world.renderer.shake=2d;

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
        animationController.Play("default");
    }
    public virtual void onStroll()
    {
        lastState=state;
        state=STATE.STROLL;
        animationController.Play("run");
    }

    public virtual Vector2 getPosition()
    {
        return WorldUtils.world.level.ToLocal(GlobalPosition);
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
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
