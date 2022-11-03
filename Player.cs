using Godot;
using System;

public class Player : KinematicBody2D
{
    private static String ANIM_RUN="RUN";
    private static String ANIM_JUMP="HIT";
    public static int LIVES;

    [Signal]
    public delegate void damage(float amount=1f, Node2D attacker=null);

    [Export] public float GRAVITY=700f, WALK_FORCE=1600f, WALK_MIN_SPEED=119f, WALK_MAX_SPEED=119f, STOP_FORCE=1600f, JUMP_SPEED=220f, JUMP_MAX_AIRBORNE_TIME=0.2f;

    private Vector2 velocity=new Vector2(0f,0f);
    private float onAirTime=100f;
    private bool jumping=false;
    private bool doubleJump=false;
    private bool justJumped=false;
    private int weaponCyle=0;
    private float slopeAngle=0f;
    private Vector2 lastVelocity=new Vector2(0f,0f);
    private Vector2 FORCE;

    private AnimatedSprite animationController;
    public CollisionShape2D collisionShape;
    private CPUParticles2D jumpParticles;

    private Weapon weapon;

    public override void _Ready()
    {
        Position=World.instance.renderer.ToLocal(World.instance.level.startingPoint.GlobalPosition);

        if(ResourceUtils.isMobile)
        {
            GetNode<Light2D>("Light2D").QueueFree();
        }

        collisionShape=GetNode<CollisionShape2D>("CollisionShape2D");
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
        animationController.Play(ANIM_RUN);

        jumpParticles=GetNode<CPUParticles2D>("JumpParticles");
        jumpParticles.Emitting=false;

        equipWeapon((PackedScene)ResourceUtils.weapons[(int)WEAPONS.DRAGGER]);

        AddToGroup(GROUPS.PLAYERS.ToString());
        ZIndex=2;

        Connect(STATE.damage.ToString(),this,nameof(onDamaged));

        FORCE=new Vector2(0f,GRAVITY);
    }

    public override void _PhysicsProcess(float delta)
    {
        Gamestate gamestate=World.instance.getGamestate();
        if(gamestate==Gamestate.SCENE_CHANGED||gamestate==Gamestate.RESTART)
        {
            return;
        }

        jumpParticles.InitialVelocity=World.instance.level.Speed;

        float friction=1f;
        if(World.instance.level.Speed!=0f)
        {
            friction=36/World.instance.level.Speed;
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
                if(collision.ColliderId==World.instance.level.GetInstanceId())
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
            }
        }

        if(IsOnCeiling()) 
        {
            if(lastVelocity.y<-150f) World.instance.renderer.shake+=Mathf.Abs(lastVelocity.y*0.004f);
        }

        if(IsOnFloor())
        {
            if(jumpParticles.Emitting)
            {
                jumpParticles.Emitting=false;
            };
            Vector2 floorVelocity=GetFloorVelocity();
            if(floorVelocity!=Vector2.Zero)
            {
                MoveAndCollide(-floorVelocity*delta);
            }
            onAirTime=0f;
            if(lastVelocity.y>300f) 
            {
                World.instance.renderer.shake+=lastVelocity.y*0.004f;
            }
        }
        else if(!jumpParticles.Emitting)
        {
            jumpParticles.Emitting=true;
        }
        
        if(jumping&&velocity.y>0f) 
        {
            animationController.Play(ANIM_RUN);
            doubleJump=jumping=false;
        }

        if(jump&&jumping&&!doubleJump) 
        {
            doubleJump=true;
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
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
        LIVES--;
        if(LIVES>0)
        {
            World.instance.CallDeferred(nameof(World.instance.restartGame),true);
        }
        else
        {
            World.instance.CallDeferred(nameof(World.changeScene),ResourceUtils.intro);
        }
    }

}
