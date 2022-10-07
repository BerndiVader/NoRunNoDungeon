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
    protected CollisionShape2D collisionController, statCollShape;
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
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        victim=null;
        collisionController=GetNode<CollisionShape2D>("CollisionShape2D");
        statCollShape=GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D");
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
    }

    protected virtual void tick(float delta)
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

    protected virtual void idle(float delta)
    {

    }
    protected virtual void stroll(float delta)
    {

    }
    protected virtual void attack(float delta)
    {

    }
    protected virtual void fight(float delta)
    {
        state=lastState;
    }
    protected virtual void passanger(float delta)
    {
        state=health<=0?state=STATE.DIE:state=lastState;
    }
    protected virtual void calm(float delta)
    {

    }
    protected virtual void damage(float delta)
    {
        lastState=state;
        state=STATE.DIE;

    }

    protected virtual void die(float delta)
    {
        EnemieDieParticles particles=(EnemieDieParticles)ResourceUtils.particles[(int)PARTICLES.ENEMIEDIEPARTICLES].Instance();
        particles.Texture=animationController.Frames.GetFrame(animationController.Animation,animationController.Frame);

        RectangleShape2D shape2D=(RectangleShape2D)collisionController.Shape;
        Vector2 position=getPosition()+collisionController.Position;
        position.y+=shape2D.Extents.y;
        particles.Position=position;

        World.instance.level.AddChild(particles);
        QueueFree();
    }

    protected virtual void onDie()
    {
        lastState=state;
        state=STATE.DIE;
    }
    protected virtual void onAttack(Player player)
    {
        lastState=state;
        state=STATE.ATTACK;
        victim=player;
    }
    protected virtual void onFight(Player player)
    {
        lastState=state;
        state=STATE.FIGHT;
        victim=player;
    }
    protected virtual void onDamage(Player player,int amount)
    {
        World.instance.renderer.shake=2d;

        statCollShape.SetDeferred("disabled",true);
        lastState=state;
        state=STATE.DAMAGE;
        attacker=player;
        damageAmount=amount;
        health-=amount;
    }

    protected virtual void onPassanger(Player player)
    {
        World.instance.renderer.shake=2d;

        lastState=state;
        state=STATE.PASSANGER;
        attacker=player;
        health--;
    }
    protected virtual void onCalm()
    {
        lastState=state;
        state=STATE.CALM;
    }
    protected virtual void onIdle()
    {
        lastState=state;
        state=STATE.IDLE;
        animationController.Play("default");
    }
    protected virtual void onStroll()
    {
        lastState=state;
        state=STATE.STROLL;
        animationController.Play("run");
    }

    protected virtual Vector2 getPosition()
    {
        return World.instance.level.ToLocal(GlobalPosition);
    }

    protected void onExitedScreen()
    {
        QueueFree();
    }

    protected void onAnimationPlayerStarts(String name)
    {
        startOffset=Position;
    }
    protected void onAnimationPlayerEnded(String name)
    {
        startOffset=Position;
        ANIMATION_OFFSET=Vector2.Zero;
        animationDirection=1;
    }

}
