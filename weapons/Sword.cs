using Godot;
using System;

public class Sword : Weapon
{

    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(hitSomething));
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.ATTACK:
            {

                if(!animationPlayer.IsPlaying())
                {
                    animationPlayer.Play("SETUP");
                    state=WEAPONSTATE.IDLE;
                }
                break;
            }
        }
    }


    public override void attack()
    {
        if(state==WEAPONSTATE.IDLE)
        {
            animationPlayer.Play("SWING");
            state=WEAPONSTATE.ATTACK;
        }
    }

    public void hitSomething(Node node)
    {
        if(state==WEAPONSTATE.ATTACK)
        {
            if(node.IsInGroup("Enemies"))
            {
                if(node.GetParent()!=null)
                {
                    node.GetParent().EmitSignal("Die");
                }
                else
                {
                    node.EmitSignal("Die");                            
                }
            }
        }
    }

}
