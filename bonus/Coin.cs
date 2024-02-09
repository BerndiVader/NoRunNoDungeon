using Godot;
using System;

public class Coin : Bonus
{
    public override void _Ready()
    {
        base._Ready();
    }

    protected override void onEnteredBody(Node body) 
    {
        if(body.Name.Equals("Player"))
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            particles.audio.Stream=CoinTakenParticles.sfxBig;
            World.level.AddChild(particles);
            QueueFree();
        }
    }

    public override void apply(Player player)
    {
        throw new NotImplementedException();
    }
}
