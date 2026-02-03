using Godot;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    public static Player instance;
    private const string ANIM_RUN="RUN";
    private const string ANIM_JUMP="HIT";
    private const string ANIM_IDLE="IDLE";
    public static int LIVES;

    private static readonly AudioStream sfxJump=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/30_Jump_03.wav");
    private static readonly AudioStream sfxDoubleJump=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/42_Cling_climb_03.wav");
    private static readonly AudioStream sfxLanding=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/45_Landing_01.wav");
    private static readonly AudioStream sfxDash=ResourceLoader.Load<AudioStream>("res://sounds/ingame/15_human_dash_2.wav");

    [Export] private float GRAVITY=700f;
    [Export] private float WALK_FORCE=1600f;
    [Export] private float WALK_MAX_SPEED=119f;
    [Export] private float STOP_FORCE=1600f;
    [Export] private float JUMP_SPEED=220f;
    [Export] private float JUMP_MAX_AIRBORNE_TIME=0.2f;

    [Export] private float DASH_FORCE=3000f;
    [Export] private float DASH_DURATION=0.20f;
    [Export] private float DASH_COOLDOWN=1f;
    [Export] private float DASH_MAX_SPEED=500f;

    [Export] private float DOUBLE_TAP_TIME=300f;

    private Vector2 velocity=Vector2.Zero;
    public Vector2 Velocity=>velocity;
    private float onAirTime=100f;
    private bool jumping=false;
    private bool doubleJump=false;
    private bool justJumped=false;
    private Vector2 lastVelocity=Vector2.Zero;
    private Vector2 lastPosition=Vector2.Zero;
    private Vector2 FORCE;
    private Vector2 platformSpeed=Vector2.Zero;

    private float dashTime=0f;
    private float dasCooldown=0f;
    private int dashDirection=0;
    private float dashLeftTap=-1f;
    private float dashRightTap=-1f;

    private bool onTeleport=false;

    private List<int>weapons;    

    private AnimatedSprite animationController;
    public AnimatedSprite AnimationController=>animationController;
    private CollisionPolygon2D collisionShape;
    public CollisionPolygon2D CollisionShape=>collisionShape;
    private CPUParticles2D airParticles,jumpParticles;
    private ShaderMaterial motionTrails;

    private Weapon weapon=null;

    public Player() : base()
    {
        instance=this;
    }

    public override void _Ready()
    {
        Position=World.instance.renderer.ToLocal(World.level.startingPoint);
        
        if(GameSettings.current.Usage!=Viewport.UsageEnum.Usage3d)
        {
            GetNode<Light2D>(nameof(Light2D)).QueueFree();
        } 
        else if(!GameSettings.current.Light)
        {
            Light2D light=GetNodeOrNull<Light2D>(nameof(Light2D));
            light?.QueueFree();
        }

        collisionShape=GetNode<CollisionPolygon2D>(nameof(CollisionPolygon2D));

        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));

        airParticles=GetNode<CPUParticles2D>(nameof(airParticles));
        airParticles.Emitting=false;
        jumpParticles=ResourceUtils.particles[(int)PARTICLES.JUMP].Instance<CPUParticles2D>();
        AddChild(jumpParticles);
        jumpParticles.Emitting=false;

        weapons=new List<int>
        {
            (int)WEAPONS.SWORD
        };

        AddToGroup(GROUPS.PLAYERS.ToString());
        ZIndex=2;

        AddUserSignal(STATE.damage.ToString());
        Connect(STATE.damage.ToString(),this,nameof(OnDamaged));

        FORCE=new Vector2(0f,GRAVITY);
        motionTrails=(ShaderMaterial)animationController.Material;
        lastPosition=GlobalPosition;
    }

    public override void _PhysicsProcess(float delta)
    {
        if((int)World.state<3||onTeleport)
        {
            return;
        }

        float friction=1f;
        Vector2 levelDirection=World.level.direction;
        if(World.level.Speed!=0f&&levelDirection.x!=0f)
        {
            friction=36f/World.level.Speed;
        }

        float levelYSpeed=levelDirection.y*World.level.Speed;
        airParticles.Direction=jumpParticles.Direction=levelDirection;
        airParticles.InitialVelocity=jumpParticles.InitialVelocity=World.level.Speed*levelDirection.Length();
        
        Vector2 force=FORCE;
        float slopeAngle=0f;

        bool left=World.instance.input.Left();
        bool right=World.instance.input.Right();
        bool jump=World.instance.input.JustJump();
        bool down=World.instance.input.Down();
        bool attack=World.instance.input.JustAttack();
        bool interact=World.instance.input.JustChange();
        bool justLeft=World.instance.input.JustLeft();
        bool justRight=World.instance.input.JustRight();

        bool dashLeft=false;
        bool dashRight=false;

        if(justLeft)
        {
            Dust dust=ResourceUtils.dust.Instance<Dust>();
            dust.Position=World.level.ToLocal(airParticles.GlobalPosition);
            dust.type=Dust.TYPE.RUN;
            dust.FlipH=true;
            World.level.AddChild(dust);

            float now=Time.GetTicksMsec();
            if(dashLeftTap>0f&&now-dashLeftTap<DOUBLE_TAP_TIME)
            {
                dashLeft=true;
                dashLeftTap=-1f;
            }
            else
            {
                dashLeftTap=now;
            }
        }
        else if(justRight)
        {
            Dust dust=ResourceUtils.dust.Instance<Dust>();
            dust.Position=World.level.ToLocal(airParticles.GlobalPosition);
            dust.type=Dust.TYPE.RUN;
            World.level.AddChild(dust);

            float now=Time.GetTicksMsec();
            if(dashRightTap>0f&&now-dashRightTap<DOUBLE_TAP_TIME)
            {
                dashRight=true;
                dashRightTap=-1f;
            }
            else
            {
                dashRightTap=now;
            }
        }

        if(attack&&weapon!=null)
        {
            weapon.Attack();
        }

        if(left)
        {
            PlayerCamera.instance.direction=1;
            animationController.FlipH=true;

            if(dashLeft&&dasCooldown<=0f&&dashTime<=0f)
            {
                dashDirection=-1;
                dashTime=DASH_DURATION;
                dasCooldown=DASH_COOLDOWN+DASH_DURATION;
                Renderer.instance.PlaySfx(sfxDash,Position);
            }
            else
            {
                float maxSpeed=(levelDirection.x>0f)?WALK_MAX_SPEED*friction:WALK_MAX_SPEED;
                if(velocity.x>-maxSpeed)
                {
                    force.x-=WALK_FORCE;
                }
                else if(dashTime<=0f)
                {
                    velocity.x=-maxSpeed;
                }
            }
            
        }
        else if(right)
        {
            if(dashRight&&dasCooldown<=0f&&dashTime<=0f)
            {
                dashDirection=1;
                dashTime=DASH_DURATION;
                dasCooldown=DASH_COOLDOWN+DASH_DURATION;
                Renderer.instance.PlaySfx(sfxDash,Position);
            }
            else
            {
                PlayerCamera.instance.direction=-1;
                animationController.FlipH=false;

                float maxSpeed=(levelDirection.x<0f)?WALK_MAX_SPEED*friction:WALK_MAX_SPEED;
                if(velocity.x<maxSpeed)
                {
                    force.x+=WALK_FORCE;
                }
                else if(dashTime<=0f)
                {
                    velocity.x=maxSpeed;
                }
            }

        }
        else
        {
            PlayerCamera.instance.direction=0;
            if(friction!=1f)
            {
                animationController.FlipH=levelDirection!=Vector2.Left;
            }

            float xlength=Mathf.Abs(velocity.x)-STOP_FORCE*delta;
            velocity.x=(xlength>0f?xlength:0f)*Mathf.Sign(velocity.x);
        }

        if(dashTime>0f)
        {
            animationController.FlipH=dashDirection==-1;
            jumpParticles.Emitting=true;
            force.x=dashDirection*DASH_FORCE;
            velocity.x=Mathf.Clamp(velocity.x,-DASH_MAX_SPEED,DASH_MAX_SPEED);
            velocity.y=0f;
            dashTime-=delta;
        } 
        else
        {
            jumpParticles.Emitting=doubleJump;
            dashDirection=0;
        }

        if(dasCooldown>0f)
        {
            dasCooldown-=delta;
        }

        velocity+=force*delta;
        if(dashDirection==0) velocity.x=Mathf.Min(Mathf.Abs(velocity.x),WALK_MAX_SPEED)*Mathf.Sign(velocity.x);

        if(platformSpeed!=Vector2.Zero&&velocity.x==0f)
        {
            velocity.x=platformSpeed.x*1.65f*friction;
        }
        platformSpeed=Vector2.Zero;        

        if(justJumped)
        {
            Dust dust=ResourceUtils.dust.Instance<Dust>();
            dust.Position=World.level.ToLocal(airParticles.GlobalPosition);
            dust.type=Dust.TYPE.JUMP;
            World.level.AddChild(dust);

            Translate(velocity*delta);
            justJumped=false;
        } 
        else
        {
            Vector2 snap=jumping?Vector2.Zero:new Vector2(0f,4f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

        Vector2 motVel=velocity==Vector2.Zero?levelDirection*World.level.Speed*-1.4f:(velocity-(levelDirection*World.level.Speed))*1.2f;
        motionTrails.SetShaderParam("velocity",motVel);
        motionTrails.SetShaderParam("flip",animationController.FlipH);

        int collides=GetSlideCount();
        bool onSlope=false;
        bool onCeiling=IsOnCeiling();
        bool onFloor=IsOnFloor();

        if(collides>0)
        {
            Vector2 diff=GlobalPosition-lastPosition;
            bool squeezed=Mathf.Abs(velocity.y)>200f&&diff.y==0f;

            for(int i=0;i<collides;i++)
            {
                KinematicCollision2D collision=GetSlideCollision(i);

                if(squeezed)
                {
                    OnDamaged();
                    return;
                }

                slopeAngle=collision.Normal.AngleTo(Vector2.Up);
                float sa=Mathf.Abs(slopeAngle);
                onSlope=sa>0.785297f&&sa<1.35f;

                if(!jumping&&slopeAngle==0f&&collision.Collider.HasUserSignal(STATE.passanger.ToString()))
                {
                    velocity.y=-JUMP_SPEED;
                    justJumped=jumping=true;
                    collision.Collider.EmitSignal(STATE.passanger.ToString(),this);
                }
                
                if(collision.Collider is Platform platform)
                {
                    platformSpeed=platform.CurrentSpeed;
                }
            }

        }

        if(jumping)
        {
            if(down)
            {
                velocity.y*=0.92f;
            }
            if(velocity.y>0f)
            {
                doubleJump=jumping=false;
                jumpParticles.Emitting=false;
            }
            else if(jump&&!doubleJump)
            {
                doubleJump=true;
                velocity.y=-(JUMP_SPEED-levelYSpeed);
                jumpParticles.Emitting=true;
                Renderer.instance.PlaySfx(sfxDoubleJump,Position);
            }
        }
        else if(jump&&!jumping&&onAirTime<JUMP_MAX_AIRBORNE_TIME)
        {
            if(onSlope)
            {
                if(slopeAngle<0f) 
                {
                    velocity.x+=50f;
                } 
                else if(slopeAngle<1f) 
                {
                    velocity.x-=50f;
                }
            }
            velocity.y=-(JUMP_SPEED-levelYSpeed);
            justJumped=jumping=true;
            Renderer.instance.PlaySfx(sfxJump,Position);
        }

        if(onCeiling) 
        {
            if(lastVelocity.y<-150f) Renderer.instance.Shake(Mathf.Abs(lastVelocity.y*0.004f));
        }

        if(onFloor||onSlope)
        {
            if(airParticles.Emitting)
            {
                Dust dust=ResourceUtils.dust.Instance<Dust>();
                dust.type=Dust.TYPE.FALL;
                dust.Position=World.level.ToLocal(airParticles.GlobalPosition);
                World.level.AddChild(dust);
                
                Renderer.instance.PlaySfx(sfxLanding,Position);
                airParticles.Emitting=false;
            }

            if(lastVelocity.y>300f) 
            {
                Renderer.instance.Shake(lastVelocity.y*0.004f);
            }
            onAirTime=0.0f;
        }
        else if(!airParticles.Emitting)
        {
            airParticles.Emitting=true;
        }

        onAirTime+=delta;

        if(Position.x<-20f||Position.y<-60f||Position.x>World.RESOLUTION.x+20f||Position.y>World.RESOLUTION.y+20f)
        {
            OnDamaged();
        }

        UpdateAnimation(onFloor,friction);

        lastPosition=GlobalPosition;
        lastVelocity=velocity;
    }

    public void EquipWeapon(PackedScene packed)
    {
        weapon=packed.Instance<Weapon>();
        if(weapon!=null)
        {
            AddChild(weapon);
        }
    }

    public void UnequipWeapon()
    {
        if(weapon!=null)
        {
            RemoveChild(weapon);
            weapon.QueueFree();
            weapon=null;
        }
    }

    public bool HasWeapon()
    {
        return weapon!=null;
    }

    private void OnDamaged(Node2D damager=null,float amount=1f)
    {
        if(World.state!=Gamestate.DIEING)
        {
            PlayerCamera.instance.SmoothingSpeed=0f;
            World.instance.SetGamestate(Gamestate.DIEING);

            PlayerDie effect=PlayerDie.Create();
            effect.Position=World.level.ToLocal(GlobalPosition);
            effect.flip=animationController.FlipH;
            Position=new Vector2(0f,-100f);
            World.level.AddChild(effect);
            LIVES--;
            PlayerDieEffect.Create();
        }
    }

    public void Die()
    {
        if(LIVES>0)
        {
            World.instance.CallDeferred(nameof(World.instance.RestartLevel),true);
            PlayerCamera.instance.ResetSmoothingSpeed();
        }
        else
        {
            World.instance.CallDeferred(nameof(World.ChangeScene),ResourceUtils.intro);
        }
    }

    private void UpdateAnimation(bool onFloor,float friction)
    {
        if(dashTime>0f)
        {
            animationController.Play("RUN");
        }
        else if(!onFloor&&velocity.y<0f)
        {
            animationController.Play(ANIM_JUMP);
        }
        else if(!onFloor&&velocity.y>0f)
        {
            animationController.Play(ANIM_RUN);
        }
        else if(Mathf.Abs(velocity.x)>5f)
        {
            animationController.Play(ANIM_RUN);
        }
        else
        {
            animationController.Play(friction==1f?ANIM_IDLE:ANIM_RUN);
        }
    }

    public void Teleport(bool teleport)
    {
        onTeleport=teleport;
        Visible=teleport==false;
    }

    public bool Teleport()
    {
        return onTeleport;
    }

}
