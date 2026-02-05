using Godot;
using System;

public class Fairy : KinematicMonster
{
    [Export]private Vector2 SIN_COS_SPEED=new Vector2(3f,1.5f);
    [Export]private Vector2 FLOAT_RANGE=new Vector2(5f,5f);

    private float passedTime;
    private int projectileCooldown;
    private Vector2 offsetPos;
    private RayCast2D raycast;

    public override void _Ready()
    {
        base._Ready();

        raycast=GetNode<RayCast2D>(nameof(RayCast2D));

        passedTime=0f;
        state=STATE.unknown;
        EmitSignal(STATE.idle.ToString());
        projectileCooldown=0;
    }

    public override void _PhysicsProcess(float delta)
    {
        float lastX=Position.x;
        goal(delta);
        bool direction=Position.x>lastX;
        if(direction!=animationController.FlipH)
        {
            FlipH();
        }
    }

    protected override void Idle(float delta)
    {
        Fly(delta);
    }

    protected override void OnIdle()
    {
        onDelay=false;
        if(state!=STATE.idle)
        {
            lastState=state;
            state=STATE.idle;
            animationController.Play("idle");
            goal=Idle;
            offsetPos=new Vector2(Position);
        }
    }

    private void Fly(float delta)
    {
        passedTime+=delta;
        Position=new Vector2(offsetPos.x+(FLOAT_RANGE.x*Mathf.Sin(passedTime*SIN_COS_SPEED.x)),offsetPos.y+(FLOAT_RANGE.y*Mathf.Cos(passedTime*SIN_COS_SPEED.y)));

        projectileCooldown--;
        projectileCooldown=Mathf.Clamp(projectileCooldown,0,100);

        ShootProjectile();
    }

    private void ShootProjectile()
    {
        Vector2 playerPos=Player.instance.GlobalPosition;
        Vector2 pos=GlobalPosition;

        bool canSeePlayer=animationController.FlipH&&playerPos.x>pos.x||!animationController.FlipH&&playerPos.x<pos.x;

        if(canSeePlayer&&projectileCooldown==0)
        {
            if(playerPos.DistanceSquaredTo(pos)<8000f)
            {
                projectileCooldown=100;
                SkullBullet bullet=ResourceUtils.bullets[(int)BULLETS.TESTBULLET].Instance<SkullBullet>();
                
                bullet.Position=Position;
                bullet.direction=animationController.FlipH?Vector2.Right:Vector2.Left;
                World.level.AddChild(bullet);
            }
        }        
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        raycast.CastTo*=-1;
    }

}
