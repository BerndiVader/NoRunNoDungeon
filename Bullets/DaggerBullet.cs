using Godot;
using System;

public class DaggerBullet : Area2D
{
    Vector2 start,end,height;
    float time;
    PackedScene packedParticles;
    CollisionShape2D collisionShape2D;

    public override void _Ready()
    {
        start=Position;
        end=new Vector2(start.x+75f,start.y+50f);
        height=new Vector2(start.x+((end.x-start.x)*0.5f),start.y-30f);
        time=0f;

        collisionShape2D=GetNode<CollisionShape2D>("CollisionShape2D");
        packedParticles=(PackedScene)ResourceUtils.particles[(int)PARTICLES.DAGGERMISSPARTICLES];

        Connect("body_entered",this,nameof(onBodyEntered));
        Connect("area_entered",this,nameof(onAreaEntered));
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

    public void onAreaEntered(Area2D area)
    {
        destroy();
    }

    public void onBodyEntered(Node node)
    {
        if(node.GetParent()!=null)
        {
            node=node.GetParent();
        }

        if(node.IsInGroup(GROUPS.ENEMIES.ToString()))
        {
            node.EmitSignal(SIGNALS.Damage.ToString(),World.instance.player,1f);                            
        }

        destroy();
    }

    void destroy()
    {
        DaggerMissParticles particles=(DaggerMissParticles)packedParticles.Instance();
        particles.Position=World.instance.level.ToLocal(GlobalPosition);
        World.instance.level.AddChild(particles);
        QueueFree();
    }

}
