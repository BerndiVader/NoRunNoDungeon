using Godot;
using System;

public class MonsterWeapon : Weapon
{
    protected KinematicMonster owner;
    public override void _Ready()
    {
        base._Ready();
        warmupCount=0;
        Connect("body_entered",this,nameof(OnHitSomething));
    }
    
    public void _Init()
    {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled=true;
        owner=GetParent<KinematicMonster>();
        animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        switch(state)
        {
            case WEAPONSTATE.IDLE:
            {
                if(!animationPlayer.IsPlaying()&&(AnimationNames.SETUP+GetStringDirection()!=animationPlayer.CurrentAnimation))
                {
                    animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
                }
                break;
            }
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
            }
        }        
    }

    public override bool Attack()
    {
        if(state==WEAPONSTATE.IDLE&&cooldownCount==0&&warmupCount==0)
        {
            warmupCount=warmup;
            animationPlayer.Play(AnimationNames.SWING+GetStringDirection());
            state=WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }

    protected override string GetStringDirection()
    {
        return directionNames[owner.FACING==Vector2.Left?1:0];
    }

    protected override void OnHitSomething(Node node)
    {
        if (state==WEAPONSTATE.ATTACK&&!hit&&owner.state!=STATE.damage)
        {
            if (node.IsInGroup(GROUPS.PLAYERS.ToString()))
            {
                cooldownCount=cooldown;
                node.EmitSignal(STATE.damage.ToString(),this,damage);
                hit=true;
            }
            else
            {
                warmupCount=warmup;
            }
        }
        else
        {
            warmupCount=warmup;
        }
    }


}
