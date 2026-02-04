using System.Collections.Generic;
using Godot;


public class Cannon : KinematicMonster
{
    public static readonly PackedScene bombPack=ResourceLoader.Load<PackedScene>("res://obstacles/Cannonball.tscn");
    public static readonly PackedScene ballPack=ResourceLoader.Load<PackedScene>("res://obstacles/Cannonball2.tscn");
    private static readonly AudioStream shootFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/SingleShot 04.wav");
    private static readonly AudioStream hitFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/26_sword_hit_1.wav");

    private enum CANNONBALL_TYPE
    {
        BOMB,
        BALL
    }

    [Export] private bool DESTORYABLE=false;
    [Export] private float FIRE_DELAY=2f;
    [Export] private CANNONBALL_TYPE CANNONBALL=CANNONBALL_TYPE.BOMB;
    [Export] private bool WARMUP=true;
    [Export] private Dictionary<string,object>CANNONBALL_SETTINGS=Cannonball.GetDefaults();

    private readonly Timer timer=new Timer();
    private AudioStreamPlayer2D sfxPlayer;
    private bool fired;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        sfxPlayer=GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));
        sfxPlayer.Stream=shootFx;
        sfxPlayer.MaxDistance=ResourceUtils.MAX_SFX_DISTANCE;

        timer.WaitTime=FIRE_DELAY;
        timer.OneShot=true;
        AddChild(timer);
        timer.Connect("timeout",this,nameof(TimerTimeout));

        SetSpawnFacing();
        OnIdle();
        if(!WARMUP)
        {
            Fire();
        }

        if(!DESTORYABLE)
        {
            if(staticBody.HasUserSignal(STATE.passanger.ToString()))
            {
                staticBody.Disconnect(STATE.passanger.ToString(),this,nameof(OnPassanger));
            }
            if(HasUserSignal(STATE.passanger.ToString()))
            {
                Disconnect(STATE.passanger.ToString(),this,nameof(OnPassanger));
            }
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        goal(delta);
    }

    protected override void Idle(float delta)
    {
        if(timer.TimeLeft==0)
        {
            timer.Start();
        }
        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        if(animationController.Frame==4)
        {
            timer.WaitTime=MathUtils.RandomRange(1,4);
            OnIdle();
        }
        else if(animationController.Frame==2&&!fired)
        {
            Fire();
            fired=true;
        }
        Navigation(delta);
    }

    protected override void OnDamage(Node2D node=null, float amount=0)
    {

        DaggerShoot particle=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT].Instance<DaggerShoot>();
        particle.Position=Position+GetCollisionRectEdge(node.GlobalPosition);

        SfxPlayer sfx=new SfxPlayer
        {
            Stream=hitFx,
            Position=Position
        };
        World.level.AddChild(sfx);
        sfx.Play();
        World.level.AddChild(particle);

        if(DESTORYABLE)
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
                if(health>0f&&amount!=0f)
                {
                    HealthNotifier(health);
                }
                goal=Damage;
            }
        }
    }

    public override void OnPassanger(Player player=null)
    {
        base.OnPassanger(player);
    }

    protected override void OnAttack(Player player=null)
    {
        onDelay=false;
        if(state!=STATE.attack)
        {
            lastState=state;
            state=STATE.attack;
            victim=Player.instance;
            fired=false;
            goal=Attack;
            sfxPlayer.Play();
            animationController.Play("attack");
            animationController.Playing=true;
        }
    }

    private void Fire()
    {
        Cannonball ball=CANNONBALL==CANNONBALL_TYPE.BOMB?bombPack.Instance<Cannonball>():ballPack.Instance<Cannonball>();
        ball.Position=Position;
        ball.SetDirection(facing);
        if((bool)CANNONBALL_SETTINGS["USE_SETTINGS"])
        {
            ball.SetOptions(CANNONBALL_SETTINGS);
        }
        World.level.AddChild(ball);
    }

    private void TimerTimeout()
    {
        OnAttack();
    }

	protected override void FlipH()
	{
		animationController.FlipH^=true;
		collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
		facing=Facing();
	}


}
