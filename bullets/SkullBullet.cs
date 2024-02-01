using Godot;
using System;

public class SkullBullet : Area2D
{
    public Vector2 direction=Vector2.Zero;
    private float speed=100f,liveSpan=50f;
    private Sprite sprite;

    public override void _Ready()
    {
        sprite=GetNode<Sprite>("Sprite");
        if(direction==Vector2.Right)
        {
            sprite.FlipH=true;
        }

        Connect("body_entered",this,nameof(onBodyEntered));
        Connect("area_entered",this,nameof(onBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        liveSpan-=1f;
        Position+=direction*(speed*delta);
        if(liveSpan<0f)
        {
            QueueFree();
        }
    }

    public void onBodyEntered(Node node)
    {
        if(node.Name=="Player")
        {
            node.EmitSignal(STATE.damage.ToString(),this,1f);
        }
        destroy();
    }

    void destroy()
    {
        DaggerMissParticles particles=(DaggerMissParticles)((PackedScene)ResourceUtils.particles[(int)PARTICLES.DAGGERMISS]).Instance();
        particles.Texture=sprite.Texture;
        particles.Position=World.level.ToLocal(GlobalPosition);
        particles.GetNode<CPUParticles2D>("Second").QueueFree();
        World.level.AddChild(particles);
        QueueFree();
    }

}
