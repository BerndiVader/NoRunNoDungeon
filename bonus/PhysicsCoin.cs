using Godot;
using System;

public class PhysicsCoin : PhysicsObject
{
    [Export] int VALUE=1;
    public override void _Ready()
    {
        base._Ready();

        GetNode<Area2D>(nameof(Area2D)).Connect("body_entered",this,nameof(OnBodyEntered));
        GetNode<AnimatedSprite>(nameof(AnimatedSprite)).Play("default");
    }

    private void OnBodyEntered(Node body) 
    {
        if(body is Player player)
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            particles.audio.Stream=CoinTakenParticles.sfxSmall;
            World.level.AddChild(particles);
            CallDeferred("queue_free");
            Apply(player);
        }
    }

    public void Apply(Player player)
    {
        player.ApplyCoins(VALUE);
    }
}
