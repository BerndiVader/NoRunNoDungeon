using Godot;
using System;

public class PhysicsCoin : PhysicsObject
{
    public override void _Ready()
    {
        base._Ready();

        GetNode<Area2D>(nameof(Area2D)).Connect("body_entered",this,nameof(onBodyEntered));
        GetNode<AnimatedSprite>(nameof(AnimatedSprite)).Play("default");
    }

    private void onBodyEntered(Node body) 
    {
        if(body.Name.Equals("Player"))
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            particles.audio.Stream=CoinTakenParticles.sfxSmall;
            World.level.AddChild(particles);
            CallDeferred("queue_free");
        }
    }

    public void apply(Player player)
    {
        throw new NotImplementedException();
    }
}
