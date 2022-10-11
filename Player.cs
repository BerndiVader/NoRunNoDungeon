using Godot;
using System;

public class Player : KinematicBody2D
{
    private static String ANIM_RUN="RUN";
    private static String ANIM_JUMP="HIT";

    [Signal]
    public delegate void Damage(float amount=1f, Node2D attacker=null);

    [Export] private float GRAVITY=500f;
    [Export] private float FLOOR_ANGLE_TOLERANCE=40f;
    [Export] private float WALK_FORCE=600f;
    [Export] private float WALK_MIN_SPEED=10f;
    [Export] private float WALK_MAX_SPEED=200f;
    [Export] private float STOP_FORCE=1300f;
    [Export] private float JUMP_SPEED=200f;
    [Export] private float JUMP_MAX_AIRBORNE_TIME=0.2f;
    [Export] private float SLIDE_STOP_VELOCITY=1f;
    [Export] private float SLIDE_STOP_MIN_TRAVEL=1f;
    [Export] private float health=20f;

    private Vector2 velocity=new Vector2(0f,0f);
    private float onAirTime=100f;
    private bool jumping=false;
    private bool doubleJump=false;
    private bool justJumped=false;
    private bool prevJumpPressed=false;
    private int weaponCyle=0;
    private float slopeAngle=0f;
    private Vector2 lastVelocity=new Vector2(0f,0f);

    private AnimatedSprite animationController;
    public CollisionShape2D collisionShape;

    private Weapon weapon;

    public override void _Ready()
    {
        Position=World.instance.renderer.ToLocal(World.instance.level.startingPoint.GlobalPosition);

        collisionShape=GetNode<CollisionShape2D>("CollisionShape2D");
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
        animationController.Play(ANIM_RUN);

        equipWeapon((PackedScene)ResourceUtils.weapons[(int)WEAPONS.DRAGGER]);

        AddToGroup(GROUPS.PLAYERS.ToString());
        ZIndex=2;

        Connect(SIGNALS.Damage.ToString(),this,nameof(onDamaged));
    }

    public override void _PhysicsProcess(float delta)
    {
        if(World.instance.state==Gamestate.SCENE_CHANGED||World.instance.state==Gamestate.RESTART) return;
        Vector2 force=new Vector2(0,GRAVITY);

        bool left=World.instance.input.getLeft();
        bool right=World.instance.input.getRight();
        bool jump=World.instance.input.getJump();
        bool attack=World.instance.input.getAttack();
        bool changeWeapon=World.instance.input.getChange();

        bool stop=true;

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
            if(velocity.x>=-(WALK_MIN_SPEED*0.3f)&&velocity.x<(WALK_MAX_SPEED*0.3f)) 
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

        velocity-=GetFloorVelocity()*delta;
        velocity+=force*delta;

        if(justJumped)
        {
            MoveLocalX(velocity.x*delta);
            MoveLocalY(velocity.y*delta);
            justJumped=false;
        } 
        else 
        {
            Vector2 snap=jumping?new Vector2(0f,0f):new Vector2(0f,4f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

        int collides=GetSlideCount();
        bool onSlope=false;

        if(collides>0&&!jumping)
        {
            for(int i=0;i<collides;i++)
            {
                KinematicCollision2D collision=GetSlideCollision(i);

                if(!onSlope)
                {
                    ulong id=World.instance.level.GetInstanceId();
                    if(collision.ColliderId==id)
                    {
                        slopeAngle=collision.Normal.AngleTo(Vector2.Up);
                        onSlope=Mathf.Abs(slopeAngle)>=0.785298f;
                    }
                }


                Node2D collider=(Node2D)collision.Collider;
                if(collider.GetParent()!=null)
                {
                    collider=(Node2D)collider.GetParent();
                }
                
                if(collider.IsInGroup(GROUPS.ENEMIES.ToString()))
                {
                    if(collision.Normal.AngleTo(Vector2.Up)==0)
                    {
                        velocity.y=-JUMP_SPEED;
                        animationController.Play(ANIM_JUMP);
                        justJumped=jumping=true;
                        collider.EmitSignal("Passanger",this);                            
                    }
                }
            }
        }

        if(IsOnCeiling()) 
        {
            if(lastVelocity.y<-150f) World.instance.renderer.shake+=Mathf.Abs(lastVelocity.y*0.004f);
        }

        if(IsOnFloor())
        {
            onAirTime=0f;
            if(lastVelocity.y>300f) World.instance.renderer.shake+=lastVelocity.y*0.004f;
        }
        
        if(jumping&&velocity.y>0f) 
        {
            animationController.Play(ANIM_RUN);
            doubleJump=jumping=false;
        }

        if(jump&&jumping&&!prevJumpPressed&&!doubleJump) 
        {
            doubleJump=true;
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
        }

        if(onAirTime<JUMP_MAX_AIRBORNE_TIME&&jump&&!prevJumpPressed&&!jumping) 
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
        prevJumpPressed=jump;

        if(Position.y>320f||Position.x<-20f||Position.x>520f) 
        {
            World.instance.CallDeferred(nameof(World.instance.restartGame),true);
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
        World.instance.CallDeferred(nameof(World.instance.restartGame),true);
    }

}
