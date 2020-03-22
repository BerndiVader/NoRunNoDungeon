using Godot;
using System;

public abstract class KinematicMonster : KinematicBody2D
{
    public STATE state;

    public void tick(float delta)
    {
        switch(state)
        {
            case STATE.IDLE:
            {
                idle(delta);
                break;
            }
            case STATE.ATTACK:
            {
                attack(delta);
                break;
            }
            case STATE.FIGHT:
            {
                fight(delta);
                break;
            }
            case STATE.DIE:
            {
                die(delta);
                break;
            }
        }
    }

    public abstract void idle(float delta);
    public abstract void attack(float delta);
    public abstract void fight(float delta);
    public abstract void die(float delta);

}
