using Godot;
using System;

public abstract class Weapon : Area2D
{
    public Godot.AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
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
            if(node.GetParent()!=null)
            {
                node=node.GetParent();
            }

            if(node.IsInGroup(GROUPS.ENEMIES.ToString()))
            {
                node.EmitSignal("Damage",WorldUtils.world.player,1f);                            
                hit=true;
            }
        }
    }

}
