using Godot;
using System;
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

    public struct Input
    {
        public bool Left;
        public bool Right;
        public bool Down;
        public bool Up;
        public bool Jump;
        public bool Attack;
        public bool JustAttack;
        public bool Interact;
        public bool JustInteract;
        public bool JustJump;
        public bool JustLeft;
        public bool JustRight;
        public bool JustUp;
        public bool JustDown;

        public void Update()
        {
            Left=World.instance.input.Left();
            Right=World.instance.input.Right();
            Up=World.instance.input.Up();
            Down=World.instance.input.Down();
            JustLeft=World.instance.input.JustLeft();
            JustRight=World.instance.input.JustRight();
            JustUp=World.instance.input.JustUp();
            JustDown=World.instance.input.JustDown();

            Jump=World.instance.input.Jump();
            Attack=World.instance.input.Attack();
            Interact=World.instance.input.Change();
            JustJump=World.instance.input.JustJump();
            JustAttack=World.instance.input.JustAttack();
            JustInteract=World.instance.input.JustChange();
        }
    }

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

    public float SpeedModifier=1f;
    public float JumpModifier=1f;

    private float dashTime=0f;
    private float dashCooldown=0f;
    private int dashDirection=0;
    private float dashLeftTap=-1f;
    private float dashRightTap=-1f;

    private bool onTeleport=false;

    public Input input=new Input();

    private List<WEAPONS>weapons;
    private int coins=0;

    private AnimatedSprite animationController;
    public AnimatedSprite AnimationController=>animationController;
    private CollisionPolygon2D collisionShape;
    public CollisionPolygon2D CollisionShape=>collisionShape;
    private CPUParticles2D airParticles;
    private JumpParticles jumpParticles;
    private ShaderMaterial motionTrails;

    private Weapon weapon=null;
    public static readonly List<WeakReference<Buff>>buffs=new List<WeakReference<Buff>>();

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
        jumpParticles=ResourceUtils.particles[(int)PARTICLES.JUMP].Instance<JumpParticles>();
        AddChild(jumpParticles);
        jumpParticles.Stop();

        weapons=new List<WEAPONS>
        {
            WEAPONS.SWORD
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
        if(World.state<=Gamestate.DIEING||onTeleport)
        {
            return;
        }

        float friction=1f;
        Vector2 levelDirection=World.level.direction;
        if(World.level.speed!=0f&&levelDirection.x!=0f)
        {
            friction=36f/World.level.speed;
        }
        float levelYSpeed=levelDirection.y*World.level.speed;

        if(airParticles.Emitting)
        {
            airParticles.Direction=jumpParticles.Direction=levelDirection;
            airParticles.InitialVelocity=jumpParticles.InitialVelocity=World.level.speed*levelDirection.Length();
        }
        
        Vector2 force=FORCE;
        float slopeAngle=0f;

        input.Update();

        if(input.JustAttack&&weapon!=null)
        {
            weapon.Attack();
        }

        (bool dashLeft,bool dashRight)=UpdateDash();
        HandleDash(dashLeft,dashRight);
        ApplyDash(ref force);

        if(input.Left)
        {
            PlayerCamera.instance.direction=1;
            animationController.FlipH=true;

            if(!dashLeft)
            {
                float maxSpeed=(levelDirection.x>0f)?WALK_MAX_SPEED*friction:WALK_MAX_SPEED;
                maxSpeed*=SpeedModifier;
            
                if(velocity.x>-maxSpeed)
                {
                    force.x-=WALK_FORCE*SpeedModifier;
                }
                else if(dashTime<=0f)
                {
                    velocity.x=-maxSpeed;
                }
            }
            
        }
        else if(input.Right)
        {
            if(!dashRight)
            {
                PlayerCamera.instance.direction=-1;
                animationController.FlipH=false;

                float maxSpeed=(levelDirection.x<0f)?WALK_MAX_SPEED*friction:WALK_MAX_SPEED;
                maxSpeed*=SpeedModifier;

                if(velocity.x<maxSpeed)
                {
                    force.x+=WALK_FORCE*SpeedModifier;
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

            float xlength=Mathf.Abs(velocity.x)-STOP_FORCE*SpeedModifier*delta;
            velocity.x=(xlength>0f?xlength:0f)*Mathf.Sign(velocity.x);
        }

        velocity+=force*delta;
        if(dashDirection==0) velocity.x=Mathf.Min(Mathf.Abs(velocity.x),WALK_MAX_SPEED*SpeedModifier)*Mathf.Sign(velocity.x);

        if(platformSpeed!=Vector2.Zero&&velocity.x==0f)
        {
            velocity.x=platformSpeed.x*1.65f*friction;
        }
        platformSpeed=Vector2.Zero;

        if(justJumped)
        {
            SpawnDust(Dust.TYPE.JUMP,animationController.FlipH);
            Translate(velocity*delta);
            justJumped=false;
        }
        else
        {
            Vector2 snap=jumping?Vector2.Zero:new Vector2(0f,4f);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

        Vector2 motVel=velocity==Vector2.Zero?levelDirection*World.level.speed*-1.4f:(velocity-(levelDirection*World.level.speed))*1.2f;
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
            if(squeezed)
            {
                OnDamaged();
                return;
            }


            for(int i=0;i<collides;i++)
            {
                KinematicCollision2D collision=GetSlideCollision(i);

                slopeAngle=collision.Normal.AngleTo(Vector2.Up);
                float sa=Mathf.Abs(slopeAngle);
                onSlope|=(sa>0.785297f&&sa<1.35f);

                if(!jumping&&slopeAngle==0f&&collision.Collider.HasUserSignal(STATE.passanger.ToString()))
                {
                    velocity.y=-JUMP_SPEED*JumpModifier;
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
            if(input.Down)
            {
                velocity.y*=0.92f;
            }
            if(velocity.y>0f)
            {
                doubleJump=jumping=false;
                jumpParticles.Emitting=false;
            }
            else if(input.JustJump&&!doubleJump)
            {
                doubleJump=true;
                velocity.y=-(JUMP_SPEED*JumpModifier-levelYSpeed);
                jumpParticles.Start(animationController.FlipH);
                Renderer.instance.PlaySfx(sfxDoubleJump,Position);

                SpawnDust(Dust.TYPE.JUMP,animationController.FlipH,true,-1f);
            }
        }
        else if(input.JustJump&&!jumping&&onAirTime<JUMP_MAX_AIRBORNE_TIME*JumpModifier)
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
            velocity.y=-(JUMP_SPEED*JumpModifier-levelYSpeed);
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
                SpawnDust(Dust.TYPE.FALL);
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

            ClearBuffs();

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
            animationController.Play(ANIM_RUN);
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

    private (bool dashLeft,bool dashRight) UpdateDash()
    {
        float max=DASH_COOLDOWN+DASH_DURATION;
        float value=Mathf.Clamp(dashCooldown,0f,max);
        HUD.instance.UpdateDash(100f*(1f-(value/max)));

        bool dashLeft=false;
        bool dashRight=false;

        if(input.JustLeft)
        {
            SpawnDust(Dust.TYPE.RUN,true);
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
        else if(input.JustRight)
        {
            SpawnDust(Dust.TYPE.RUN);
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
        return(dashLeft,dashRight);
    }

    private void HandleDash(bool dashLeft,bool dashRight)
    {
        if(dashLeft&&dashCooldown<=0f&&dashTime<=0f)
        {
            dashDirection=-1;
            dashTime=DASH_DURATION;
            dashCooldown=DASH_COOLDOWN+DASH_DURATION;
            Renderer.instance.PlaySfx(sfxDash,Position);
        }
        else if(dashRight&&dashCooldown<=0f&&dashTime<=0f)
        {
            dashDirection=1;
            dashTime=DASH_DURATION;
            dashCooldown=DASH_COOLDOWN+DASH_DURATION;
            Renderer.instance.PlaySfx(sfxDash,Position);
        }
    }

    private void ApplyDash(ref Vector2 force)
    {
        if(dashTime>0f)
        {
            animationController.FlipH=dashDirection==-1;
            jumpParticles.Start(animationController.FlipH);
            force.x=dashDirection*DASH_FORCE;
            velocity.x=Mathf.Clamp(velocity.x,-DASH_MAX_SPEED,DASH_MAX_SPEED);
            velocity.y=0f;
            dashTime-=GetPhysicsProcessDeltaTime();
        }
        else
        {
            jumpParticles.Emitting=doubleJump;
            dashDirection=0;
        }

        if(dashCooldown>0f)
        {
            dashCooldown-=GetPhysicsProcessDeltaTime();
        }
    }

    private void SpawnDust(Dust.TYPE type,bool flipH=false,bool flipV=false,float offset=1f)
    {
        Dust dust=ResourceUtils.dust.Instance<Dust>();
        dust.Position=World.level.ToLocal(airParticles.GlobalPosition);
        dust.type=type;
        dust.FlipH=flipH;
        dust.FlipV=flipV;
        dust.Offset*=offset;
        World.level.AddChild(dust);        
    }

    public void ApplyCoins(int amount)
    {
        coins+=amount;
        HUD.instance.UpdateCoins(World.instance.overall_coins+coins);
    }

    public int SaveCoins()
    {
        int amount=coins;
        coins=0;
        return amount;
    }

    public int GetCoins()
    {
        return coins;
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

    public void ClearBuffs()
    {
        foreach(WeakReference<Buff>weak in buffs)
        {
            if(weak.TryGetTarget(out Buff target))
            {
                if(IsInstanceValid(target)&&!target.IsQueuedForDeletion()&&target.IsInsideTree())
                {
                    target.CallDeferred("queue_free");
                }
            }
        }
        buffs.Clear();        
    }

    public Buff FindBuff(Buff buff)
    {
        foreach(WeakReference<Buff>weak in buffs)
        {
            if(weak.TryGetTarget(out Buff target))
            {
                if(IsInstanceValid(target)&&target.IsInsideTree()&&target.GetClass()==buff.GetClass())
                {
                    return target;
                }
            }
        }
        return null;
    }

    public void RemoveBuff(WeakReference<Buff>weak)
    {
        buffs.Remove(weak);
    }

}
