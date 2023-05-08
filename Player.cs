using Godot;
using System;

public class Player : KinematicBody2D
{
    public static Player instance;
    private static String ANIM_RUN="RUN";
    private static String ANIM_JUMP="HIT";
    public static int LIVES;

    [Signal]
    public delegate void damage(float amount=1f, Node2D attacker=null);

    [Export] public float GRAVITY=700f, WALK_FORCE=1600f, WALK_MIN_SPEED=119f, WALK_MAX_SPEED=119f, STOP_FORCE=1600f, JUMP_SPEED=220f, JUMP_MAX_AIRBORNE_TIME=0.2f;

    private Vector2 velocity=Vector2.Zero;
    private float onAirTime=100f;
    private bool jumping=false;
    private bool doubleJump=false;
    private bool justJumped=false;
    private int weaponCyle=0;
    private float slopeAngle=0f;
    private Vector2 lastVelocity=Vector2.Zero;
    private Vector2 FORCE;
    private Vector2 platformSpeed=Vector2.Zero;
    private float smoothingSpeed;

    private AnimatedSprite animationController;
    public CollisionShape2D collisionShape;
    private CPUParticles2D airParticles,jumpParticles;
    private ShaderMaterial motionTrails;

    private Weapon weapon;

    public Player() : base()
    {
        instance=this;
    }

    public override void _Ready()
    {
        Position=World.instance.renderer.ToLocal(World.level.startingPoint);

        if(ResourceUtils.isMobile)
        {
            GetNode<Light2D>("Light2D").QueueFree();
        }

        collisionShape=GetNode<CollisionShape2D>(nameof(CollisionShape2D));
        animationController=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animationController.Play(ANIM_RUN);

        airParticles=GetNode<CPUParticles2D>(nameof(airParticles));
        airParticles.Emitting=false;
        jumpParticles=ResourceUtils.particles[(int)PARTICLES.JUMP].Instance<CPUParticles2D>();
        AddChild(jumpParticles);
        jumpParticles.Emitting=false;


        equipWeapon((PackedScene)ResourceUtils.weapons[(int)WEAPONS.DRAGGER]);

        AddToGroup(GROUPS.PLAYERS.ToString());
        ZIndex=2;

        Connect(STATE.damage.ToString(),this,nameof(onDamaged));

        FORCE=new Vector2(0f,GRAVITY);

        smoothingSpeed=PlayerCamera.instance.SmoothingSpeed;

        motionTrails=(ShaderMaterial)animationController.Material;
    }

