using Godot;
using System;

public class ZombieNovice : Zombie
{
    protected override void Attack(float delta)
    {
        if(MathUtils.RandomRange(0,6)>2)
        {
            if(!weapon.IsPlaying())
            {
                weapon.Attack();
                cooldown=20;
            }

            rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
            cooldown=0;
            OnIdle();

            Navigation(delta);
            return;
        }

        float distance=rayCast2D.GlobalPosition.DistanceTo(victim.GlobalPosition);

        if(distance<ATTACK_RANGE)
        {
            Vector2 direction=rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x<0f);

            rayCast2D.CastTo=direction*distance;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                if(cooldown<0&&!weapon.IsPlaying())
                {
                    weapon.Attack();
                    cooldown=20;
                }
            }
            else
            {
                rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
                cooldown=0;
                OnIdle();
            }
        }
        else
        {
            rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
            cooldown=0;
            OnIdle();
        }
        cooldown--;
        Navigation(delta);
    }

}
