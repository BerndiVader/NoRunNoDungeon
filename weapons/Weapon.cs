using Godot;
using System;

public abstract class Weapon : Area2D
{
    [Export] protected float damage=1f;

    protected AnimationPlayer animationPlayer;
    protected bool hit;
    protected WEAPONSTATE state;
    protected WEAPONSTATE oldState;

    public override void _Ready()
    {
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
            if(node.GetParent()!=null)
            {
                node=node.GetParent();
            }

            if(node.IsInGroup(GROUPS.ENEMIES.ToString()))
            {
                node.EmitSignal(SIGNALS.Damage.ToString(),World.instance.player,damage);                            
                hit=true;
            }
        }
    }

    public virtual bool isPlaying()
    {
        return animationPlayer.IsPlaying();
    }

}
