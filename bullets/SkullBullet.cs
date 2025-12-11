using Godot;
using System;

public class SkullBullet : Area2D
{
    public Vector2 direction=Vector2.Zero;
    [Export] private float speed=100f,liveSpan=50f;
    private Sprite sprite;

    public override void _Ready()
    {
        sprite=GetNode<Sprite>(nameof(Sprite));
        if(direction==Vector2.Right)
        {
            sprite.FlipH=true;
        }

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("area_entered",this,nameof(OnBodyEntered));
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

    public void OnBodyEntered(Node node)
    {
        if(node.Name=="Player")
        {
            node.EmitSignal(STATE.damage.ToString(),this,1f);
        }
        Destroy();
    }

    void Destroy()
    {
        DaggerMissParticles particles=(DaggerMissParticles)((PackedScene)ResourceUtils.particles[(int)PARTICLES.DAGGERMISS]).Instance();
        particles.Texture=sprite.Texture;
        particles.Position=World.level.ToLocal(GlobalPosition);
        particles.GetNode<CPUParticles2D>("Second").QueueFree();
        World.level.AddChild(particles);
        QueueFree();
    }

}
