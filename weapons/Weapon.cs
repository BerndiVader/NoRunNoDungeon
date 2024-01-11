using Godot;
using System;

public abstract class Weapon : Area2D
{
    [Export] protected float damage=1f;

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

    public override void _Ready()
    {
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

    public override void _Process(float delta) {}

    public virtual void _Free() {}

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
                node.EmitSignal(STATE.damage.ToString(),Player.instance,damage);                            
                hit=true;
            }
        }
    }

    public virtual bool isPlaying()
    {
        return animationPlayer.IsPlaying();
    }

    protected virtual String getStringDirection()
    {
        return directionNames[Player.instance.animationController.FlipH==true?1:0];
    }    

}
