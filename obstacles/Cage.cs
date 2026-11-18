using Godot;
using System;

public class Cage : KinematicMonster
{
    public override void _Ready()
    {
        base._Ready();
        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        goal(delta);
    }

    protected override void Idle(float delta)
    {
        Navigation(delta);
    }

    protected override void Damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0f)
            {
                OnDie();
            }
            else
            {
                OnIdle();
            }            
        }
        Navigation(delta);
    }

    protected override void OnIdle()
    {
        if(state!=STATE.idle)
        {
            animationPlayer.Play("SETUP");
            base.OnIdle();
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0f,bool overrideDestroyable=false)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            if(node==null)
            {
                node=this;
            }
            else if(node is Player)
            {
                attacker=Player.instance;
            }

            Renderer.instance.Shake(1f);

            lastState=state;
            state=STATE.damage;

            if(DESTORYABLE||overrideDestroyable)
            {
                health-=amount;
                if(amount!=0f&&health>0f)
                {
                    HealthNotifier(health);
                }
            }

            animationPlayer.Play("HIT");
            goal=Damage;
        }
        
    }


}
