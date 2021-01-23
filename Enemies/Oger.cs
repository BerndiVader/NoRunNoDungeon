using Godot;
using System;

public class Oger : KinematicMonster
{
    [Export] public new Vector2 ANIMATION_OFFSET=new Vector2(0f,0f);
    [Export] public float GRAVITY=500f;
    [Export] public float WALK_FORCE=600f;
    [Export] public float WALK_MIN_SPEED=10f;
    [Export] public float WALK_MAX_SPEED=30f;
    [Export] public float STOP_FORCE=1300f;

    Vector2 velocity=new Vector2(0f,0f);
    Vector2 direction,PLAYERCASTTO,CASTTO;
    float travelTime=0f;

    CollisionShape2D collisionController;
    VisibilityNotifier2D notifier2D;
    RayCast2D rayCast2D,playerCast2D;
    Staff weapon;

    public override void _Ready()
    {
        base._Ready();
        weapon=GetNode<Staff>("Baton");

        notifier2D=new VisibilityNotifier2D();
        animationPlayer=GetNode<Godot.AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect("animation_started",this,nameof(animationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(animationPlayerEnded));

        if(GetParent().GetType().Name=="Placeholder")
        {
            parent=(Placeholder)GetParent();
            notifier2D.Connect("screen_exited",parent,"exitedScreen");
        }
        else 
        {
            notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        AddChild(notifier2D);

        rayCast2D=(RayCast2D)GetNode("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        playerCast2D=(RayCast2D)GetNode("PlayerCast2D");
        playerCast2D.Enabled=true;
        PLAYERCASTTO=playerCast2D.CastTo;

        collisionController=(CollisionShape2D)GetNode("CollisionShape2D");

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
        animationController.Play("default");
        state=STATE.IDLE;
        lastState=state;

        direction=new Vector2(0,0);

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

    public override void idle(float delta)
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
                EmitSignal("Stroll");
                travelTime=0;
            }
            navigation(delta);
        }
        else
        {
            EmitSignal("Attack",WorldUtils.world.player);
        }
    }

    public override void stroll(float delta)
    {
        if(!canSeePlayer())
        {
            travelTime++;
            if(travelTime>300)
            {
                if(MathUtils.randomRangeInt(0,2)==1)
                {
                    EmitSignal("Idle");
                }
                travelTime=0;
            }
            navigation(delta);
        }
        else
        {
            EmitSignal("Attack",WorldUtils.world.player);
        }
    }

    public override void attack(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=direction*150f;

        if(distance>40)
        {
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
            bool jump=false;
            bool stop=true;

            if(left)
            {
                if(velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED) 
                {
                    force.x-=WALK_FORCE;
                    stop=false;
                }
            }
            else if(right)
            {
                if(velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED) 
                {
                    force.x+=WALK_FORCE;
                    stop=false;
                }
            }

            if(!canSeePlayer())
            {
                EmitSignal("Stroll");
            }

            if(stop)
            {
                float vSign=Mathf.Sign(velocity.x);
                float vLen=Mathf.Abs(velocity.x);
                vLen-=STOP_FORCE*delta;
                if(vLen<0f) vLen=0f;
                velocity.x=vLen*vSign;
            }

            velocity+=force*delta;

            Vector2 snap=new Vector2(0f,8f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }

            if(IsOnWall()||jump)
            {
                direction=new Vector2(direction.x*-1,0);
                FlipH();
            }        
        }
        else
        {
            EmitSignal("Fight",victim);
        }
    }

    public override void fight(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        direction=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=direction*40f;

        if(distance<40)
        {
            if(animationController.FlipH&&direction.x>=0)
            {
                FlipH();
            }
            else if(!animationController.FlipH&&direction.x<0)
            {
                FlipH();
            }
            
            Vector2 force=new Vector2(0,GRAVITY);

            if(!canSeePlayer())
            {
                EmitSignal("Stroll");
            }

            float vSign=Mathf.Sign(velocity.x);
            float vLen=Mathf.Abs(velocity.x);
            vLen-=STOP_FORCE*delta;
            if(vLen<0f) vLen=0f;
            velocity.x=vLen*vSign;
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
            EmitSignal("Stroll");
        }


    }

    public override void calm(float delta)
    {
        throw new NotImplementedException();
    }

    public override void damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                EmitSignal("Die");
            }
            else
            {
                state=lastState;
            }
        }
    }

    public override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.passanger(delta);
        }
    }

    public override void die(float delta)
    {
        base.die(delta);
    }

    public override void onDamage(Player player, int amount)
    {
        if(state!=STATE.DAMAGE&&state!=STATE.DIE)
        {
            base.onDamage(player, amount);
            if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
            {
                animationDirection=-1;
            }
            animationPlayer.Play("HIT");
        }
    }

    public override void onAttack(Player player)
    {
        base.onAttack(player);
        animationController.Play("run");
        WALK_MAX_SPEED=80f;
    }

    public override void onFight(Player player)
    {
        base.onFight(player);
        animationController.Play("default");
        weapon.attack();
        WALK_MAX_SPEED=0f;
    }

    public override void onPassanger(Player player)
    {
        base.onPassanger(player);
        animationController.Play("default");
        animationPlayer.Play("PASSANGER");
    }

    public override void onStroll()
    {
        base.onStroll();
        animationController.Play("run");
        WALK_MAX_SPEED=30f;
        travelTime=0;
        playerCast2D.CastTo=direction*150f;
    }


    public override void onIdle()
    {
        base.onIdle();
        animationController.Play("default");
        WALK_MAX_SPEED=30f;
        playerCast2D.CastTo=direction*150f;
    }

    bool canSeePlayer()
    {
        if(playerCast2D.IsColliding())
        {
            return playerCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId();
        }
        else
        {
            return false;
        }
    }

    void FlipH()
    {
        animationController.FlipH^=true;

        int x=-1;

        Vector2 position=rayCast2D.Position;
        position.x*=x;
        rayCast2D.Position=position;
        
        position=playerCast2D.Position;
        position.x*=x;
        playerCast2D.Position=position;

        position=playerCast2D.CastTo;
        position.x*=x;
        playerCast2D.CastTo=position;

        position=collisionController.Position;
        position.x*=x;
        collisionController.Position=position;
    }

    void navigation(float delta)
    {
        if(direction.Length()>0)
        {
            travelTime++;
        }
        Vector2 force=new Vector2(0,GRAVITY);

        bool left=direction.x==-1&&state!=STATE.IDLE;
        bool right=direction.x==1&&state!=STATE.IDLE;
        bool stop=true;

        if(left)
        {
            if(velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED) 
            {
                force.x-=WALK_FORCE;
                stop=false;
            }
        }
        else if(right)
        {
            if(velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED) 
            {
                force.x+=WALK_FORCE;
                stop=false;
            }
        }

        if(stop)
        {
            float vSign=Mathf.Sign(velocity.x);
            float vLen=Mathf.Abs(velocity.x);
            vLen-=STOP_FORCE*delta;
            if(vLen<0f) vLen=0f;
            velocity.x=vLen*vSign;
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
            direction=new Vector2(direction.x*-1,0);
            FlipH();
        }
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

}
