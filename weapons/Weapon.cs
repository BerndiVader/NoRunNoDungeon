using Godot;
using System;

public abstract class Weapon : Area2D
{
    AnimationPlayer animationPlayer;
    CollisionShape2D collisionControler;
    public Player player;
    public WEAPONSTATE state;
    public WEAPONSTATE old_state;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        Visible=false;
        state=old_state=WEAPONSTATE.IDLE;
    }

    public abstract void attack();

    public enum WEAPONSTATE
    {
        IDLE,
        ATTACK
    }

}
