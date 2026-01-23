using Godot;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    public static Player instance;
    private static readonly string ANIM_RUN="RUN";
    private static readonly string ANIM_JUMP="HIT";
    public static int LIVES;

    static readonly AudioStream sfxJump=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/30_Jump_03.wav");
    static readonly AudioStream sfxDoubleJump=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/42_Cling_climb_03.wav");
    static readonly AudioStream sfxLanding=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/45_Landing_01.wav");

    [Export] private float GRAVITY=700f, WALK_FORCE=1600f, WALK_MIN_SPEED=119f, WALK_MAX_SPEED=119f, STOP_FORCE=1600f, JUMP_SPEED=220f, JUMP_MAX_AIRBORNE_TIME=0.2f;

    private Vector2 velocity=Vector2.Zero;
    public Vector2 Velocity=>velocity;
    private float onAirTime=100f;
    private bool jumping=false;
    private bool doubleJump=false;
    private bool justJumped=false;
    private List<int>weapons;
    private Vector2 lastVelocity=Vector2.Zero;
    private Vector2 lastPosition=Vector2.Zero;
    private Vector2 FORCE;
    private Vector2 platformSpeed=Vector2.Zero;
    private float smoothingSpeed;
    private bool onTeleport=false;

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
        animationController.Play(ANIM_RUN);

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
        smoothingSpeed=PlayerCamera.instance.SmoothingSpeed;
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
        if(World.level.Speed!=0f&&World.level.direction.x!=0f)
        {
            friction=36f/World.level.Speed;
        }

        float levelYSpeed=World.level.direction.y*World.level.Speed;
        airParticles.Direction=jumpParticles.Direction=World.level.direction;
        airParticles.InitialVelocity=jumpParticles.InitialVelocity=World.level.Speed*World.level.direction.Length();
        
        Vector2 force=FORCE;
        float slopeAngle=0f;

        bool left=World.instance.input.Left();
        bool right=World.instance.input.Right();
        bool jump=World.instance.input.JustJump();
        bool down=World.instance.input.Down();
        bool attack=World.instance.input.JustAttack();
        bool interact=World.instance.input.JustChange();

        if(attack&&weapon!=null)
        {
            weapon.Attack();
        }

        if(left)
        {
            PlayerCamera.instance.direction=1;
            animationController.FlipH=true;
        }
        else if(right)
        {
            PlayerCamera.instance.direction=-1;
            animationController.FlipH=false;
        }
        else
        {
            PlayerCamera.instance.direction=0;
            if(friction!=1f)
            {
                animationController.FlipH=false;
            }
        }

        Vector2 motVel=velocity==Vector2.Zero?World.level.direction*World.level.Speed*-1.4f:(velocity-(World.level.direction*World.level.Speed))*1.2f;
        motionTrails.SetShaderParam("velocity",motVel);
        motionTrails.SetShaderParam("flip",animationController.FlipH);

        if(left&&velocity.x>-WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        } 
        else if(right&&velocity.x<(WALK_MAX_SPEED*friction))
        {
            force.x+=WALK_FORCE;
        }
        else
        {
            float xlength=Mathf.Abs(velocity.x)-STOP_FORCE*delta;
            velocity.x=(xlength>0f?xlength:0f)*Mathf.Sign(velocity.x);
        }

        velocity+=force*delta;
        velocity.x=Mathf.Min(Mathf.Abs(velocity.x),WALK_MAX_SPEED)*Mathf.Sign(velocity.x);

        if(platformSpeed!=Vector2.Zero&&velocity.x==0f)
        {
            velocity.x=platformSpeed.x*1.65f*friction;
        }
        platformSpeed=Vector2.Zero;        

        if(justJumped)
        {
            Translate(velocity*delta);
            justJumped=false;
        } 
        else
        {
            Vector2 snap=jumping?Vector2.Zero:new Vector2(0f,4f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

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

                if(squeezed&&collision.ColliderId==World.level.GetInstanceId())
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
                    animationController.Play(ANIM_JUMP);
                    justJumped=jumping=true;
                    collision.Collider.EmitSignal(STATE.passanger.ToString(),this);
                }
                
                if(collision.Collider is MovingPlatform platform)
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
                animationController.Play(ANIM_RUN);
                doubleJump=jumping=false;
                jumpParticles.Emitting=false;
            }
            else if(jump&&!doubleJump)
            {
                doubleJump=true;
                velocity.y=-(JUMP_SPEED-levelYSpeed);
                animationController.Play(ANIM_JUMP);
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
            animationController.Play(ANIM_JUMP);
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
                Renderer.instance.PlaySfx(sfxLanding,Position);
                airParticles.Emitting=false;
                animationController.Play(ANIM_RUN);
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
            PlayerCamera.instance.SmoothingSpeed=smoothingSpeed;
        }
        else
        {
            World.instance.CallDeferred(nameof(World.ChangeScene),ResourceUtils.intro);
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
