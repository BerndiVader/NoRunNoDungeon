using Godot;
using System;

public abstract class Weapon : Area2D
{
    [Export] protected float damage=1f;

    protected AnimationPlayer animationPlayer;
    protected bool hit;
    protected WEAPONSTATE state;
    protected WEAPONSTATE oldState;
    PackedScene weaponInitPartPacked=ResourceUtils.particles[(int)PARTICLES.WEAPONCHANGE];

    public override void _Ready()
    {
        CPUParticles2D initParticles=weaponInitPartPacked.Instance<CPUParticles2D>();
        initParticles.Position=World.level.ToLocal(GlobalPosition);
        initParticles.Texture=GetNode<Sprite>(nameof(Sprite)).Texture;
        initParticles.Rotation=Rotation;
        World.level.AddChild(initParticles);
        SetProcess(false);
        SetProcessInput(false);
        Visible=true;
        state=WEAPONSTATE.IDLE;
        oldState=state;
        animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.CurrentAnimation="SETUP";
        animationPlayer.Play();
    }

    public override void _PhysicsProcess(float delta)
    {

    }

    public virtual void _Free()
    {

    }

    public abstract void attack();

    protected enum WEAPONSTATE
    {
        IDLE,
        ATTACK
    }

    protected virtual void onHitSomething(Node node)
    {
        if(state==WEAPONSTATE.ATTACK&&!hit)
        {
            if(node.HasUserSignal(STATE.damage.ToString()))
            {
                node.EmitSignal(STATE.damage.ToString(),World.instance.player,damage);                            
                hit=true;
            }
        }
    }

    public virtual bool isPlaying()
    {
        return animationPlayer.IsPlaying();
    }

}
