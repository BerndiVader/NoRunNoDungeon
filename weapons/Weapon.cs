using System.Data.Odbc;
using Godot;

public abstract class Weapon : Area2D
{
    [Export] protected float damage=1f;
    [Export] protected float cooldown=0.1f;
    [Export] protected float  warmup=0.1f;

    protected Timer cooldownTimer;
    protected Timer warmupTimer;

    protected AnimationPlayer animationPlayer;
    protected bool hit;
    protected WEAPONSTATE state;
    protected WEAPONSTATE oldState;
    protected static string[]directionNames=new string[]{"_RIGHT","_LEFT"};
    protected enum AnimationNames
    {
        SETUP,
        SWING,
        DOUBLE_SWING
    }

    protected bool warmupReady=false;
    protected bool cooldownReady=false;

    protected static readonly AudioStream sfxSwing=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/56_Attack_03.wav");
    protected static readonly AudioStream sfxHit=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/61_Hit_03.wav");
    protected static readonly AudioStream sfxMiss=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/08_Step_rock_02.wav");

    public override void _Ready()
    {
        cooldownTimer=new Timer();
        warmupTimer=new Timer();

        cooldownTimer.OneShot=true;
        cooldownTimer.WaitTime=cooldown;

        warmupTimer.OneShot=true;
        warmupTimer.WaitTime=warmup;

        AddChild(cooldownTimer);
        AddChild(warmupTimer);

        warmupTimer.Start();
        cooldownTimer.Start();

        animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.CurrentAnimation=AnimationNames.SETUP.ToString();
        animationPlayer.Play();

        CPUParticles2D initParticles=ResourceUtils.particles[(int)PARTICLES.WEAPONCHANGE].Instance<CPUParticles2D>();
        initParticles.Position=World.level.ToLocal(GlobalPosition);
        initParticles.Texture=GetNode<Sprite>(nameof(Sprite)).Texture;
        initParticles.Rotation=Rotation;
        World.level.AddChild(initParticles);
        
        SetProcess(false);
        SetProcessInput(false);

        Visible=true;
        state=WEAPONSTATE.IDLE;
        oldState=state;
    }

    public virtual void _Free() {}

    public virtual bool Attack()
    {
        if(state==WEAPONSTATE.IDLE&&CooldownReady()&&WarmupReady())
        {
            animationPlayer.Play(AnimationNames.SWING+GetStringDirection());
            state=WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }

    protected enum WEAPONSTATE
    {
        IDLE,
        ATTACK
    }

    protected virtual void OnHitSomething(Node node)
    {
        if(state==WEAPONSTATE.ATTACK&&!hit)
        {
            if(node.HasUserSignal(STATE.damage.ToString()))
            {
                PlaySfx(sfxHit);
                node.EmitSignal(STATE.damage.ToString(),Player.instance,damage);
                hit = true;
                animationPlayer.PlayBackwards();
            }
            else
            {
                PlaySfx(sfxMiss);
            }
        }
    }

    public virtual bool IsPlaying()
    {
        return animationPlayer.IsPlaying();
    }

    protected virtual string GetStringDirection()
    {
        return directionNames[Player.instance.AnimationController.FlipH==true?1:0];
    }

    protected void PlaySfx(AudioStream stream)
    {
        SfxPlayer sfx=new SfxPlayer();
        sfx.Position=World.level.ToLocal(GlobalPosition);
        sfx.Stream=stream;
        World.level.AddChild(sfx);
    }

    public bool CooldownReady()
    {
        return 0f==cooldownTimer.TimeLeft;
    }

    public bool WarmupReady()
    {
        return 0f==warmupTimer.TimeLeft;
    }

}
