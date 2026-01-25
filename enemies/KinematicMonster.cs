using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    private static readonly PackedScene levelControlPack=ResourceLoader.Load<PackedScene>("res://level/LevelControl.tscn");

    protected enum SPAWN_FACING
    {
        DEFAULT,
        RANDOM,
        LEFT,
        RIGHT
    }

    [Export] protected Vector2 ANIMATION_OFFSET=Vector2.Zero;
    [Export] protected Vector2 VELOCITY=Vector2.Zero;
    [Export] protected float DAMAGE_AMOUNT=1f;
    [Export] protected float HEALTH=1f;
    [Export] protected bool RIDEABLE=true;
    [Export] protected float GRAVITY=500f;
    [Export] protected float FRICTION=0.01f;
    [Export] protected float STOP_FORCE=1300f;
    [Export] protected SPAWN_FACING spawn_facing=SPAWN_FACING.DEFAULT;
    [Export] protected int SPAWN_LEFT_WEIGHT=50;
    [Export] protected Godot.Collections.Dictionary<string,object> LEVEL_SETTINGS=new Godot.Collections.Dictionary<string,object>()
    {
        {"Use",false},
        {"Dir",Vector2.Zero},
        {"Speed",-1.0f},
        {"Zoom",-1.0f},
    };

    private Settings levelSettings;
    private bool wasVisible=false;

    protected Player victim,attacker;
    protected AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected StaticBody2D staticBody;
    public AnimatedSprite animationController;

    protected float health;
    protected Vector2 startOffset = Vector2.Zero;
    protected int animationDirection = 1;
    protected Vector2 FORCE;
    protected Vector2 velocity,direction,LastPosition,facing;
    protected readonly Vector2 snap=new Vector2(0f,8f);
    public STATE state,lastState;
    protected delegate void Goal(float delta);
    protected Goal goal;
    protected bool onDelay=false;
    protected bool squeezed=false;

    public override void _Ready()
    {
        FORCE = new Vector2(0f, GRAVITY);
        health = HEALTH;
        velocity = VELOCITY;
        LastPosition = GlobalPosition;
        SetProcess(false);
        SetPhysicsProcess(true);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D = new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(OnScreenExited));
        notifier2D.Connect("screen_entered",this,nameof(OnScreenEntered));
        AddChild(notifier2D);

        collisionController = GetNode<CollisionShape2D>(nameof(CollisionShape2D));
        staticBody = GetNode<StaticBody2D>(nameof(StaticBody2D));
        animationController = GetNode<AnimatedSprite>(nameof(AnimatedSprite));

        staticBody.AddUserSignal(STATE.damage.ToString());
        staticBody.Connect(STATE.damage.ToString(), this, nameof(OnDamage));

        if(RIDEABLE)
        {
            AddUserSignal(STATE.passanger.ToString());
            Connect(STATE.passanger.ToString(), this, nameof(OnPassanger));
            staticBody.AddUserSignal(STATE.passanger.ToString());
            staticBody.Connect(STATE.passanger.ToString(), this, nameof(OnPassanger));
        }


        AddUserSignal(STATE.damage.ToString());
        AddUserSignal(STATE.die.ToString());
        AddUserSignal(STATE.attack.ToString());
        AddUserSignal(STATE.fight.ToString());
        AddUserSignal(STATE.calm.ToString());
        AddUserSignal(STATE.idle.ToString());
        AddUserSignal(STATE.stroll.ToString());
        AddUserSignal(STATE.panic.ToString());
        AddUserSignal(STATE.alert.ToString());

        Connect(STATE.die.ToString(), this, nameof(OnDie));
        Connect(STATE.attack.ToString(), this, nameof(OnAttack));
        Connect(STATE.fight.ToString(), this, nameof(OnFight));
        Connect(STATE.calm.ToString(), this, nameof(OnCalm));
        Connect(STATE.idle.ToString(), this, nameof(OnIdle));
        Connect(STATE.stroll.ToString(), this, nameof(OnStroll));
        Connect(STATE.damage.ToString(), this, nameof(OnDamage));
        Connect(STATE.panic.ToString(), this, nameof(OnPanic));
        Connect(STATE.alert.ToString(), this, nameof(OnAlert));

        AddToGroup(GROUPS.ENEMIES.ToString());
        staticBody.AddToGroup(GROUPS.ENEMIES.ToString());

        attacker = victim = null;
        FORCE = new Vector2(0f, GRAVITY);

        state = STATE.unknown;
        goal = Unknown;

        facing=direction=Facing();

    }

    public override void _PhysicsProcess(float delta)
    {
        direction=Direction();
        facing=Facing();

        Vector2 diff=GlobalPosition-LastPosition;
        squeezed=Mathf.Abs(velocity.y)>200f&&diff.y==0f;
        if(squeezed)
        {
            OnDamage(this,1);
        }

        LastPosition = GlobalPosition;
    }

    protected virtual void Unknown(float delta) { }
    protected virtual void Idle(float delta) {}
    protected virtual void Stroll(float delta) {}
    protected virtual void Attack(float delta) {}
    protected virtual void Fight(float delta) {}
    protected virtual void Panic(float delta) {}
    protected virtual void Alert(float delta) {}
    protected virtual void Passanger(float delta)
    {
        if(health<=0)
        {
            OnDie();
        }
        else
        {
            animationController.SpeedScale=1;
            OnIdle();
        }
    }
    protected virtual void Calm(float delta) {}
    protected virtual void Damage(float delta)
    {
        if(health<=0)
        {
            OnDie();
        }
        else
        {
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
            OnIdle();
        }
    }
    protected virtual void Die(float delta)
    {
        EnemieDieParticles particles=ResourceUtils.particles[(int)PARTICLES.ENEMIEDIE].Instance<EnemieDieParticles>();
        particles.Texture=animationController.Frames.GetFrame(animationController.Animation,animationController.Frame);

        Vector2 position=GlobalPosition;
        position.y+=particles.Texture.GetWidth()/2;
        particles.Position=World.level.ToLocal(position);

        World.level.AddChild(particles);
        QueueFree();
    }
    protected virtual void Navigation(float delta) {}

    protected virtual void OnDie()
    {
        onDelay=false;
        if(state!=STATE.die)
        {
            lastState=state;
            state=STATE.die;
            goal=Die;
        }
    }
    protected virtual void OnAttack(Player player=null)
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
            goal=Attack;
        }
    }
    protected virtual void OnFight(Player player=null)
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
            goal=Fight;
        }
    }
    protected virtual void OnDamage(Node2D node=null,float amount=0f)
    {
        onDelay=false;
        if(state!=STATE.damage&&state!=STATE.die)
        {
            if(node==null)
            {
                node=Player.instance;
            }
            Renderer.instance.Shake(1f);
            staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled",true);
            lastState=state;
            state=STATE.damage;
            if(node is Player)
            {
                attacker=node as Player;
            }
            health-=amount;
            goal=Damage;
        }
    }
    public virtual void OnPassanger(Player player=null)
    {      
        onDelay=false;
        if(state!=STATE.passanger)
        {
            if (player == null)
            {
                player = Player.instance;
            }

            Renderer.instance.Shake(1f);
            lastState=state;
            state = STATE.passanger;
            health -= 0.5f;
            attacker = player;
            goal=Passanger;
        }
    }
    protected virtual void OnCalm()
    {
        onDelay = false;
        if (state != STATE.calm)
        {
            lastState = state;
            state = STATE.calm;
            goal = Calm;
        }
    }
    protected virtual void OnPanic()
    {
        onDelay = false;
        if (state != STATE.panic)
        {
            lastState = state;
            state = STATE.panic;
            goal = Panic;
        }
    }
    protected virtual void OnAlert()
    {
        onDelay = false;
        if(state!=STATE.alert)
        {
            lastState = state;
            state = STATE.alert;
            goal = Alert;
        }
    }
    protected virtual void OnIdle()
    {
        onDelay=false;
        if(state!=STATE.idle)
        {
            lastState=state;
            state=STATE.idle;
            animationController.Play("idle");
            goal=Idle;
        }
    }
    protected virtual void OnStroll()
    {
        onDelay=false;
        if(state!=STATE.stroll)
        {
            lastState=state;
            state=STATE.stroll;
            animationController.Play("stroll");
            goal=Stroll;
        }
    }

    protected virtual void DelayedState(float seconds,STATE next,params object[] args)
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

    protected void OnAnimationPlayerStarts(string name)
    {
        startOffset=Position;
    }
    protected void OnAnimationPlayerEnded(string name)
    {
        ANIMATION_OFFSET=Vector2.Zero;
        animationDirection=1;
    }

    protected virtual void FlipH()
    {
        facing=animationController.FlipH?Vector2.Left:Vector2.Right;
    }

    protected Vector2 FlipX(Vector2 vector)
    {
        vector.x*=-1f;
        return vector;
    }

    protected Vector2 FlipY(Vector2 vector)
    {
        vector.y*=-1f;
        return vector;
    }

    protected virtual Vector2 StopX(Vector2 velocity,float delta)
    {
        float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
        if(xLength<0f) {
            xLength=0f;
        }
        velocity.x=xLength*Mathf.Sign(velocity.x);
        return velocity;
    }

    protected virtual Vector2 Facing()
    {
        return animationController.FlipH?Vector2.Left:Vector2.Right;
    }

    protected virtual Vector2 Direction()
    {
        Vector2 d=LastPosition.DirectionTo(GlobalPosition);
        d.x=Mathf.Sign(d.x);
        d.y=0f;
        return d;
    }

    protected virtual void SetSpawnFacing()
    {
        switch(spawn_facing)
        {
            case SPAWN_FACING.RANDOM:
            case SPAWN_FACING.DEFAULT:
                if(SPAWN_LEFT_WEIGHT!=0)
                {
                    if(SPAWN_LEFT_WEIGHT>MathUtils.RandomRange(0,100))
                    {
                        if(facing!=Vector2.Left)
                        {
                            FlipH();
                        }
                    }
                    else
                    {
                        if(facing!=Vector2.Right)
                        {
                            FlipH();
                        }
                    }
                }
                else
                {
                    if(MathUtils.RandBool())
                    {
                        FlipH();
                    }
                }
                break;
            case SPAWN_FACING.LEFT:
                if(facing!=Vector2.Left)
                {
                    FlipH();
                }
                break;
            case SPAWN_FACING.RIGHT:
                if(facing!=Vector2.Right)
                {
                    FlipH();
                }
                break;
        }
    }

    private void OnScreenEntered()
    {
        wasVisible=true;
    }

    private void OnScreenExited()
    {
        if(wasVisible)
        {
            World.OnObjectExitedScreen(this);
        }
    }
    
    public override void _EnterTree()
    {
        if((bool)LEVEL_SETTINGS["Use"])
        {
            levelSettings=new Settings(World.level,Vector2.Zero,(float)LEVEL_SETTINGS["Speed"],(float)LEVEL_SETTINGS["Zoom"]);
            LevelControl control=levelControlPack.Instance<LevelControl>();
            control.SetMonsterControlled(levelSettings);
            control.Position=Position;
            World.level.AddChild(control);
        }
    }

    public override void _ExitTree()
    {
        if((bool)LEVEL_SETTINGS["Use"])
        {
            levelSettings.Restore();
        }
    }

}
