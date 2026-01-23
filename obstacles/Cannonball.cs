using System.Collections.Generic;
using Godot;

public class Cannonball : KinematicMonster
{
    [Export] public float MOVE_FORCE=150f;
    [Export] public float MIN_SPEED=20f;
    [Export] public float MAX_SPEED=50f;
    [Export] public Vector2 BOUNCE_FORCE=new Vector2(50f,100f);
    [Export] public bool RANDOM_BOUNCE_FORCE=false;
    [Export] public bool EXPLODE_ON_WALL=false;
    [Export] public int EXPLODE_AFTER_MS=0;

    private Area2D collider;

    public override void _Ready()
    {
        base._Ready();
        staticBody.QueueFree();

        collider=GetNode<Area2D>(nameof(Area2D));
        collider.Connect("body_entered",this,nameof(OnBodyEntered));

        SetSpawnFacing();
        OnStroll();

        if(EXPLODE_AFTER_MS!=0)
        {
            GetTree().CreateTimer(1f).Connect("timeout",this,nameof(OnAttack));

        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        goal(delta);
    }

    protected override void Attack(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides=GetSlideCount();
        if(slides>0)
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
        else
        {
            velocity=StopX(velocity,delta);
        }

        if(animationController.Frame==5)
        {
            CallDeferred("queue_free");
        }

    }    

    protected override void Stroll(float delta)
    {
        Vector2 force=new Vector2(0,GRAVITY);
        if(facing==Vector2.Left&&velocity.x<=MIN_SPEED&&velocity.x>-MAX_SPEED)
        {
            force.x-=MOVE_FORCE;
        }
        else if(facing==Vector2.Right&&velocity.x>=-MIN_SPEED&&velocity.x<MAX_SPEED)
        {
            force.x+=MOVE_FORCE;
        }
        else
        {
            velocity=StopX(velocity,delta);
        }

        bool onFloor=IsOnFloor();
        bool onCeiling=IsOnCeiling();
        bool onWall=IsOnWall();


        if(EXPLODE_ON_WALL&&(onFloor||onCeiling||onWall))
        {
            OnAttack();
            return;
        }

        if(onFloor&&Mathf.Abs(velocity.y)<0.1f)
        {
            if(RANDOM_BOUNCE_FORCE)
            {
                velocity.y=-MathUtils.RandomRange(BOUNCE_FORCE.x,BOUNCE_FORCE.y);
            }
            else
            {
                velocity.y=-BOUNCE_FORCE.y;
            }
            animationController.Rotation=Mathf.Deg2Rad(MathUtils.RandomRange(-45,45));
        }

        if(onWall)
        {
            FlipH();
        }

        velocity+=force*delta;
        velocity=MoveAndSlideWithSnap(velocity,Vector2.Zero,Vector2.Up,false,4,0.785398f,true);
    }

    protected override void OnDamage(Node2D node=null, float amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            Renderer.instance.Shake(1f);
        }
    }

    protected override void OnStroll()
    {
        onDelay=false;
        if(state!=STATE.stroll)
        {
            lastState=state;
            state=STATE.stroll;
            animationController.Play("stroll");
            int dir=spawn_facing==SPAWN_FACING.LEFT?-1:1;
            velocity.x=dir*200f;
            goal=Stroll;
        }
    }

    private void OnAttack()
    {
        OnAttack(null);
    }
    protected override void OnAttack(Player player=null)
    {
        onDelay=false;
        if(state!=STATE.attack)
        {
            lastState=state;
            state=STATE.attack;
            animationController.Play("attack");
            animationController.Rotation=0f;
            goal=Attack;
            if(player!=null)
            {
                player.EmitSignal(STATE.damage.ToString(),this,DAMAGE_AMOUNT);
            }
        }
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        collisionController.Position=FlipX(collisionController.Position);

        facing=Facing();
    }

    public void SetDirection(Vector2 direction)
    {
        if(direction==Vector2.Left)
        {
            spawn_facing=SPAWN_FACING.LEFT;
        }
        else if(direction==Vector2.Right)
        {
            spawn_facing=SPAWN_FACING.RIGHT;
        }
    }

    private void OnBodyEntered(Node body)
    {
        OnAttack(Player.instance);
    }

    public void SetOptions(Dictionary<string,object>options)
    {
        DAMAGE_AMOUNT=(float)options["DAMAGE_AMOUNT"];
        HEALTH=(float)options["HEALTH"];
        GRAVITY=(float)options["GRAVITY"];
        FRICTION=(float)options["FRICTION"];
        STOP_FORCE=(float)options["STOP_FORCE"];
        MOVE_FORCE=(float)options["MOVE_FORCE"];
        MIN_SPEED=(float)options["MIN_SPEED"];
        MAX_SPEED=(float)options["MAX_SPEED"];
        BOUNCE_FORCE=(Vector2)options["BOUNCE_FORCE"];
        RANDOM_BOUNCE_FORCE=(bool)options["RANDOM_BOUNCE_FORCE"];
        EXPLODE_AFTER_MS=(int)options["EXPLODE_AFTER_MS"];
        EXPLODE_ON_WALL=(bool)options["EXPLODE_ON_WALL"];
    }

    public static Dictionary<string,object>GetDefaults()
    {
        return new Dictionary<string,object>
        {
            {"USE_SETTINGS",false},
            {"DAMAGE_AMOUNT",1f},
            {"HEALTH",1f},
            {"GRAVITY",500f},
            {"FRICTION",0.01f},
            {"STOP_FORCE",500f},
            {"MOVE_FORCE",150f},
            {"MIN_SPEED",20f},
            {"MAX_SPEED",50f},
            {"BOUNCE_FORCE",new Vector2(50f,100f)},
            {"RANDOM_BOUNCE_FORCE",false},
            {"EXPLODE_ON_WALL",false},
            {"EXPLODE_AFTER_MS",0}
        };
    }

}
