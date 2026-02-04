using Godot;
using System;

public class Coin : Bonus
{
    [Export] int VALUE=1;

    public override void _Ready()
    {
        base._Ready();
    }

    protected override void OnEnteredBody(Node body) 
    {
        if(body is Player player)
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            particles.audio.Stream=CoinTakenParticles.sfxBig;
            World.level.AddChild(particles);
            CallDeferred("queue_free");
            Apply(player);
        }
    }

    public override void Apply(Player player)
    {
        player.ApplyCoins(VALUE);
    }
}
