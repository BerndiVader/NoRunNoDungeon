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
        packedParticles=ResourceUtils.particles[(int)PARTICLES.DAGGERMISSPARTICLES] as PackedScene;

        Connect("body_entered",this,nameof(bodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation+=delta*10f;
        Position=step();

        time+=0.023f;
        if (time>2f)
        {
            CallDeferred("queue_free");
        }
    }

    Vector2 step()
    {
        Vector2 q0=start.LinearInterpolate(height,time);
        Vector2 q1=height.LinearInterpolate(end,time);
        return q0.LinearInterpolate(q1,time);
    }

    void bodyEntered(Node node)
    {
        if(node.IsInGroup("Enemies"))
        {
            if(node.GetParent()!=null)
            {
                node.GetParent().EmitSignal("Damage",WorldUtils.world.player,1f);
            }
            else
            {
                node.EmitSignal("Damage",WorldUtils.world.player,1f);                            
            }
        }

        DaggerMissParticles particles=packedParticles.Instance() as DaggerMissParticles;
        particles.Position=WorldUtils.world.level.ToLocal(GlobalPosition);
        WorldUtils.world.level.AddChild(particles);

        CallDeferred("queue_free");
    }

}
