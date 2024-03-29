using Godot;
using System;

public class DaggerBullet : Area2D
{
    private Vector2 start,end,height,offset=new Vector2(75f,50f);
    private float time;

    public override void _Ready()
    {
        int xDir=Player.instance.animationController.FlipH?-1:1;
        start=Position;
        end=new Vector2(start.x+(offset.x*xDir),start.y+offset.y);
        height=new Vector2(start.x+((end.x-start.x)*0.5f),start.y-30f);
        time=0f;

        Connect("body_entered",this,nameof(onBodyEntered));
        Connect("area_entered",this,nameof(onBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation+=delta*10f;
        Position=step();

        time+=0.023f;
        if (time>2f)
        {
            QueueFree();
        }
    }

    Vector2 step()
    {
        Vector2 q0=start.LinearInterpolate(height,time);
        Vector2 q1=height.LinearInterpolate(end,time);
        return q0.LinearInterpolate(q1,time);
    }

    public void onBodyEntered(Node node)
    {
        if(node.HasUserSignal(STATE.damage.ToString()))
        {
            node.EmitSignal(STATE.damage.ToString(),Player.instance,1f);
        }
        destroy();
    }

    void destroy()
    {
        DaggerMissParticles particles=(DaggerMissParticles)((PackedScene)ResourceUtils.particles[(int)PARTICLES.DAGGERMISS]).Instance();
        particles.Position=World.level.ToLocal(GlobalPosition);
        World.level.AddChild(particles);
        QueueFree();
    }

}
