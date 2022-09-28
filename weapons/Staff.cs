using Godot;
using System;

public class Staff : Weapon
{
    protected KinematicMonster owner;
    bool flipped;
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(hitSomething));
    }
    public void _Init()
    {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled=true;
        owner=GetParent() as KinematicMonster;

        if(owner!=null)
        {
            animationPlayer.Play("SETUP"+getStringDirection());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.IDLE:
            {
                if(!animationPlayer.IsPlaying())
                {
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

    protected String getStringDirection()
    {
        flipped=owner.animationController.FlipH;

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
        if(state==WEAPONSTATE.ATTACK&&!hit&&owner.state!=STATE.DAMAGE)
        {
            if(node.IsInGroup(GROUPS.PLAYERS.ToString()))
            {
                node.EmitSignal(SIGNALS.Damage.ToString(),damage,this);                           
                hit=true;
            }
        }
    }


}
