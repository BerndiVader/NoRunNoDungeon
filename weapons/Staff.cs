using Godot;
using System;

public class Staff : Weapon
{
    protected Zombie owner;
    bool flipped;
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(hitSomething));
    }
    public void _Init()
    {
        owner=GetParent<Zombie>();

        if(owner!=null)
        {
            flipped=owner.animationController.FlipH;
            animationPlayer.Play("SETUP"+getStringDirection());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.IDLE:
            {
                if(owner.animationController.FlipH!=flipped&&!animationPlayer.IsPlaying())
                {
                    flipped=owner.animationController.FlipH;
                    animationPlayer.Play("SETUP"+getStringDirection());
                }
                break;
            }
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    animationPlayer.Play("SETUP"+getStringDirection());
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
            }
        }        
    }

    public override void attack()
    {
        if(state==WEAPONSTATE.IDLE)
        {
            animationPlayer.Play("SWING"+getStringDirection());
            state=WEAPONSTATE.ATTACK;
        }
    }

    String getStringDirection()
    {
        if(flipped)
        {
            return "_LEFT";
        }
        else
        {
            return "_RIGHT";
        }
    }

    public override void hitSomething(Node node)
    {
        if(state==WEAPONSTATE.ATTACK&&!hit)
        {
            if(node.IsInGroup("Players"))
            {
                node.EmitSignal("Damage",1f);                            
                hit=true;
            }
        }
    }


}