    public override void _PhysicsProcess(float delta)
    {
        Gamestate gamestate=World.instance!=null?World.instance.state:Gamestate.RESTART;
        if((int)gamestate<3)
        {
            return;
        }

        airParticles.InitialVelocity=jumpParticles.InitialVelocity=World.level.Speed;
        motionTrails.SetShaderParam("velocity",velocity);


        float friction=1f;
        if(World.level.Speed!=0f)
        {
            friction=36/World.level.Speed;
        }
        
        Vector2 force=FORCE;

        bool left=World.instance.input.getLeft();
        bool right=World.instance.input.getRight();
        bool jump=World.instance.input.getJump();
        bool attack=World.instance.input.getAttack();
        bool changeWeapon=World.instance.input.getChange();

        if(left)
        {
            PlayerCamera.instance.direction=1;
        }
        else if(right)
        {
            PlayerCamera.instance.direction=-1;
        }
        else
        {
            PlayerCamera.instance.direction=0;
        }

        if(changeWeapon&&!attack)
        {
            weaponCyle++;
            if(weaponCyle>2) 
            {
                weaponCyle=0;
            }
            unequipWeapon();
            equipWeapon(ResourceUtils.weapons[weaponCyle]);
        }

        if(attack&&weapon!=null)
        {
            weapon.attack();
        }

        if(left&&velocity.x<WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
        {
            force.x-=WALK_FORCE;
        } 
        else if(right&&velocity.x>=-(WALK_MIN_SPEED*friction)&&velocity.x<(WALK_MAX_SPEED*friction))
        {
            force.x+=WALK_FORCE;
        }
        else
        {
            float xlength=Mathf.Abs(velocity.x);

            xlength-=STOP_FORCE*delta;

            if(xlength<0f) 
            {
                xlength=0f;
            }
            velocity.x=xlength*Mathf.Sign(velocity.x);
        }

        velocity+=force*delta;
        velocity.x=Mathf.Min(Mathf.Abs(velocity.x),WALK_MAX_SPEED)*Mathf.Sign(velocity.x);

        if(platformSpeed!=Vector2.Zero&&velocity.x==0.0f)
        {
            velocity.x=platformSpeed.x*1.65f*friction;
        }
        platformSpeed=Vector2.Zero;        

        if(justJumped)
        {
            MoveLocalX(velocity.x*delta);
            MoveLocalY(velocity.y*delta);
            justJumped=false;
        } 
        else
        {
            Vector2 snap=jumping?Vector2.Zero:new Vector2(0f,4f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

        int collides=GetSlideCount();
        bool onSlope=false;

        if(collides>0&&!jumping)
        {
            for(int i=0;i<collides;i++)
            {
                KinematicCollision2D collision=GetSlideCollision(i);
                if(collision.ColliderId==World.level.GetInstanceId())
                {
                    slopeAngle=collision.Normal.AngleTo(Vector2.Up);
                    onSlope=Mathf.Abs(slopeAngle)>=0.785298f;
                }
                else if(collision.Collider.HasSignal(STATE.passanger.ToString())&&collision.Normal.AngleTo(Vector2.Up)==0)
                {
                    velocity.y=-JUMP_SPEED;
                    animationController.Play(ANIM_JUMP);
                    justJumped=jumping=true;
                    collision.Collider.EmitSignal(STATE.passanger.ToString(),this);
                }
                else if(collision.Collider is MovingPlatform)
                {
                    MovingPlatform platform=(MovingPlatform)collision.Collider;
                    platformSpeed=platform.CurrentSpeed;
                }
            }
        }

        if(IsOnCeiling()) 
        {
            if(lastVelocity.y<-150f) World.instance.renderer.shake+=Mathf.Abs(lastVelocity.y*0.004f);
        }

        if(IsOnFloor())
        {
            if(airParticles.Emitting)
            {
                airParticles.Emitting=false;
            }

            Vector2 floorVelocity=GetFloorVelocity();
            if(floorVelocity!=Vector2.Zero)
            {
                MoveAndCollide(-floorVelocity*delta);
            }
            onAirTime=0.0f;
            if(lastVelocity.y>300f) 
            {
                World.instance.renderer.shake+=lastVelocity.y*0.004f;
            }
        }
        else if(!airParticles.Emitting)
        {
            airParticles.Emitting=true;
        }
        
        if(jumping&&velocity.y>0f) 
        {
            animationController.Play(ANIM_RUN);
            doubleJump=jumping=false;
            jumpParticles.Emitting=false;
        }

        if(jump&&jumping&&!doubleJump) 
        {
            doubleJump=true;
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
            jumpParticles.Emitting=true;
        }

        if(jump&&!jumping&&onAirTime<JUMP_MAX_AIRBORNE_TIME) 
        {
            if(onSlope) 
            {
                if(slopeAngle<0f) 
                {
                    velocity.x+=100;
                } 
                else if(slopeAngle<1f) 
                {
                    velocity.x-=100;
                }
            }
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
            justJumped=jumping=true;
        }

        onAirTime+=delta;

        if(Position.y>320f||Position.x<-20f||Position.x>520f) 
        {
            onDamaged();
        }

        lastVelocity=velocity;
    }

    public void equipWeapon(PackedScene packed)
    {
        weapon=(Weapon)packed.Instance();
        if(weapon!=null)
        {
            AddChild(weapon);
        }
    }

    public void unequipWeapon()
    {
        RemoveChild(weapon);
        weapon.QueueFree();
    }

    private void onDamaged(float amount=1f,Node2D damager=null)
    {
        PlayerCamera.instance.SmoothingSpeed=0f;
        World.instance.setGamestate(Gamestate.DIEING);

        PlayerDie effect=PlayerDie.create();
        effect.Position=World.level.ToLocal(GlobalPosition);
        Position=new Vector2(0,-100);
        World.level.AddChild(effect);
        LIVES--;
        PlayerDieEffect.create();
    }

    public void die()
    {
        if(LIVES>0)
        {
            World.instance.CallDeferred(nameof(World.instance.restartLevel),true);
            PlayerCamera.instance.SmoothingSpeed=smoothingSpeed;
        }
        else
        {
            World.instance.CallDeferred(nameof(World.changeScene),ResourceUtils.intro);
        }
    }

}
