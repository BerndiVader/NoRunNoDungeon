using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    private static PackedScene levelControlPack;

    static KinematicMonster()
    {
        levelControlPack=ResourceLoader.Load<PackedScene>("res://level/LevelControl.tscn");
    }

    [Export] protected Vector2 ANIMATION_OFFSET=Vector2.Zero,velocity=Vector2.Zero;
    [Export] protected float damageAmount=1f;
    [Export] protected float health=1f;
    [Export] protected float GRAVITY=500f;
    [Export] protected float friction=0.01f;
    [Export] protected Godot.Collections.Dictionary<string,object> LEVEL_SETTINGS=new Godot.Collections.Dictionary<string,object>()
    {
        {"Use",false},
        {"Speed",-1.0f},
        {"Zoom",-1.0f},
    };

    private Settings levelSettings;
    
    protected Player victim,attacker;
    protected Godot.AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected StaticBody2D staticBody;
    protected Vector2 startOffset=Vector2.Zero,force;
    protected int animationDirection=1;

    public STATE state;
    protected STATE lastState;
    protected delegate void Goal(float delta);
    protected Goal goal;
    protected bool onDelay=false;
    public AnimatedSprite animationController;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(true);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        collisionController=GetNode<CollisionShape2D>("CollisionShape2D");
        staticBody=GetNode<StaticBody2D>("StaticBody2D");
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");

        staticBody.AddUserSignal(STATE.passanger.ToString());
        staticBody.AddUserSignal(STATE.damage.ToString());
        staticBody.Connect(STATE.passanger.ToString(),this,nameof(onPassanger));
        staticBody.Connect(STATE.damage.ToString(),this,nameof(onDamage));

        AddUserSignal(STATE.passanger.ToString());
        AddUserSignal(STATE.damage.ToString());
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
        Connect(STATE.passanger.ToString(),this,nameof(onPassanger));
        Connect(STATE.damage.ToString(),this,nameof(onDamage));

        AddToGroup(GROUPS.ENEMIES.ToString());
        staticBody.AddToGroup(GROUPS.ENEMIES.ToString());

        attacker=victim=null;
        force=new Vector2(0f,GRAVITY);

        state=STATE.unknown;
        goal=unknown;
    }

    protected virtual void unknown(float delta) {}
    protected virtual void idle(float delta) {}
    protected virtual void stroll(float delta) {}
    protected virtual void attack(float delta) {}
    protected virtual void fight(float delta) {}
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
    protected virtual void calm(float delta) {}
    protected virtual void damage(float delta)
    {
        if(health<=0)
        {
            onDie();
        }
        else
        {
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).Disabled=false;
            EmitSignal(STATE.idle.ToString());
        }
    }
    protected virtual void die(float delta)
    {
        EnemieDieParticles particles=(EnemieDieParticles)ResourceUtils.particles[(int)PARTICLES.ENEMIEDIE].Instance();
        particles.Texture=animationController.Frames.GetFrame(animationController.Animation,animationController.Frame);

        Vector2 position=GlobalPosition;
        position.y+=particles.Texture.GetWidth()/2;
        particles.Position=World.level.ToLocal(position);

        World.level.AddChild(particles);
        QueueFree();
    }

    protected virtual void onDie()
    {
        onDelay=false;
        if(state!=STATE.die)
        {
            lastState=state;
            state=STATE.die;
            goal=die;
        }
    }
    protected virtual void onAttack(Player player)
    {
        onDelay=false;
        if(state!=STATE.attack)
        {
            lastState=state;
            state=STATE.attack;
            victim=player;
            goal=attack;
        }
    }
    protected virtual void onFight(Player player)
    {
        onDelay=false;
        if(state!=STATE.fight)
        {
            lastState=state;
            state=STATE.fight;
            victim=player;
            goal=fight;
        }
    }
    protected virtual void onDamage(Player player,int amount)
    {
        onDelay=false;
        if(state!=STATE.damage&&state!=STATE.die)
        {
            World.instance.renderer.shake=2d;
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",true);
            lastState=state;
            state=STATE.damage;
            attacker=player;
            damageAmount=amount;
            health-=amount;
            goal=damage;
        }
    }
    public virtual void onPassanger(Player player)
    {
        onDelay=false;
        if(state!=STATE.passanger)
        {
            World.instance.renderer.shake=2d;
            lastState=state;
            state=STATE.passanger;
            attacker=player;
            health--;
            goal=passanger;
        }
    }
    protected virtual void onCalm()
    {
        onDelay=false;
        if(state!=STATE.calm)
        {
            lastState=state;
            state=STATE.calm;
            goal=calm;
        }
    }
    protected virtual void onIdle()
    {
        onDelay=false;
        if(state!=STATE.idle)
        {
            lastState=state;
            state=STATE.idle;
            animationController.Play("idle");
            goal=idle;
        }
    }
    protected virtual void onStroll()
    {
        onDelay=false;
        if(state!=STATE.stroll)
        {
            lastState=state;
            state=STATE.stroll;
            animationController.Play("stroll");
            goal=stroll;
        }
    }

    protected virtual void delayedState(float seconds,STATE next,params object[] args)
    {
        if(!onDelay)
        {
            onDelay=true;
            char[]chr=next.ToString().ToCharArray();
            chr[0]=char.ToUpper(chr[0]);
            string method="on"+new string(chr);

            SceneTreeTimer timer=GetTree().CreateTimer(seconds,false);
            timer.Connect("timeout",this,method,new Godot.Collections.Array(args));
        }
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

    public override void _EnterTree()
    {
        if((bool)LEVEL_SETTINGS["Use"])
        {
            levelSettings=new Settings(World.level,(float)LEVEL_SETTINGS["Speed"],(float)LEVEL_SETTINGS["Zoom"]);
            LevelControl control=levelControlPack.Instance<LevelControl>();
            control.setMonsterControlled(levelSettings);
            control.Position=Position;
            World.level.AddChild(control);
        }
        base._EnterTree();
    }

    public override void _ExitTree()
    {
        if((bool)LEVEL_SETTINGS["Use"])
        {
            levelSettings.restore();
        }
        base._ExitTree();
    }

}
