using Godot;
using System;

public class MonsterWeapon : Weapon
{
    protected KinematicMonster owner;
    public override void _Ready()
    {
        base._Ready();
        warmupCount = 0;
        Connect("body_entered",this,nameof(onHitSomething));
    }
    public void _Init()
    {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled=true;
        owner=GetParent<KinematicMonster>();
        animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.IDLE:
            {
                if(!animationPlayer.IsPlaying()&&(AnimationNames.SETUP+getStringDirection()!=animationPlayer.CurrentAnimation))
                {
                    animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
                }
                break;
            }
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
            }
        }        
    }

    public override bool attack()
    {
        if (state == WEAPONSTATE.IDLE && cooldownCount == 0 && warmupCount == 0)
        {
            animationPlayer.Play(AnimationNames.SWING + getStringDirection());
            state = WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }

    protected override string getStringDirection()
    {
        return directionNames[owner.animationController.FlipH==true?1:0];
    }

    protected override void onHitSomething(Node node)
    {
        if (state == WEAPONSTATE.ATTACK && !hit && owner.state != STATE.damage)
        {
            if (node.IsInGroup(GROUPS.PLAYERS.ToString()))
            {
                cooldownCount = cooldown;
                node.EmitSignal(STATE.damage.ToString(), damage, this);
                hit = true;
            }
            else
            {
                warmupCount = warmup;
            }
        }
        else
        {
            warmupCount = warmup;
        }
    }


}
