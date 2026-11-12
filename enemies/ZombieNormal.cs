using Godot;
using System;


public class ZombieNormal : Zombie
{
    [Export] protected int IDLE_COOLDOWN=250;
    [Export] protected int ALERT_COOLDOWN=10;
    [Export] protected int CALM_COOLDOWN=100;

    protected bool used;

    protected override void Idle(float delta)
    {
        if(cooldown>IDLE_COOLDOWN)
        {
            cooldown=ALERT_COOLDOWN;
            used=false;
            OnAlert();
        }
        cooldown++;
        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        if(cooldown<0&&!weapon.IsPlaying())
        {
            if(!used)
            {
                weapon.Attack();
                used=true;            
            }
        }
        if(used&&!weapon.IsPlaying())
        {
            cooldown=CALM_COOLDOWN;
            OnCalm();
        }
        cooldown--;
        Navigation(delta);
    }

    protected override void Calm(float delta)
    {
        if(cooldown<0)
        {
            cooldown=0;
            FlipH();
            OnIdle();
        }
        cooldown--;
        Navigation(delta);
    }



}
