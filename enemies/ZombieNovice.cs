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
            else
            {
                rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
                cooldown=0;
                OnIdle();
            }

            Navigation(delta);
            return;
        }

        float distance=GlobalPosition.DistanceSquaredTo(victim.GlobalPosition);
        if(distance<attackRangeSqrd)
        {
            Vector2 direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x<0f);

            rayCast2D.CastTo=Mathf.Sqrt(distance)*direction;
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

    protected override void Fight(float delta)
    {
        if(!weapon.IsPlaying())
        {
            weapon.Attack();
            cooldown=20;
        }
        else
        {
            rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
            cooldown=0;
            OnIdle();
        }

        Navigation(delta);
    }

    protected override void Damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                OnDie();
            }
            else
            {
                if(facing.x!=MathUtils.SignVector(GlobalPosition.DirectionTo(Player.instance.GlobalPosition)).x)
                {
                    FlipH();
                    cooldown=0;
                }

                if(MathUtils.RandBool())
                {
                    cooldown=0;
                    OnFight();
                }
                else
                {
                    OnIdle();
                }

            }
        }
        Navigation(delta);
    }    

}
