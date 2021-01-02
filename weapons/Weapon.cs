using Godot;
using System;

public abstract class Weapon : Area2D
{
    public Godot.AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    protected CPUParticles2D particles2D;
    protected bool hit;
    public WEAPONSTATE state;
    public WEAPONSTATE oldState;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        Visible=true;
        state=WEAPONSTATE.IDLE;
        oldState=state;
        animationPlayer=(Godot.AnimationPlayer)GetNode("AnimationPlayer");
        collisionController=(CollisionShape2D)GetNode("CollisionShape2D");
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

    public enum WEAPONSTATE
    {
        IDLE,
        ATTACK
    }

    public virtual void hitSomething(Node node)
    {
        if(state==WEAPONSTATE.ATTACK&&!hit)
        {
            if(node.IsInGroup("Enemies"))
            {
                particles2D.Emitting=true;
                if(node.GetParent()!=null)
                {
                    node.GetParent().EmitSignal("Damage",WorldUtils.world.player,1f);
                }
                else
                {
                    node.EmitSignal("Damage",WorldUtils.world.player,1f);                            
                }
                hit=true;
            }
        }
    }

}
