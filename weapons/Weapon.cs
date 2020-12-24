using Godot;
using System;

public abstract class Weapon : Area2D
{
    protected AnimationPlayer animationPlayer;
    protected CollisionShape2D collisionController;
    public WEAPONSTATE state;
    public WEAPONSTATE old_state;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        Visible=true;
        state=old_state=WEAPONSTATE.IDLE;
        animationPlayer=(AnimationPlayer)GetNode("AnimationPlayer");
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

}
