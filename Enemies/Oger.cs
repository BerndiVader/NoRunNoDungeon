using Godot;
using System;

public class Oger : KinematicMonster
{
    [Export] private new Vector2 ANIMATION_OFFSET=new Vector2(0f,0f);
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=10f;
    [Export] private float WALK_MAX_SPEED=30f;
    [Export] private float STOP_FORCE=1300f;

    private Vector2 velocity=new Vector2(0f,0f);
    private Vector2 direction,PLAYERCASTTO,CASTTO;
    private float travelTime=0f;
    private RayCast2D rayCast2D,playerCast2D;
    private Staff weapon;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        playerCast2D=GetNode<RayCast2D>("PlayerCast2D");
        playerCast2D.Enabled=true;
        PLAYERCASTTO=playerCast2D.CastTo;

        animationController.Play("idle");
        state=STATE.idle;
        lastState=state;

        direction=new Vector2(0,0);

        weapon=GetNode<Staff>("Baton");
        if(weapon!=null)
        {
            weapon._Init();
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        if(animationPlayer.IsPlaying())
        {
            Position=startOffset+(ANIMATION_OFFSET*animationDirection);
        }
        tick(delta);
    }

    protected override void idle(float delta)
    {
        if(!canSeePlayer())
        {
            travelTime++;
            if(travelTime==50)
            {
                if(MathUtils.randomRangeInt(0,5)>2)
                {
                    FlipH();
                }
            }
            if(travelTime>100)
            {
                direction=MathUtils.randomRangeInt(0,2)==1?Vector2.Left:Vector2.Right;

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
            onAttack(World.instance.player);
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
            onAttack(World.instance.player);
        }
    }

    protected override void attack(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=direction*150f;

        if(distance>40)
        {
            float angle=Mathf.Rad2Deg(GlobalPosition.AngleToPoint(victim.GlobalPosition));
            if(angle>45&&angle<165||!canSeePlayer())
            {
                EmitSignal(lastState.ToString());
                return;
            }
           
            if(animationController.FlipH&&direction.x>=0)
            {
                FlipH();
            }
            else if(!animationController.FlipH&&direction.x<0)
            {
                FlipH();
            }

            Vector2 force=new Vector2(0,GRAVITY);

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

            Vector2 snap=new Vector2(0f,8f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }

            if(IsOnWall())
            {
                direction=new Vector2(direction.x*-1,0);
                FlipH();
            }        
        }
        else
        {
            float angle=Mathf.Rad2Deg(GlobalPosition.AngleToPoint(victim.GlobalPosition));
            if(angle>45&&angle<165)
            {
                EmitSignal(lastState.ToString());
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

        if(distance<40)
        {
            float angle=Mathf.Rad2Deg(GlobalPosition.AngleToPoint(victim.GlobalPosition));
            if(angle>45&&angle<135||!canSeePlayer())
            {
                onStroll();
                return;
            }

            if(animationController.FlipH&&direction.x>=0)
            {
                FlipH();
            }
            else if(!animationController.FlipH&&direction.x<0)
            {
                FlipH();
            }

            Vector2 force=new Vector2(0,GRAVITY);

            float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
            if(xLength<0f) {
                xLength=0f;
            }
            velocity.x=xLength*Mathf.Sign(velocity.x);

            velocity+=force*delta;

            Vector2 snap=new Vector2(0f,8f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
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
                EmitSignal(lastState.ToString());
            }
        }
    }

    protected override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.passanger(delta);
        }
    }

    protected override void onDamage(Player player, int amount)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.onDamage(player, amount);
            if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
            {
                animationDirection=-1;
            }
            animationPlayer.Play("HIT");
        }
    }

    protected override void onAttack(Player player)
    {
        if(state!=STATE.attack)
        {
            base.onAttack(player);
            animationController.Play("stroll");
            WALK_MAX_SPEED=80f;
        }
    }

    protected override void onFight(Player player)
    {
        if(state!=STATE.fight)
        {
            base.onFight(player);
            animationController.Play("idle");
            weapon.attack();
            WALK_MAX_SPEED=0f;
        }
    }

    public override void onPassanger(Player player)
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
            base.onStroll();
            WALK_MAX_SPEED=30f;
            travelTime=0;
            playerCast2D.CastTo=new Vector2(direction.x,0f)*150f;
        }
    }


    protected override void onIdle()
    {
        if(state!=STATE.idle)
        {
            base.onIdle();
            WALK_MAX_SPEED=30f;
            playerCast2D.CastTo=new Vector2(direction.x,0f)*150f;
        }
    }

    private bool canSeePlayer()
    {
        return playerCast2D.IsColliding()&&playerCast2D.GetCollider().GetInstanceId()==World.instance.player.GetInstanceId();
    }

    private void FlipH()
    {
        animationController.FlipH^=true;

        Vector2 position=rayCast2D.Position;
        position.x*=-1;
        rayCast2D.Position=position;
        
        position=playerCast2D.Position;
        position.x*=-1;
        playerCast2D.Position=position;

        position=playerCast2D.CastTo;
        position.x*=-1;
        playerCast2D.CastTo=position;

        position=collisionController.Position;
        position.x*=-1;
        collisionController.Position=position;
    }

    void navigation(float delta)
    {
        if(direction.Length()>0)
        {
            travelTime++;
        }
        Vector2 force=new Vector2(0,GRAVITY);

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

        Vector2 snap=new Vector2(0f,8f);
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        if(IsOnFloor())
        {
            velocity-=GetFloorVelocity()*delta;
        }

        if(IsOnWall()||!rayCast2D.IsColliding())
        {
            direction*=-1f;
            FlipH();
        }
    }

}
