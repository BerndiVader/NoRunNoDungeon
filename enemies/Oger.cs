using Godot;
using System;

public class Oger : KinematicMonster
{
    [Export] private new Vector2 ANIMATION_OFFSET=new Vector2(0f,0f);
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=10f;
    [Export] private float WALK_MAX_SPEED=40f;
    [Export] private float FIGHT_DISTANCE = 30f;
    [Export] private float DETECT_DISTANCE=150f;
    private float travelTime=0f;
    private RayCast2D rayCast2D,playerCast2D;
    private MonsterWeapon weapon;

    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;

        playerCast2D=GetNode<RayCast2D>("PlayerCast2D");
        playerCast2D.Enabled=true;

        weapon=GetNode<MonsterWeapon>("Baton");
        if(weapon!=null)
        {
            weapon._Init();
        }        

        SetSpawnFacing();

        animationController.Play("idle");
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);        
        if (animationPlayer.IsPlaying())
        {
            Position = startOffset + (ANIMATION_OFFSET * animationDirection);
        }
        goal(delta);
    }

    protected override void Idle(float delta)
    {
        if(!CanSeePlayer())
        {
            travelTime++;
            if(travelTime==50f)
            {
                if(MathUtils.RandomRange(0,5)>4)
                {
                    FlipH();
                }
            } 
            else if(travelTime>100f)
            {
                if(MathUtils.RandomRange(0,4)==1)
                {
                    FlipH();
                }
                OnStroll();
                return;
            }
            Navigation(delta);
        }
        else
        {
            OnAttack(Player.instance);
        }
    }

    protected override void Stroll(float delta)
    {
        if(!CanSeePlayer())
        {
            travelTime++;
            if(travelTime>300f)
            {
                if(MathUtils.RandomRange(0,5)==1)
                {
                    OnIdle();
                    return;
                }
                travelTime=0f;
            }
            Navigation(delta);
        }
        else
        {
            OnAttack(Player.instance);
        }
    }

    protected override void Attack(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);
        Vector2 dir=GlobalPosition.DirectionTo(victim.GlobalPosition);
        playerCast2D.CastTo=dir*DETECT_DISTANCE;

        if(distance>FIGHT_DISTANCE)
        {
            if(!CanSeePlayer())
            {
                if (MathUtils.RandBool())
                {
                    OnIdle();
                }
                else
                {
                    OnStroll();
                }
                return;
            }

            if(Mathf.Sign(dir.x)!=facing.x)
            {
                FlipH();
            }
           

            Vector2 force=new Vector2(FORCE);

            bool left=facing.x==-1f&&rayCast2D.IsColliding();
            bool right=facing.x==1f&&rayCast2D.IsColliding();

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
                velocity=StopX(velocity,delta);
            }

            velocity+=force*delta;
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }

            if(IsOnWall())
            {
                FlipH();
                OnIdle();
            }        
        }
        else
        {
            float angle=Mathf.Rad2Deg(GlobalPosition.AngleToPoint(victim.GlobalPosition));
            if(angle>45f&&angle<165f)
            {
                if (MathUtils.RandBool())
                {
                    OnStroll();
                }
                else
                {
                    OnIdle();
                }
            }
            else
            {
                OnFight(victim);
            }
        }
    }

    protected override void Fight(float delta)
    {
        if(GlobalPosition.DistanceTo(victim.GlobalPosition)<=FIGHT_DISTANCE)
        {
            Vector2 dir=GlobalPosition.DirectionTo(victim.GlobalPosition);
            playerCast2D.CastTo=dir*(FIGHT_DISTANCE+15f);
            if(!CanSeePlayer())
            {
                OnIdle();
                return;
            }

            if(Mathf.Sign(dir.x)!=facing.x)
            {
                FlipH();
            }

            Vector2 force=new Vector2(FORCE);

            velocity=StopX(velocity,delta);
            velocity+=force*delta;
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

            if(IsOnFloor())
            {
                velocity-=GetFloorVelocity()*delta;
            }
            if(IsOnWall())
            {
                FlipH();
                OnIdle();
            }
        }
        else
        {
            OnStroll();
        }
    }

    protected override void Calm(float delta)
    {
        throw new NotImplementedException();
    }

    protected override void Damage(float delta)
    {
        if (!animationPlayer.IsPlaying())
        {
            if (health <= 0f)
            {
                OnDie();
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                OnAlert();
            }
        }
    }

    protected override void Alert(float delta)
    {
        Fight(delta);
        
    }

    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                OnDie();
            }
            else
            {
                animationController.SpeedScale = 1f;
                if (MathUtils.RandBool())
                {
                    OnAttack(Player.instance);
                }
                else
                {
                    OnStroll();
                }
            }            
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0f)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node, amount);
            if (node.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x < 0f)
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

    protected override void OnAttack(Player player=null)
    {      
        if(state!=STATE.attack)
        {
            base.OnAttack(player);
            animationController.Play("stroll");
            WALK_MAX_SPEED=80f;
        }
    }

    protected override void OnFight(Player player=null)
    {
        if(state!=STATE.fight)
        {
            base.OnFight(player);
            animationController.Play("idle");
            weapon.Attack();
            WALK_MAX_SPEED=0f;
        }
    }

    public override void OnPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {         
            base.OnPassanger(player);
            animationController.Play("idle");
            animationPlayer.Play("PASSANGER");
        }
    }

    protected override void OnStroll()
    {
        if(state!=STATE.stroll)
        {
            WALK_MAX_SPEED=30f;
            travelTime=0f;
            playerCast2D.CastTo=Facing()*DETECT_DISTANCE;
            base.OnStroll();
        }
    }


    protected override void OnIdle()
    {
        if (state != STATE.idle)
        {
            travelTime = 0f;
            WALK_MAX_SPEED = 30f;
            playerCast2D.CastTo = Facing()*DETECT_DISTANCE;
            base.OnIdle();
        }
    }

    protected override void OnAlert()
    {
        onDelay = false;
        if(state!=STATE.alert)
        {
            lastState = state;
            state = STATE.alert;
            goal = Alert;

            victim = Player.instance;
            float distance = GlobalPosition.DistanceTo(victim.GlobalPosition);
            Vector2 dir = GlobalPosition.DirectionTo(victim.GlobalPosition);
            playerCast2D.CastTo = dir*DETECT_DISTANCE;

            if(Mathf.Sign(dir.x)!=facing.x)
            {
                FlipH();
            }

            if (distance > FIGHT_DISTANCE)
            {
                OnAttack(victim);
            }
            else
            {
                OnFight(victim);
            }
     
        }
    }

    private bool CanSeePlayer()
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
        facing=Facing();
    }

    protected override void Navigation(float delta)
    {
        if(direction.Length()>0f)
        {
            travelTime++;
        }
        Vector2 force=new Vector2(FORCE);

        bool left=facing.x==-1f&&state!=STATE.idle;
        bool right=facing.x==1f&&state!=STATE.idle;

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
            velocity=StopX(velocity,delta);

        }

        velocity+=force*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides=GetSlideCount();
        if(velocity.x==0f&&slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                var collision=GetSlideCollision(i);
                if(collision.Collider is Platform platform&&collision.Normal==Vector2.Up)
                {
                    velocity.x=platform.CurrentSpeed.x;
                } else
                {
                    velocity=StopX(velocity,delta);
                }
            }    
        }

        if(IsOnFloor())
        {
            velocity-=GetFloorVelocity()*delta;
        }

        if(IsOnWall()||!rayCast2D.IsColliding())
        {
            if(IsOnFloor())
            {
                FlipH();
            }
        }
    }

}
