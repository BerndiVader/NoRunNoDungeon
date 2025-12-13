using Godot;
using System;

public class DaggerBullet : Area2D
{
    private Vector2 start, end, height; 
    [Export] private Vector2 offset=new Vector2(75f,50f);
    private float time;

    public override void _Ready()
    {
        int xDir=Player.instance.animationController.FlipH?-1:1;
        start=Position;
        end=new Vector2(start.x+(offset.x*xDir),start.y+offset.y);
        height=new Vector2(start.x+((end.x-start.x)*0.5f),start.y-30f);
        time=0f;

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("area_entered",this,nameof(OnBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation+=delta*10f;
        Position=Step();

        time+=0.023f;
        if (time>2f)
        {
            QueueFree();
        }
    }

    Vector2 Step()
    {
        Vector2 q0=start.LinearInterpolate(height,time);
        Vector2 q1=height.LinearInterpolate(end,time);
        return q0.LinearInterpolate(q1,time);
    }

    public void OnBodyEntered(Node node)
    {
        if(node.HasUserSignal(STATE.damage.ToString()))
        {
            node.EmitSignal(STATE.damage.ToString(),Player.instance,1f);
        }
        Destroy();
    }

    void Destroy()
    {
        BulletMiss particles=(BulletMiss)((PackedScene)ResourceUtils.particles[(int)PARTICLES.BULLETMISS]).Instance();
        particles.Position=World.level.ToLocal(GlobalPosition);
        World.level.AddChild(particles);
        QueueFree();
    }

}
