using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    [Export] protected Vector2 ANIMATION_OFFSET=Vector2.Zero;
    [Export] protected float damageAmount=1f;
    [Export] protected float health=1f;
    [Export] protected float GRAVITY=500f;
    [Export] protected float friction=0.01f;
    
    protected Player victim,attacker;
    protected Godot.AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected StaticBody2D staticBody;
    protected Vector2 startOffset=Vector2.Zero,force;
    protected int animationDirection=1;

    public STATE state;
    protected STATE lastState;
    public AnimatedSprite animationController;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        collisionController=GetNode<CollisionShape2D>("CollisionShape2D");
        staticBody=GetNode<StaticBody2D>("StaticBody2D");
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");

        staticBody.AddUserSignal(STATE.passanger.ToString());
        staticBody.AddUserSignal(STATE.damage.ToString());
        staticBody.Connect(STATE.passanger.ToString(),this,nameof(onPassanger));
        staticBody.Connect(STATE.damage.ToString(),this,nameof(onDamage));

        AddUserSignal(STATE.die.ToString());
        AddUserSignal(STATE.attack.ToString());
        AddUserSignal(STATE.fight.ToString());
        AddUserSignal(STATE.calm.ToString());
        AddUserSignal(STATE.idle.ToString());
        AddUserSignal(STATE.stroll.ToString());                

        Connect(STATE.die.ToString(),this,nameof(onDie));
        Connect(STATE.attack.ToString(),this,nameof(onAttack));
        Connect(STATE.fight.ToString(),this,nameof(onFight));
        Connect(STATE.calm.ToString(),this,nameof(onCalm));
        Connect(STATE.idle.ToString(),this,nameof(onIdle));
        Connect(STATE.stroll.ToString(),this,nameof(onStroll));

        AddToGroup(GROUPS.ENEMIES.ToString());
        staticBody.AddToGroup(GROUPS.ENEMIES.ToString());

        victim=null;
        force=new Vector2(0f,GRAVITY);
    }

    protected virtual void tick(float delta)
    {
        switch(state)
        {
            case(STATE.attack):
                attack(delta);
                break;
            case(STATE.calm):
                calm(delta);
                break;
            case(STATE.damage):
                damage(delta);
                break;
            case(STATE.die):
                die(delta);
                break;
            case(STATE.fight):
                fight(delta);
                break;
            case(STATE.idle):
                idle(delta);
                break;
            case(STATE.passanger):
                passanger(delta);
                break;
            case(STATE.stroll):
                stroll(delta);
                break;
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
        if(health<=0)
        {
            onDie();
        }
        else if(state!=lastState)
        {
            EmitSignal(lastState.ToString());
        }
        else
        {
            animationController.SpeedScale=1;
            onIdle();
        }
    }
    protected virtual void calm(float delta)
    {

    }
    protected virtual void damage(float delta)
    {
        onDie();
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
        if(state!=STATE.die)
        {
            lastState=state;
            state=STATE.die;
        }
    }
    protected virtual void onAttack(Player player)
    {
        if(state!=STATE.attack)
        {
            lastState=state;
            state=STATE.attack;
            victim=player;
        }
    }
    protected virtual void onFight(Player player)
    {
        if(state!=STATE.fight)
        {
            lastState=state;
            state=STATE.fight;
            victim=player;
        }
    }
    protected virtual void onDamage(Player player,int amount)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            World.instance.renderer.shake=2d;
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",true);
            lastState=state;
            state=STATE.damage;
            attacker=player;
            damageAmount=amount;
            health-=amount;
        }
    }

    public virtual void onPassanger(Player player)
    {
        if(state!=STATE.passanger)
        {
            World.instance.renderer.shake=2d;
            lastState=state;
            state=STATE.passanger;
            attacker=player;
            health--;
        }
    }
    protected virtual void onCalm()
    {
        if(state!=STATE.calm)
        {
            lastState=state;
            state=STATE.calm;
        }
    }
    protected virtual void onIdle()
    {
        if(state!=STATE.idle)
        {
            lastState=state;
            state=STATE.idle;
            animationController.Play("idle");
        }
    }
    protected virtual void onStroll()
    {
        if(state!=STATE.stroll)
        {
            lastState=state;
            state=STATE.stroll;
            animationController.Play("stroll");
        }
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
