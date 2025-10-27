using Godot;
using System;

public class Oger : KinematicMonster
{
    [Export] private new Vector2 ANIMATION_OFFSET=new Vector2(0f,0f);
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=10f;
    [Export] private float WALK_MAX_SPEED=40f;
    [Export] private float STOP_FORCE=1300f;

    private Vector2 snap=new Vector2(0f,8f);

    private float travelTime=0f;
    private RayCast2D rayCast2D,playerCast2D;
    private MonsterWeapon weapon;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled=true;

        playerCast2D=GetNode<RayCast2D>("PlayerCast2D");
        playerCast2D.Enabled=true;

        direction =new Vector2(animationController.FlipH?-1f:1f,0f);

        animationController.Play("idle");
        onIdle();

        weapon=GetNode<MonsterWeapon>("Baton");
        if(weapon!=null)
        {
            weapon._Init();
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
        
        if (animationPlayer.IsPlaying())
        {
            Position = startOffset + (ANIMATION_OFFSET * animationDirection);
        }
    }

    protected override void idle(float delta)
    {
        if(!canSeePlayer())
        {
            travelTime++;
            if(travelTime==50)
            {
                if(MathUtils.randomRangeInt(0,5)>4)
                {
                    FlipH();
                }
            } 
            else if(travelTime>100)
            {
                direction=MathUtils.randomRangeInt(0,4)==1?Vector2.Right:Vector2.Left;

                if((animationController.FlipH&&direction!=Vector2.Left)||(!animationController.FlipH&&direction!=Vector2.Right))
                {
                    FlipH();
                }
                onStroll();
                return;
            }
            navigation(delta);
        }
        else
        {
            onAttack(Player.instance);
        }
    }

    protected override void stroll(float delta)
    {
        if(!canSeePlayer())
        {
            travelTime++;
            if(travelTime>300)
            {
                if(MathUtils.randomRangeInt(0,5)==1)
                {
                    onIdle();
                    return;
                }
                travelTime=0;
            }
            navigation(delta);
        }
        else
        {
            onAttack(Player.instance);
        }
    }

    protected override void attack(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=direction*150f;

        if(distance>40)
        {
            if(!canSeePlayer())
            {
                if (MathUtils.randBool())
                {
                    onIdle();
                }
                else
                {
                    onStroll();
                }
                return;
            }
           
            if(animationController.FlipH&&direction.x>=0||!animationController.FlipH&&direction.x<0)
            {
                FlipH();
            }


            Vector2 force=new Vector2(FORCE);

            bool left=direction.x<0&&rayCast2D.IsColliding();
            bool right=direction.x>0&&rayCast2D.IsColliding();

            if(left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
            {
                force.x-=WALK_FORCE;
            }
            else if(right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
            {
                force.x+=WALK_FORCE;
            }
            else
            {
                float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
                if(xLength<0f) {
                    xLength=0f;
                }
                velocity.x=xLength*Mathf.Sign(velocity.x);
            }

            velocity+=force*delta;
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }

            if(IsOnWall())
            {
                direction=new Vector2(Mathf.Sign(direction.x)*-1f,0f);
                FlipH();
                onIdle();
            }        
        }
        else
        {
            float angle=Mathf.Rad2Deg(GlobalPosition.AngleToPoint(victim.GlobalPosition));
            if(angle>45&&angle<165)
            {
                if (MathUtils.randBool())
                {
                    onStroll();
                }
                else
                {
                    onIdle();
                }
            }
            else
            {
                onFight(victim);
            }
        }
    }

    protected override void fight(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=direction*40f;

        if(distance<40f)
        {
            if(!canSeePlayer())
            {
                onIdle();
                return;
            }

            if(animationController.FlipH&&direction.x>=0||!animationController.FlipH&&direction.x<0)
            {
                FlipH();
            }

            Vector2 force=new Vector2(FORCE);

            float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
            if(xLength<0f) {
                xLength=0f;
            }
            velocity.x=xLength*Mathf.Sign(velocity.x);
            velocity+=force*delta;
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }
            if(IsOnWall())
            {
                direction=new Vector2(Mathf.Sign(direction.x)*-1f,0f);
                FlipH();
                onIdle();
            }
        }
        else
        {
            onStroll();
        }
    }

    protected override void calm(float delta)
    {
        throw new NotImplementedException();
    }

    protected override void damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                onDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                onIdle();
            }
        }
    }

    protected override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                onDie();
            }
            else
            {
                animationController.SpeedScale = 1;
                if (MathUtils.randBool())
                {
                    onAttack(Player.instance);
                }
                else
                {
                    onStroll();
                }
            }            
        }
    }

    protected override void onDamage(Player player=null, int amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.onDamage(player, amount);
            if (player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x < 0)
            {
                animationDirection = -1;
            }
            else
            {
                animationDirection = 1;
            }
            animationPlayer.Play("HIT");
        }
    }

    protected override void onAttack(Player player=null)
    {      
        if(state!=STATE.attack)
        {
            base.onAttack(player);
            animationController.Play("stroll");
            WALK_MAX_SPEED=80f;
        }
    }

    protected override void onFight(Player player=null)
    {
        if(state!=STATE.fight)
        {
            base.onFight(player);
            animationController.Play("idle");
            weapon.attack();
            WALK_MAX_SPEED=0f;
        }
    }

    public override void onPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {         
            base.onPassanger(player);
            animationController.Play("idle");
            animationPlayer.Play("PASSANGER");
        }
    }

    protected override void onStroll()
    {
        if(state!=STATE.stroll)
        {
            WALK_MAX_SPEED=30f;
            travelTime=0f;
            playerCast2D.CastTo=Facing()*150f;
            base.onStroll();
        }
    }


    protected override void onIdle()
    {
        if(state!=STATE.idle)
        {
            travelTime=0f;
            WALK_MAX_SPEED=30f;
            playerCast2D.CastTo=Facing()*150f;
            base.onIdle();
        }
    }

    private bool canSeePlayer()
    {
        return playerCast2D.IsColliding()&&playerCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId();
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;

        rayCast2D.Position=FlipX(rayCast2D.Position);
        playerCast2D.Position=FlipX(playerCast2D.Position);
        playerCast2D.CastTo=FlipX(playerCast2D.CastTo);
        collisionController.Position=FlipX(collisionController.Position);
    }

    void navigation(float delta)
    {
        if(direction.Length()>0)
        {
            travelTime++;
        }
        Vector2 force=new Vector2(FORCE);

        bool left=direction.x<0f&&state!=STATE.idle;
        bool right=direction.x>0f&&state!=STATE.idle;

        if(left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        }
        else if(right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
        {
            force.x+=WALK_FORCE;
        }
        else
        {
            float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
            if(xLength<0f) {
                xLength=0f;
            }
            velocity.x=xLength*Mathf.Sign(velocity.x);
        }

        velocity+=force*delta;

        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        if(IsOnFloor())
        {
            velocity-=GetFloorVelocity()*delta;
        }

        if(IsOnWall()||!rayCast2D.IsColliding())
        {
            direction=new Vector2(Mathf.Sign(direction.x)*-1f,0f);
            FlipH();
        }
    }

}
