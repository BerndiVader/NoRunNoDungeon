using System.Collections.Generic;
using Godot;


public class Cannon : KinematicMonster
{
    private static readonly PackedScene bombPack=ResourceLoader.Load<PackedScene>("res://obstacles/Cannonball.tscn");
    private static readonly PackedScene ballPack=ResourceLoader.Load<PackedScene>("res://obstacles/Cannonball2.tscn");
    private static readonly AudioStream shootFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/SingleShot 04.wav");

    private enum CANNONBALL_TYPE
    {
        BOMB,
        BALL
    }

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
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        goal(delta);
        Navigation(delta);
    }

    protected override void Navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                var collision=GetSlideCollision(i);
                if(collision.Collider is Platform platform&&collision.Normal==Vector2.Up)
                {
                    velocity.x=platform.CurrentSpeed.x;
                } else
                {
                    velocity=StopX(velocity,delta);
                }
            }    
        }
        else
        {
            velocity=StopX(velocity,delta);
        }
    }    

    protected override void Idle(float delta)
    {
        if(timer.TimeLeft==0)
        {
            timer.Start();
        }
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
