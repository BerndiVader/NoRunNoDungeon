using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    private static PackedScene levelControlPack=ResourceLoader.Load<PackedScene>("res://level/LevelControl.tscn");

    [Export] protected Vector2 ANIMATION_OFFSET = Vector2.Zero;
    [Export] protected Vector2 VELOCITY=Vector2.Zero;
    [Export] protected float DAMAGE_AMOUNT=1f;
    [Export] protected float HEALTH=1f;
    [Export] protected float GRAVITY=500f;
    [Export] protected float FRICTION=0.01f;
    [Export] protected Godot.Collections.Dictionary<string,object> LEVEL_SETTINGS=new Godot.Collections.Dictionary<string,object>()
    {
        {"Use",false},
        {"Dir",Vector2.Zero},
        {"Speed",-1.0f},
        {"Zoom",-1.0f},
    };

    private Settings levelSettings;
    
    protected Player victim,attacker;
    protected AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected StaticBody2D staticBody;
    public AnimatedSprite animationController;

    protected float health;
    protected Vector2 startOffset = Vector2.Zero;
    protected int animationDirection = 1;
    protected Vector2 FORCE;
    protected Vector2 velocity,direction,lastDirection,facing;
    public STATE state,lastState;
    protected delegate void Goal(float delta);
    protected Goal goal;
    protected bool onDelay=false;

    public override void _Ready()
    {
        FORCE = new Vector2(0f, GRAVITY);
        health = HEALTH;
        velocity = VELOCITY;
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
        AddUserSignal(STATE.panic.ToString());
        AddUserSignal(STATE.alert.ToString());

        Connect(STATE.die.ToString(),this,nameof(onDie));
        Connect(STATE.attack.ToString(),this,nameof(onAttack));
        Connect(STATE.fight.ToString(),this,nameof(onFight));
        Connect(STATE.calm.ToString(),this,nameof(onCalm));
        Connect(STATE.idle.ToString(),this,nameof(onIdle));
        Connect(STATE.stroll.ToString(),this,nameof(onStroll));
        Connect(STATE.passanger.ToString(),this,nameof(onPassanger));
        Connect(STATE.damage.ToString(),this,nameof(onDamage));
        Connect(STATE.panic.ToString(),this,nameof(onPanic));
        Connect(STATE.alert.ToString(),this,nameof(onAlert));

        AddToGroup(GROUPS.ENEMIES.ToString());
        staticBody.AddToGroup(GROUPS.ENEMIES.ToString());

        attacker=victim=null;
        FORCE=new Vector2(0f,GRAVITY);

        state=STATE.unknown;
        goal = unknown;

        facing = animationController.FlipH ? Vector2.Left : Vector2.Right;
        direction = new Vector2(facing);
    }

    protected virtual void unknown(float delta) {}
    protected virtual void idle(float delta) {}
    protected virtual void stroll(float delta) {}
    protected virtual void attack(float delta) {}
    protected virtual void fight(float delta) {}
    protected virtual void panic(float delta) {}
    protected virtual void alert(float delta) {}
    protected virtual void passanger(float delta)
    {
        if(health<=0)
        {
            onDie();
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
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
            onIdle();
        }
    }
    protected virtual void die(float delta)
    {
        EnemieDieParticles particles=ResourceUtils.particles[(int)PARTICLES.ENEMIEDIE].Instance<EnemieDieParticles>();
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
    protected virtual void onAttack(Player player=null)
    {
        if(player==null)
        {
            player = Player.instance;
        }
        onDelay=false;
        if(state!=STATE.attack)
        {
            lastState=state;
            state=STATE.attack;
            victim=player;
            goal=attack;
        }
    }
    protected virtual void onFight(Player player=null)
    {    
        onDelay=false;
        if(state!=STATE.fight)
        {
            if (player == null)
            {
                player = Player.instance;
            }                
            
            lastState=state;
            state=STATE.fight;
            victim=player;
            goal=fight;
        }
    }
    protected virtual void onDamage(Player player=null,int amount=0)
    {
        onDelay=false;
        if(state!=STATE.damage&&state!=STATE.die)
        {
            if (player == null)
            {
                player = Player.instance;
            }
            World.instance.renderer.shake=2d;
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",true);
            lastState=state;
            state=STATE.damage;
            attacker=player;
            DAMAGE_AMOUNT = amount;
            
            health-=amount;
            goal = damage;
        }
    }
    public virtual void onPassanger(Player player=null)
    {      
        onDelay=false;
        if(state!=STATE.passanger)
        {
            if (player == null)
            {
                player = Player.instance;
            }

            World.instance.renderer.shake=2d;
            lastState=state;
            state = STATE.passanger;
            health -= 0.5f;
            attacker = player;
            goal=passanger;
        }
    }
    protected virtual void onCalm()
    {
        onDelay = false;
        if (state != STATE.calm)
        {
            lastState = state;
            state = STATE.calm;
            goal = calm;
        }
    }
    protected virtual void onPanic()
    {
        onDelay = false;
        if (state != STATE.panic)
        {
            lastState = state;
            state = STATE.panic;
            goal = panic;
        }
    }
    protected virtual void onAlert()
    {
        onDelay = false;
        if(state!=STATE.alert)
        {
            lastState = state;
            state = STATE.alert;
            goal = alert;
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

    protected void onAnimationPlayerStarts(string name)
    {
        startOffset=Position;
    }
    protected void onAnimationPlayerEnded(string name)
    {
        startOffset=Position;
        ANIMATION_OFFSET=Vector2.Zero;
        animationDirection=1;
    }

    protected virtual void FlipH()
    {
        facing = animationController.FlipH ? Vector2.Left : Vector2.Right;
    }

    protected Vector2 FlipX(Vector2 vector)
    {
        vector.x *= -1f;
        return vector;
    }

    protected Vector2 FlipY(Vector2 vector)
    {
        vector.y *= -1f;
        return vector;
    }

    protected virtual Vector2 Facing()
    {
        return animationController.FlipH ? Vector2.Left : Vector2.Right;
    }
    
    public override void _EnterTree()
    {
        if((bool)LEVEL_SETTINGS["Use"])
        {
            levelSettings=new Settings(World.level,Vector2.Zero,(float)LEVEL_SETTINGS["Speed"],(float)LEVEL_SETTINGS["Zoom"]);
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
